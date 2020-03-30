/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.oM.TDRepo;
using System.IO;
using BH.Engine.Geometry;

namespace BH.Adapter.TDRepo
{
    public static partial class Convert
    {
        static List<int> startEndArray = new List<int>();
        static List<MemoryStream> geometryBuffers = new List<MemoryStream>();
        static int bufferCount = 0;
        static int geometryIndex = 0;

        static BIMFileMeta meta;
        static private int counter = 0;
        static private int materialCount = 0;


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BIMGeom MeshToBIMGeom(BH.oM.Geometry.Mesh mesh)
        {
            BIMGeom bimGeom = new BIMGeom();

            // Initialize first geometry buffer
            geometryBuffers.Add(new MemoryStream());

            // Compose the BIMGeom object
            BH.oM.Geometry.Mesh triangulatedMesh = mesh.Triangulate();
            int numVertices = triangulatedMesh.Vertices.Count;

            bimGeom.triangleIndices = new List<uint>();
            foreach (var face in triangulatedMesh.Faces)
            {
                bimGeom.triangleIndices.Add((uint)face.A);
                bimGeom.triangleIndices.Add((uint)face.B);
                bimGeom.triangleIndices.Add((uint)face.C);
            }

            bimGeom.vertices = new List<double>(numVertices * 3);
            bimGeom.normals = new List<float>(numVertices * 3);

            foreach (var vertex in triangulatedMesh.Vertices)
            {
                bimGeom.vertices.Add(vertex.X);
                bimGeom.vertices.Add(vertex.Y);
                bimGeom.vertices.Add(vertex.Z);
            }

            foreach (var normal in BH.Engine.Geometry.Query.Normals(triangulatedMesh))
            {
                bimGeom.normals.Add((float)normal.X);
                bimGeom.normals.Add((float)normal.Y);
                bimGeom.normals.Add((float)normal.Z);
            }


            bimGeom.triangleOffsets = AddGeomToBuffer<uint>(bimGeom.triangleIndices, sizeof(uint));
            bimGeom.vertexOffsets = AddGeomToBuffer<double>(bimGeom.vertices, sizeof(double));
            bimGeom.normalOffsets = AddGeomToBuffer<float>(bimGeom.normals, sizeof(float)); // are these face normals? 


            BoundingBox bbox = BH.Engine.Geometry.Query.IBounds(mesh);
            bimGeom.BoundingBox = bbox;

            return bimGeom;

       

        }

        /***************************************************/


        /// <summary>
        /// Add geometry to buffer. Takes an array of type T to write to Memory buffers. It 
        /// returns a list of start and end byte numbers. If the amount of geometry is over
        /// a certain level it will call the output function.
        /// </summary>
        private static List<int> AddGeomToBuffer<T>(List<T> obj, int sz) where T : struct
        {
            int MAX_BUFFER_SIZE = 10000000;

            var byteArray = new byte[obj.Count * sz];
            Buffer.BlockCopy(obj.ToArray(), 0, byteArray, 0, byteArray.Length);

            startEndArray.Add(bufferCount);

            geometryBuffers[geometryIndex].Write(byteArray, 0, byteArray.Length);
            bufferCount += byteArray.Length;

            if (bufferCount > (geometryIndex + 1) * MAX_BUFFER_SIZE)
            {
                geometryBuffers.Add(new MemoryStream());
                geometryIndex = geometryBuffers.Count - 1;
            }

            startEndArray.Add(bufferCount);

            return startEndArray;
        }

        //public bool export(JsonTextWriter writer, StreamWriter standardWriter)
        //{
        //    meta.headerSize = standardWriter.BaseStream.Length;

        //    standardWriter.Write("{\"nodes\":[");
        //    standardWriter.Flush();

        //    counter = 0;

        //    ModelItem root = this.m.RootItem;

        //    BIMFileNode rootObject = new BIMFileNode(counter, Path.GetFileName(m.FileName), root, "Meters", "Meters", rootBBOX);

        //    Stack<Tuple<ModelItem, BIMFileNode>> stack = new Stack<Tuple<ModelItem, BIMFileNode>>();
        //    ModelItem current;
        //    BIMFileNode parent;

        //    MemoryStream materialStream = new MemoryStream();
        //    StreamWriter materialStreamWriter = new StreamWriter(materialStream);
        //    JsonTextWriter materialWriter = new JsonTextWriter(materialStreamWriter);

        //    List<long> sizes = new List<long>();

        //    // Point at which output begins
        //    long prevLength = standardWriter.BaseStream.Length;

        //    sizes.Add(prevLength);
        //    rootObject.ToJObject().WriteTo(writer);
        //    standardWriter.Flush();

        //    sizes.Add(standardWriter.BaseStream.Length - prevLength);

        //    standardWriter.Write(",");
        //    standardWriter.Flush();

        //    prevLength = standardWriter.BaseStream.Length;

        //    int count = 0;

        //    stack.Push(new Tuple<ModelItem, BIMFileNode>(root, rootObject));

        //    while (stack.Count > 0)
        //    {
        //        Tuple<ModelItem, BIMFileNode> head = stack.Pop();
        //        current = head.Item1;
        //        parent = head.Item2;

        //        count += 1;
        //        progress((float)count / (float)childrenCount);

        //        foreach (ModelItem subMI in current.Children)
        //        {
        //            if (!subMI.IsHidden)
        //            {
        //                string objName = (subMI.DisplayName == "") ? subMI.ClassDisplayName.ToString() : subMI.DisplayName;

        //                stageProgress("Processing " + objName + "[" + count + "]");

        //                if (!subMI.IsInsert)
        //                {
        //                    Geometry geom = gp.process(subMI);

        //                    BIMFileNode bfn = new BIMFileNode(++counter, objName, subMI, geom, parent, current);
        //                    BIMFileMaterial mat = new BIMFileMaterial(subMI);

        //                    if (mat.isValid)
        //                    {
        //                        if (materialMap.ContainsKey(mat))
        //                        {
        //                            bfn.addMaterialID(materialMap[mat]);
        //                        }
        //                        else
        //                        {
        //                            materialMap.Add(mat, materialCount);
        //                            bfn.addMaterialID(materialCount);
        //                            materialCount++;
        //                            mat.ToJObject().WriteTo(materialWriter);
        //                            materialWriter.Flush();
        //                            materialStreamWriter.Write(",");
        //                            materialStreamWriter.Flush();
        //                        }

        //                    }

        //                    bfn.ToJObject().WriteTo(writer);
        //                    standardWriter.Flush();

        //                    // Add count for the commas
        //                    sizes.Add(standardWriter.BaseStream.Length - prevLength);

        //                    standardWriter.Flush();

        //                    standardWriter.Write(",");
        //                    standardWriter.Flush();

        //                    prevLength = standardWriter.BaseStream.Length;
        //                    stack.Push(new Tuple<ModelItem, BIMFileNode>(subMI, bfn));
        //                }
        //                else
        //                {
        //                    stack.Push(new Tuple<ModelItem, BIMFileNode>(subMI, parent));
        //                }
        //            }
        //        }
        //    }
    }
}

