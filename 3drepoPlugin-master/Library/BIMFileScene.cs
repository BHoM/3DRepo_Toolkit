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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//Add the namespaces

using System.IO;
using System.IO.Compression;

using Newtonsoft.Json.Linq;
using Autodesk.Navisworks.Api;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;
using static RepoNET.BIMFileExporter.GeometryProcessor;

namespace RepoNET
{
    namespace BIMFileExporter
    {
        /// <summary>
        /// BIM File exporter handling class 
        /// </summary>
        public class BIMFileScene
        {
            private int counter = 0;
            private int materialCount = 0;

            private GeometryProcessor gp;

            private const string TMP_GEOM_EXTENSION = ".geom";
            private const string TMP_HEADER_EXTENSION = ".out";
            private const string BIM_EXTENSION = ".bim";

            private const string FILE_VERSION = "BIM002";

            private BoundingBox3D rootBBOX = null;

            private Stream outputStream;
            private Stream tmpStream;
            private List<Stream> geomStreams;

            private bool inFileMode = true; // Output to file rather than other stream

            // write JSON directly to a file
            private static string _tmpDirectory;
            private Dictionary<BIMFileMaterial, int> materialMap;

            private BIMFileMeta meta;

            private Model m;

            public struct BIMFileMeta
            {
                public long headerSize;
                public long geometrySize;
                public long sizesStart;
                public long sizesSize;
                public long matStart;
                public long matSize;
                public long numChildren;

                public void write(BinaryWriter bin)
                {
                    bin.Write(headerSize);
                    bin.Write(geometrySize);
                    bin.Write(sizesStart);
                    bin.Write(sizesSize);
                    bin.Write(matStart);
                    bin.Write(matSize);
                    bin.Write(numChildren);
                    bin.Flush();
                 }
            }

            /// <summary>
            /// Internal function to write geometry to file, passed to GeometryProcessor 
            /// </summary>
            /// <param name="stateIn"></param>
            private void WriteToFile(Object stateIn)
            {
                GeomOutputState state = (GeomOutputState)stateIn;

                if (inFileMode)
                {
                    var file = Path.Combine(_tmpDirectory, "_" + state.index + TMP_GEOM_EXTENSION);
                    geomStreams.Add(new FileStream(file, FileMode.Create)); //, FileAccess.Write, FileShare.None));
                }
                else
                {
                    geomStreams.Add(new MemoryStream(100));
                }

                var outStream = geomStreams.Last<Stream>();

                BinaryWriter binaryData = new BinaryWriter(outStream);
                binaryData.Write(state.geometryBuffer.ToArray());
                binaryData.Flush();

                state.geometryBuffer.SetLength(0);
                state.geometryBuffer.Close();
            }

            private Action<float> progress;
            private Action<string> stageProgress;

            /// <summary>
            /// Basic progress function to output progress
            /// </summary>
            /// <param name="perc"></param>
            private void consoleProgress(float perc)
            {
                Console.WriteLine((perc * 100.0f) + " % ...");
            }

            /// <summary>
            /// Basic function to output stage of processing
            /// </summary>
            /// <param name="stage"></param>
            private void consoleStageProgress(string stage)
            {
                Console.WriteLine("STAGE " + stage + " ...");
            }

            /// <summary>
            /// Constructor that takes Document to be exported
            /// </summary>
            /// <param name="doc"></param>
            public BIMFileScene(Model m)
            {
                this.m = m;
                progress = consoleProgress;
                stageProgress = consoleStageProgress;
                geomStreams = new List<Stream>();
                gp = new GeometryProcessor(WriteToFile);
                materialMap = new Dictionary<BIMFileMaterial, int>();
            }

            /// <summary>
            /// Constructor that allows the progress and stageProgress Actions to be overidden.
            /// Allows linking to external Navisworks progress etc.
            /// </summary>
            /// <param name="doc"></param>
            /// <param name="_progress"></param>
            /// <param name="_stageProgress"></param>
            public BIMFileScene(Model m, Action<float> _progress, Action<string> _stageProgress)
            {
                this.m = m;
                progress = _progress;
                stageProgress = _stageProgress;
                geomStreams = new List<Stream>();
                gp = new GeometryProcessor(WriteToFile);
                materialMap = new Dictionary<BIMFileMaterial, int>();
            }

