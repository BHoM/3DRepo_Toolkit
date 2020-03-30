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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.TDRepo;
using BH.oM.Base;
using BH.oM.TDRepo;
using RepoFileExporter;
using RepoFileExporter.dataStructures;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter : BHoMAdapter
    {
        public static string WriteBIMFile(List<IObject> objectsToWrite, string directory = null, string fileName = null)
        {
            // --------------------------------------------- //
            //             Directory preparation             //
            // --------------------------------------------- //

            directory = directory ?? Path.Combine("C:\\temp", "BIMFileFormat");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            fileName = fileName ?? Guid.NewGuid().ToString();
            string bimFilePath = Path.Combine(directory, fileName + ".bim");


            BIMDataExporter exporter = new BIMDataExporter();
            var redMat = exporter.AddMaterial(new List<float> { 1f, 0f, 0f, 0f });

            Geometry geometry = new Geometry(
                new List<double> { 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0 },
                new List<uint> { 0, 1, 2, 0, 2, 3 }, null,
                redMat);

            var rootNode = exporter.AddNode("root", -1, null);

            Dictionary<string, RepoVariant> metadata = new Dictionary<string, RepoVariant>();
            metadata.Add("CustomMeta1", RepoVariant.String("value 2"));
            metadata.Add("Area", RepoVariant.Int(1));
            metadata.Add("Boolean Test", RepoVariant.Boolean(true));
            metadata.Add("Double", RepoVariant.Double(1.3242524));

            exporter.AddNode("mesh1", rootNode, null, geometry, metadata);

            exporter.AddNode("mesh2", rootNode,
                new List<float> {
                    1, 0, 0, 2,
                    0, 1, 0, 2,
                    0, 0, 1, 2,
                    0, 0, 0, 1
                },
                geometry);

            exporter.ExportToFile(bimFilePath);

            return bimFilePath;
        }
    }
}
