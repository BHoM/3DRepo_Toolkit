/*
 *	Copyright (C) 2017 3D Repo Ltd
 *
 *	This program is free software: you can redistribute it and/or modify
 *	it under the terms of the GNU Affero General Public License as
 *	published by the Free Software Foundation, either version 3 of the
 *	License, or (at your option) any later version.
 *
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU Affero General Public License for more details.
 *
 *	You should have received a copy of the GNU Affero General Public License
 *	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *  Author: Timothy R Scully
 */

using Autodesk.Navisworks.Api;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;

using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RepoNET
{
    namespace BIMFileExporter
    {
        /// <summary>
        /// Geometry processor to convert Navisworks callback
        /// </summary>
        class GeometryProcessor
        {
            private List<MemoryStream> geometryBuffers;
            private int bufferCount;
            public int geometryIndex { get; set; }
            private const int MAX_BUFFER_SIZE = 10000000;

            private List<Task> tasks;
            
            /// <summary>
            /// Class to encapsulate output of geometry processor
            /// </summary>
            public class Geometry
            {
                public List<uint> triangleIndices;  // 0 1 2 0 3 4
                public List<double> vertices;       // x y z x y z ...
                public List<float> normals;         // 0 1 0 v1 v2 v3 // normalised

                public List<int> triangleOffsets;   // 135th byte where triangle starts
                public List<int> vertexOffsets;     // 
                public List<int> normalOffsets;
            }

            /// <summary>
            /// Callback geom listener called by Navisworks API
            /// </summary>
            #region InwSimplePrimitivesCB Class
            private class CallbackGeomListener : ComApi.InwSimplePrimitivesCB
            {
                private Dictionary<Tuple<ulong, ulong>, uint> vertexIndices;
                private PointHasher hasher;
                
                private double[] f;
                private Matrix<double> m;
                private Matrix<double> invTrans;

                private Geometry _geom;

                /// <summary>
                /// Point hasher that computes hash to eliminate duplicates vertex/normal pairs.
                /// </summary>
                private class PointHasher
                {
                    private BoundingBox3D _bbox;
                    private double xSize, ySize, zSize;
                    private double minX, minY, minZ;

                    private const float quantSize = 500000;

                    /// <summary>
                    /// Constructor that takes a bounding box of the mesh for normalisation.
                    /// </summary>
                    /// <param name="bbox">Navisworks API BoundingBox3D</param>
                    public PointHasher(BoundingBox3D bbox)
                    {
                        _bbox = bbox;
                        xSize = (bbox.Max.X - bbox.Min.X);
                        ySize = (bbox.Max.Y - bbox.Min.Y);
                        zSize = (bbox.Max.Z - bbox.Min.Z);

                        xSize = (xSize != 0.0f) ? xSize : 1.0f;
                        ySize = (ySize != 0.0f) ? ySize : 1.0f;
                        zSize = (zSize != 0.0f) ? zSize : 1.0f;

                        minX = bbox.Min.X;
                        minY = bbox.Min.Y;
                        minZ = bbox.Min.Z;

                    }

                    /// <summary>
                    /// Returns a tuple of hash based on a given point and normal
                    /// </summary>
                    /// <param name="point">MathNet Numerics Vector\<double\></param>
                    /// <param name="normal">MathNet Numerics Vector\<double\></param>
                    /// <returns></returns>
                    public Tuple<ulong, ulong> GetHashCode(Vector<double> point, Vector<double> normal)
                    {
                        double x_normalized = (point[0] - minX) / xSize;
                        double y_normalized = (point[1] - minY) / ySize;
                        double z_normalized = (point[2] - minZ) / zSize;

                        ulong hash_x = (ulong)(x_normalized * (quantSize - 1));
                        ulong hash_y = (ulong)(y_normalized * (quantSize - 1)) * (ulong)(quantSize);
                        ulong hash_z = (ulong)(z_normalized * (quantSize - 1)) * (ulong)(quantSize) * (ulong)(quantSize);

                        ulong hash = hash_x + hash_y + hash_z;

                        ulong n_hash_x = (ulong)(normal[0] * (quantSize - 1));
                        ulong n_hash_y = (ulong)(normal[1] * (quantSize - 1)) * (ulong)(quantSize);
                        ulong n_hash_z = (ulong)(normal[2] * (quantSize - 1)) * (ulong)(quantSize) * (ulong)(quantSize);

                        ulong n_hash = n_hash_x + n_hash_y + n_hash_z;

                        return new Tuple<ulong, ulong>(hash, n_hash);
                    }
                }

                /// <summary>
                /// Internal function that transforms a Navisworks vertex by the set matrix
                /// </summary>
                /// <param name="vertex"></param>
                /// <returns></returns>
                private Tuple<Vector<double>, Vector<double>> Transform(ComApi.InwSimpleVertex vertex)
                {
                    Array v_arr = (Array)(object)vertex.coord;

                    var v = Vector<double>.Build.Dense(4);

                    v[0] = (float)(v_arr.GetValue(1));
                    v[1] = (float)(v_arr.GetValue(2));
                    v[2] = (float)(v_arr.GetValue(3));
                    v[3] = 1.0;

                    v = m.Multiply(v);

                    Array n_arr = (Array)(object)vertex.normal;

                    var n = Vector<double>.Build.Dense(4);

                    n[0] = (float)(n_arr.GetValue(1));
                    n[1] = (float)(n_arr.GetValue(2));
                    n[2] = (float)(n_arr.GetValue(3));
                    n[3] = 0.0;

                    n = invTrans.Multiply(n);

                    return new Tuple<Vector<double>, Vector<double>>(n, v);
                }

                /// <summary>
                /// Constructor takes the number of vertices and a bounding box for the model.
                /// </summary>
                /// <param name="numVertices">Integer</param>
                /// <param name="bbox">Navisworks API BoundingBox3D</param>
                public CallbackGeomListener(int numVertices, BoundingBox3D bbox, Geometry geom)
                {
                    vertexIndices = new Dictionary<Tuple<ulong, ulong>, uint>();
                    hasher = new PointHasher(bbox);

                    _geom = geom;

                    _geom.triangleIndices = new List<uint>(numVertices);
                    _geom.vertices = new List<double>(numVertices * 3);
                    _geom.normals = new List<float>(numVertices * 3);


                    f = new double[16];
                }

                public void Line(ComApi.InwSimpleVertex v1,
                        ComApi.InwSimpleVertex v2)
                {
                    // TODO
                }

                public void Point(ComApi.InwSimpleVertex v1)
                {
                    // TODO
                }

                public void SnapPoint(ComApi.InwSimpleVertex v1)
                {
                    // TODO
                }

                /// <summary>
                /// Set matrix transformation for future vertex and normal extraction
                /// </summary>
                /// <param name="trans"></param>
                public void SetMatrix(ComApi.InwLTransform3f3 trans)
                {
                    var dataAsArray = (Array)(object)trans.Matrix;

                    dataAsArray.CopyTo(f, 0);

                    m = Matrix<double>.Build.DenseOfColumnMajor(4, 4, f);

                    // Convert inverse transpose for normal transformation
                    invTrans = m.Inverse().Transpose();
                }

                /// <summary>
                /// Navisworks triangle callback that takes three Navisworks vertices
                /// </summary>
                /// <param name="v1"></param>
                /// <param name="v2"></param>
                /// <param name="v3"></param>
                public void Triangle(ComApi.InwSimpleVertex v1,
                        ComApi.InwSimpleVertex v2,
                        ComApi.InwSimpleVertex v3)
                {
                    List<ComApi.InwSimpleVertex> vs = new List<ComApi.InwSimpleVertex>();

                    vs.Add(v1);
                    vs.Add(v2);
                    vs.Add(v3);

                    foreach (ComApi.InwSimpleVertex v in vs)
                    {
                        Tuple<Vector<double>, Vector<double>> nv_trans = Transform(v);

                        var n_trans = nv_trans.Item1;
                        var v_trans = nv_trans.Item2;

                        // Have we seen this vertex/normal pair before ?
                        Tuple<ulong, ulong> hash = hasher.GetHashCode(v_trans, n_trans);

                        if (!vertexIndices.ContainsKey(hash))
                        {
                            vertexIndices[hash] = (uint)(vertexIndices.Keys.Count);

                            _geom.vertices.Add(v_trans[0]);
                            _geom.vertices.Add(v_trans[1]);
                            _geom.vertices.Add(v_trans[2]);

                            _geom.normals.Add((float)n_trans[0]);
                            _geom.normals.Add((float)n_trans[1]);
                            _geom.normals.Add((float)n_trans[2]);

                        }

                        _geom.triangleIndices.Add(vertexIndices[hash]);
                    }
                }
            }
            #endregion

            public Action<GeomOutputState> _outputFunction;

            /// <summary>
            /// Constructor for Geometry processor. Output function writes to disk and should be an Action that
            /// takes a GeomOutputState
            /// </summary>
            /// <param name="outputFunc"></param>
            public GeometryProcessor(Action<GeomOutputState> outputFunc)
            {
                geometryBuffers = new List<MemoryStream>();
                tasks = new List<Task>();

                this._outputFunction = outputFunc;
                this.geometryIndex = 0;
                
                // Initialize first geometry buffer
                geometryBuffers.Add(new MemoryStream());
                bufferCount = 0;
            }
            
            /// <summary>
            /// Output state passed to Action, contains MemoryStream containing geometry data
            /// and index pointing to order.
            /// </summary>
            public class GeomOutputState
            {
                public MemoryStream geometryBuffer;
                public int index;

                public GeomOutputState(MemoryStream _geometryBuffer, int _index)
                {
                    geometryBuffer = _geometryBuffer;
                    index = _index;
                }
            };

            /// <summary>
            /// Internal function to add parallel task for outputting geometry.
            /// </summary>
            private void queueOutput()
            {
                var task = Task.Factory.StartNew(new Action<Object>(o => _outputFunction((GeomOutputState)o)), new GeomOutputState(geometryBuffers[geometryIndex], geometryIndex));
                tasks.Add(task);
            }

            /// <summary>
            /// Add geometry to buffer. Takes an array of type T to write to Memory buffers. It 
            /// returns a list of start and end byte numbers. If the amount of geometry is over
            /// a certain level it will call the output function.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="obj"></param>
            /// <param name="sz"></param>
            /// <returns></returns>
            private List<int> addGeometry<T>(List<T> obj, int sz) where T : struct
            {
                List<int> startEndArray = new List<int>();

                var byteArray = new byte[obj.Count * sz];
                Buffer.BlockCopy(obj.ToArray(), 0, byteArray, 0, byteArray.Length);

                startEndArray.Add(bufferCount);

                geometryBuffers[geometryIndex].Write(byteArray, 0, byteArray.Length);
                bufferCount += byteArray.Length;

                if (bufferCount > (geometryIndex + 1) * MAX_BUFFER_SIZE)
                {
                    queueOutput();

                    geometryBuffers.Add(new MemoryStream());
                    geometryIndex = geometryBuffers.Count - 1;
                }

                startEndArray.Add(bufferCount);

                return startEndArray;
            }

            /// <summary>
            /// Public facing function to process the geometry of a Navisworks ModelItem
            /// </summary>
            /// <param name="mi"></param>
            /// <returns></returns>
            public Geometry process(ModelItem mi)
            {
                Geometry geom = null;

                if (mi.HasGeometry && ((mi.Geometry.PrimitiveTypes & PrimitiveTypes.Triangles) == PrimitiveTypes.Triangles))
                {
                    geom = new Geometry();
                    BoundingBox3D bbox = mi.BoundingBox();

                    CallbackGeomListener callbkListener = new CallbackGeomListener(0, bbox, geom);
                    ComApi.InwOaPath path = ComBridge.ToInwOaPath(mi);

                    foreach (ComApi.InwOaFragment3 frag in path.Fragments())
                    {
                        ComApi.InwLTransform3f3 trans = (ComApi.InwLTransform3f3)frag.GetLocalToWorldMatrix();
                        callbkListener.SetMatrix(trans);

                        ComApi.InwLBox3f fragBBox = (ComApi.InwLBox3f)frag.GetWorldBox();

                        ComApi.InwLPos3f min = (ComApi.InwLPos3f)fragBBox.min_pos;
                        ComApi.InwLPos3f max = (ComApi.InwLPos3f)fragBBox.max_pos;

                        if ((min.data1 >= bbox.Min.X && min.data2 >= bbox.Min.Y && min.data3 >= bbox.Min.Z)
                            && (max.data1 <= bbox.Max.X && max.data2 <= bbox.Max.Y && max.data3 <= bbox.Max.Z))
                        {
                            frag.GenerateSimplePrimitives(ComApi.nwEVertexProperty.eNORMAL, callbkListener);
                        }
                    }

                    
                    geom.triangleOffsets = addGeometry<uint>(geom.triangleIndices, sizeof(uint));
                    geom.vertexOffsets = addGeometry<double>(geom.vertices, sizeof(double));
                    geom.normalOffsets = addGeometry<float>(geom.normals, sizeof(float)); // face normals? 
                }

                return geom;
            }
            
            /// <summary>
            /// Flush remaining geometry to output.
            /// </summary>
            public void flush()
            {
                queueOutput();

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