            private int countChildren(ModelItem mi)
            {
                Stack<ModelItem> stack = new Stack<ModelItem>();
                ModelItem current;

                int count = 0;

                stack.Push(mi);
                
                while (stack.Count > 0)
                {
                    count += 1;
                    current = stack.Pop();
                    
                    foreach (var child in current.Children)
                    {
                        if (!child.IsHidden)
                        {
                            if (rootBBOX == null)
                            {
                                rootBBOX = child.BoundingBox();
                            }
                            else
                            {
                                if (child.HasGeometry)
                                {
                                    rootBBOX.Extend(child.BoundingBox());
                                }
                            }

                            stack.Push(child);
                        }
                    }
                }

                return count;
            }
            // Output nodes list

            public bool export(JsonTextWriter writer, StreamWriter standardWriter, Units destUnits)
            {
                meta.headerSize = standardWriter.BaseStream.Length;

                standardWriter.Write("{\"nodes\":[");
                standardWriter.Flush();

                counter = 0;
                
                ModelItem root = this.m.RootItem;

                int childrenCount = countChildren(root);
                BIMFileNode rootObject = new BIMFileNode(counter, Path.GetFileName(m.FileName), root, this.m.Units, destUnits, rootBBOX);

                Stack<Tuple<ModelItem, BIMFileNode>> stack = new Stack<Tuple<ModelItem, BIMFileNode>>();
                ModelItem current;
                BIMFileNode parent;

                MemoryStream materialStream = new MemoryStream();
                StreamWriter materialStreamWriter = new StreamWriter(materialStream);
                JsonTextWriter materialWriter = new JsonTextWriter(materialStreamWriter);

                List<long> sizes = new List<long>();

                // Point at which output begins
                long prevLength = standardWriter.BaseStream.Length;

                sizes.Add(prevLength);
                rootObject.ToJObject().WriteTo(writer);
                standardWriter.Flush();

                sizes.Add(standardWriter.BaseStream.Length - prevLength);

                standardWriter.Write(",");
                standardWriter.Flush();

                prevLength = standardWriter.BaseStream.Length;

                int count = 0;

                stack.Push(new Tuple<ModelItem, BIMFileNode>(root, rootObject));

                while (stack.Count > 0)
                {
                    Tuple<ModelItem, BIMFileNode> head = stack.Pop();
                    current = head.Item1;
                    parent = head.Item2;

                    count += 1;
                    progress((float)count / (float)childrenCount);

                    foreach (ModelItem subMI in current.Children)
                    {
                        if (!subMI.IsHidden)
                        {
                            string objName = (subMI.DisplayName == "") ? subMI.ClassDisplayName.ToString() : subMI.DisplayName;

                            stageProgress("Processing " + objName + "[" + count + "]");

                            if (!subMI.IsInsert)
                            {
                                Geometry geom = gp.process(subMI);

                                BIMFileNode bfn = new BIMFileNode(++counter, objName, subMI, geom, parent, current);
                                BIMFileMaterial mat = new BIMFileMaterial(subMI);

                                if (mat.isValid)
                                {
                                    if (materialMap.ContainsKey(mat))
                                    {
                                        bfn.addMaterialID(materialMap[mat]);
                                    }
                                    else
                                    {
                                        materialMap.Add(mat, materialCount);
                                        bfn.addMaterialID(materialCount);
                                        materialCount++;
                                        mat.ToJObject().WriteTo(materialWriter);
                                        materialWriter.Flush();
                                        materialStreamWriter.Write(",");
                                        materialStreamWriter.Flush();
                                    }

                                }

                                bfn.ToJObject().WriteTo(writer);
                                standardWriter.Flush();

                                // Add count for the commas
                                sizes.Add(standardWriter.BaseStream.Length - prevLength);

                                standardWriter.Flush();

                                standardWriter.Write(",");
                                standardWriter.Flush();

                                prevLength = standardWriter.BaseStream.Length;
                                stack.Push(new Tuple<ModelItem, BIMFileNode>(subMI, bfn));
                            }
                            else
                            {
                                stack.Push(new Tuple<ModelItem, BIMFileNode>(subMI, parent));
                            }
                        }
                    }
                }

                int numChildren = sizes.Count - 2;

                standardWriter.BaseStream.Seek(-1, SeekOrigin.Current);
                standardWriter.Write("],\"sizes\":");
                standardWriter.Flush();
                meta.sizesStart = standardWriter.BaseStream.Length;
                new JArray(sizes).WriteTo(writer);
                standardWriter.Flush();
                meta.sizesSize = standardWriter.BaseStream.Length - meta.sizesStart;
                standardWriter.Write(",\"materials\":");
                standardWriter.Flush();
                meta.matStart = standardWriter.BaseStream.Length;
                standardWriter.Write("[");
                standardWriter.Flush();
                materialStream.WriteTo(standardWriter.BaseStream);
                materialStream.Flush();
                standardWriter.BaseStream.Seek(-1, SeekOrigin.Current);
                standardWriter.Write("]");
                standardWriter.Flush();
                meta.matSize = standardWriter.BaseStream.Length - meta.matStart;
                standardWriter.Flush();

                meta.headerSize = standardWriter.BaseStream.Length - meta.headerSize;

                meta.numChildren = numChildren;

                return true;
            }

