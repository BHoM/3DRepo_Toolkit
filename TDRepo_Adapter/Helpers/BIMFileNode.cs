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

using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.Engine.Base;
using BH.Engine.Geometry;

namespace BH.Adapter.TDRepo
{
    /// <summary>
    /// Class for converting between Navisworks and a JSON header
    /// Computes ID using static integer (potentially not thread-safe)
    /// JSON objects
    /// </summary>
    public class BIMFileNode
    {
        private const string STRING_FIELD_PREFIX = "S";
        private const string DATETIME_FIELD_PREFIX = "T";
        private const string DOUBLE_FIELD_PREFIX = "D";
        private const string BOOLEAN_FIELD_PREFIX = "B";
        private const string INTEGER_FIELD_PREFIX = "I";



        private static string[] excludedCategories =
        {
                "Material", "Geometry", "Autodesk Material", "Revit Material"
            };

        private JObject jsonHeader;
        public int id { get; set; }
        private IObject iObject;

        private bool hasGeometry = false;

        /// <summary>
        /// Create a root node.
        /// </summary>
        public BIMFileNode()
        {
            jsonHeader = new JObject();

            jsonHeader["id"] = 0;
            jsonHeader["name"] = "root";

            //jsonHeader["units"] = destUnits.ToString();

            // - Root node boundingbox creation.
            // This is intended to be a bounding box of the whole set of objects exported in the BIM file format.
            // It was formerly used in 3drepo to move everything to origin; currently disabled as it had problems.
            // However, you do need to export A boundinbox for the bouncer code to accept the BIM file.
            jsonHeader["bbox"] = BH.Adapter.TDRepo.Convert.SerialisedBBox(BH.Engine.Geometry.Create.RandomBoundingBox());


            // Transformation matrix.

            //double scaleFactor = UnitConversion.ScaleFactor(srcUnits, destUnits);


            double[] identityMatrix = {
                    1.0,         0.0,          0.0,         0.0,
                    0.0,         1.0,          0.0,         0.0,
                    0.0,         0.0,          1.0,         0.0,
                    0.0,         0.0,          0.0,         1.0
                };

            jsonHeader["transformation"] = new JArray(identityMatrix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="objName"></param>
        /// <param name="mi"></param>
        /// <param name="geom"></param>
        public BIMFileNode(int id, string objName, object metadata, BH.oM.Geometry.Mesh meshRepresentation = null, int parentId = 0)
        {
            // The input object could be an IGeometry, for example a BH.oM.Geometry.Mesh.
            IBHoMObject bhomobject = iObject as IBHoMObject;

            this.iObject = iObject;
            this.id = id;

            jsonHeader = new JObject();

            jsonHeader["parent"] = parentId; // parent should always be the root
            jsonHeader["id"] = this.id;
            jsonHeader["name"] = objName;

            // // - Metadata 

            //var mdObject = getMetadata(parentMI); // extract properties from BHoMObject and serialise them in metadata

            //if (mdObject != null)
            //    jsonHeader["metadata"] = mdObject;

            if (meshRepresentation != null)
            {
                BH.oM.TDRepo.BIMGeom bimGeom = BH.Adapter.TDRepo.Convert.MeshToBIMGeom(meshRepresentation);

                JObject serialisedMesh = BH.Adapter.TDRepo.Convert.SerialiseBIMGeometry(bimGeom);//addGeometry(geom);

                jsonHeader["geometry"] = serialisedMesh;
            }
        }

        /// <summary>
        /// Get internal JObject
        /// </summary>
        /// <returns></returns>
        public JObject ToJObject()
        {
            return jsonHeader;
        }

    }
}

