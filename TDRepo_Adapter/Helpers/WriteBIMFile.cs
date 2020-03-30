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
using Newtonsoft.Json;
using BH.Engine.Geometry;
using BH.oM.Base;
using Newtonsoft.Json.Linq;
using BH.Adapter.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        private const string TMP_GEOM_EXTENSION = ".geom";
        private const string TMP_HEADER_EXTENSION = ".out";
        private const string BIM_EXTENSION = ".bim";
        private const string FILE_VERSION = "BIM002";


        public static void WriteBIMFile(List<IObject> iobjects, string directory = null, string fileName = null)
        {
            Stream outputStream;
            Stream tmpStream = null;
            List<Stream> geomStreams;

            directory = directory ?? Path.Combine(Path.GetTempPath(), "BIMFileFormat");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            fileName = fileName ?? Guid.NewGuid().ToString();
            string filePath = Path.Combine(directory, fileName + TMP_HEADER_EXTENSION);
            string newGZipFileName = Path.Combine(directory, fileName + BIM_EXTENSION);

            // write JSON directly to a file
            tmpStream = new FileStream(filePath, FileMode.Create);

            StreamWriter jsonHeader = new StreamWriter(tmpStream);
            BinaryWriter binaryData = new BinaryWriter(tmpStream);
            JsonTextWriter jsonWriter = new JsonTextWriter(jsonHeader);

            // Output file version magic string
            binaryData.Write(System.Text.Encoding.ASCII.GetBytes(FILE_VERSION));

            // Output place holder for header size and geometry
            BIMFileMeta meta = new BIMFileMeta();
            meta.write(binaryData);

            // Current position in file
            bool success = export(iobjects, jsonWriter, jsonHeader, meta);
        }

        private static bool export(List<IObject> iobjects, JsonTextWriter writer, StreamWriter standardWriter, BIMFileMeta meta)
        {
            meta.headerSize = standardWriter.BaseStream.Length;

            standardWriter.Write("{\"nodes\":[");
            standardWriter.Flush();

            int counter = 0;
            int materialCount = 0;
            int objectCount = iobjects.Count;

            BIMFileNode rootObject = new BIMFileNode();

            //MemoryStream materialStream = new MemoryStream();
            //StreamWriter materialStreamWriter = new StreamWriter(materialStream);
            //JsonTextWriter materialWriter = new JsonTextWriter(materialStreamWriter);

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

            for (int i = 0; i < iobjects.Count; i++)
            {
                IObject iObject = iobjects[i];
                IBHoMObject bhomobject = iObject as IBHoMObject;
                BH.oM.Geometry.Mesh meshRepresentation = null;

                string objName = !string.IsNullOrWhiteSpace(bhomobject?.Name) ? bhomobject.Name : iObject.GetType().Name;

                // // - Geometry 
                // 1. If the input object is not a Geometry, extract BHoM Mesh representation
                // 2. Convert BHoM mesh to BIM Geometry.
                // 3. Serialise BIM geometry

                if (bhomobject != null)
                {
                    // Get mesh representation. Reference stuff done for Speckle_Toolkit.

                    //IGeometry geometry = bhomobject.IGeometry();
                    //BoundingBox bbox = null;
                    //if (geometry != null)
                    //    bbox = BH.Engine.Geometry.Query.IBounds(geometry);
                }

                // If all else has failed, check if the input object is itself a mesh
                if (meshRepresentation == null)
                    meshRepresentation = iObject as BH.oM.Geometry.Mesh;

                BIMFileNode bfn = new BIMFileNode(i + 1, objName, null, meshRepresentation);

                bfn.ToJObject().WriteTo(writer);
                standardWriter.Flush();

                // Add count for the commas
                sizes.Add(standardWriter.BaseStream.Length - prevLength);
                standardWriter.Flush();

                standardWriter.Write(",");
                standardWriter.Flush();

                prevLength = standardWriter.BaseStream.Length;
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
            //materialStream.WriteTo(standardWriter.BaseStream);
            //materialStream.Flush();
            standardWriter.BaseStream.Seek(-1, SeekOrigin.Current);
            standardWriter.Write("]");
            standardWriter.Flush();
            meta.matSize = standardWriter.BaseStream.Length - meta.matStart;
            standardWriter.Flush();

            meta.headerSize = standardWriter.BaseStream.Length - meta.headerSize;

            meta.numChildren = numChildren;

            return true;
        }

    }
}