            /// <summary>
            /// Internal exporting function.
            /// </summary>
            /// <returns>Output filename</returns>
            private string _export(Units destUnits)
            {
                
                string newFileName = null;
                string newGZipFileName = null;

                if (inFileMode)
                {
                    _tmpDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                    if (Directory.Exists(_tmpDirectory))
                    {
                        Directory.Delete(_tmpDirectory, true);
                    }
                    
                    Directory.CreateDirectory(_tmpDirectory);

                    string baseFileName = Path.GetFileNameWithoutExtension(m.FileName);

                    newFileName = Path.Combine(_tmpDirectory, baseFileName + TMP_HEADER_EXTENSION);
                    newGZipFileName = Path.Combine(_tmpDirectory, baseFileName + BIM_EXTENSION);

                    // write JSON directly to a file

                    if (tmpStream == null)
                    {
                        tmpStream = new FileStream(newFileName, FileMode.Create);
                    }
                }
                
                StreamWriter jsonHeader = new StreamWriter(tmpStream);
                BinaryWriter binaryData = new BinaryWriter(tmpStream);
                JsonTextWriter jsonWriter = new JsonTextWriter(jsonHeader);

                // Output file version magic string
                binaryData.Write(System.Text.Encoding.ASCII.GetBytes(FILE_VERSION));

                // Output place holder for header size and geometry
                meta.write(binaryData);

                // Current position in file
                bool success = export(jsonWriter, jsonHeader, destUnits); // recurseStructure(model.RootItem, root, childrenCount, jsonWriter, jsonHeader);
                
                gp.flush();

                meta.geometrySize = tmpStream.Length;

                for (var i = 0; i < gp.geometryIndex + 1; i++)
                {
                    stageProgress("Combining geometry");

                    var geomStream = geomStreams[i];

                    geomStream.Seek(0, SeekOrigin.Begin);
                    geomStream.CopyTo(tmpStream);
                    geomStream.Close();
                }

                meta.geometrySize = tmpStream.Length - meta.geometrySize;

                tmpStream.Seek(FILE_VERSION.Length, SeekOrigin.Begin);

                meta.write(binaryData);

                // Overwrite header and geometry size
                tmpStream.Seek(0, SeekOrigin.Begin);

                if (inFileMode)
                {
                    if (outputStream == null)
                    {
                        outputStream = new FileStream(newGZipFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    }
                }

                // Begin compressing
                using (GZipStream zipFile = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    int read, soFar = 0;
                    byte[] buffer = new byte[16 * 1024];

                    stageProgress("Compressing file");

                    while ((read = tmpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        zipFile.Write(buffer, 0, read);

                        // report progress back
                        soFar += read;
                        progress((float)soFar / tmpStream.Length);
                    }
                }

                tmpStream.Close();

                if (inFileMode)
                {
                    File.Delete(newFileName);    
                }

                return inFileMode ? newGZipFileName : null;
            }

            /// <summary>
            /// Export to a file
            /// </summary>
            /// <returns></returns>
            public string export(Units destUnits)
            {
                inFileMode = true;
                outputStream = null;

                return _export(destUnits);
            }

            /// <summary>
            /// Export to another memory stream
            /// </summary>
            /// <param name="m"></param>
            /// <returns></returns>
            public string export(MemoryStream ms, Units destUnits)
            {
                inFileMode = false;

                outputStream = ms;
                tmpStream = new MemoryStream(100);

                return _export(destUnits);
            }
        }
    }
}