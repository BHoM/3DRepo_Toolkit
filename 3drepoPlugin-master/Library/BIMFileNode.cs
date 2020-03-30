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

using System.Linq;
using Newtonsoft.Json.Linq;
using Autodesk.Navisworks.Api;
using static RepoNET.BIMFileExporter.GeometryProcessor;
using System;

namespace RepoNET
{
    namespace BIMFileExporter
    {
        /// <summary>
        /// Class for converting between Navisworks and a JSON header
        /// Computes ID using static integer (potentially not thread-safe)
        /// JSON objects
        /// </summary>
        internal class BIMFileNode
        {
            private const string STRING_FIELD_PREFIX = "S";
            private const string DATETIME_FIELD_PREFIX = "T";
            private const string DOUBLE_FIELD_PREFIX = "D";
            private const string BOOLEAN_FIELD_PREFIX = "B";
            private const string INTEGER_FIELD_PREFIX = "I";
            
            private const string BOUNDING_BOX_MIN_FIELD = "min";
            private const string BOUNDING_BOX_MAX_FIELD = "max";

            private static string[] excludedCategories =
            {
                "Material", "Geometry", "Autodesk Material", "Revit Material"
            };
            
            private JObject jsonHeader;
            public int id { get; set; }
            private ModelItem mi;

            private bool hasGeometry = false;

            /// <summary>
            /// Create a root node.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="rootName"></param>
            /// <param name="mi"></param>
            /// <param name="destUnits"></param>
            public BIMFileNode(int id, string objName, ModelItem mi, Units srcUnits, Units destUnits, BoundingBox3D bbox = null)
            {
                this.mi = mi;
                this.id = id;
                    
                jsonHeader = new JObject();
                
                jsonHeader["id"] = this.id;
                jsonHeader["name"] = objName;

                jsonHeader["units"] = destUnits.ToString();

                jsonHeader["bbox"] = getBoundingBox(bbox);
                        
                double scaleFactor = UnitConversion.ScaleFactor(srcUnits, destUnits);
            
                double[] flip270 = {
                    scaleFactor, 0.0,          0.0,         0.0,
                    0.0,         0.0,          scaleFactor, 0.0,
                    0.0,         -scaleFactor, 0.0,         0.0,
                    0.0,         0.0,          0.0,         1.0
                };
                
                jsonHeader["transformation"] = new JArray(flip270);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="objName"></param>
            /// <param name="mi"></param>
            /// <param name="geom"></param>
            public BIMFileNode(int id, string objName, ModelItem mi, Geometry geom, BIMFileNode parent, ModelItem parentMI = null)
            {
                this.mi = mi;
                this.id = id;

                jsonHeader = new JObject();

                jsonHeader["parent"] = parent.id;
                jsonHeader["id"] = this.id;
                jsonHeader["name"] = parentMI.IsComposite? "" :  objName;

                var mdObject = getMetadata(parentMI);

                if (mdObject != null)
                    jsonHeader["metadata"] = mdObject;

                var gObject = addGeometry(geom);

                if (gObject != null)
                {
                    hasGeometry = true;
                    jsonHeader["geometry"] = gObject;
                }
            }
            
            /// <summary>
            /// Set JObject property based on NavisWorks DataProperty
            /// </summary>
            /// <param name="productCategory"></param>
            /// <param name="oDP"></param>
            /// <returns></returns>
            private static void setProp(JObject metadata, string propName, DataProperty oDP)
            {
                if (oDP == null)
                {
                    throw new System.ArgumentNullException("Invalid data property");
                }
                else
                {
                    if (oDP.Value.IsDisplayString)
                    {
                        propName = STRING_FIELD_PREFIX + propName;
                        metadata[propName] = oDP.Value.ToDisplayString();
                    }
                    else if (oDP.Value.IsDateTime)
                    {
                        propName = DATETIME_FIELD_PREFIX + propName;
                        metadata[propName] = oDP.Value.ToDateTime();
                    }
                    else if (oDP.Value.IsAnyDouble)
                    {
                        propName = DOUBLE_FIELD_PREFIX + propName;

                        if (oDP.Value.IsDouble)
                            metadata[propName] = oDP.Value.ToDouble();
                        else if (oDP.Value.IsDoubleAngle)
                            metadata[propName] = oDP.Value.ToDoubleAngle();
                        else if (oDP.Value.IsDoubleArea)
                            metadata[propName] = oDP.Value.ToDoubleArea();
                        else if (oDP.Value.IsDoubleLength)
                            metadata[propName] = oDP.Value.ToDoubleLength();
                        else if (oDP.Value.IsDoubleVolume)
                            metadata[propName] = oDP.Value.ToDoubleVolume();
                    }
                    else if (oDP.Value.IsBoolean)
                    {
                        propName = BOOLEAN_FIELD_PREFIX + propName;
                        metadata[propName] = oDP.Value.ToBoolean();
                    }
                    else if (oDP.Value.IsIdentifierString)
                    {
                        propName = STRING_FIELD_PREFIX + propName;
                        metadata[propName] = oDP.Value.ToIdentifierString();
                    }
                    else if (oDP.Value.IsInt32)
                    {
                        propName = INTEGER_FIELD_PREFIX + propName;
                        metadata[propName] = oDP.Value.ToInt32();
                    }
                    else
                    {
                        // Unsupported property type
                        // throw new System.NotSupportedException("Invalid data property type");
                    }
                }
            }

            private void addMetdataToObject(ModelItem item, JObject metadata) {
                var productCategories = item.PropertyCategories.ToArray();

                if (productCategories.Length == 0)
                {
                    return;
                }

                foreach (PropertyCategory oPC in productCategories)
                {
                    string category = oPC.DisplayName;

                    if (!excludedCategories.Contains(category))
                    {
                        var properties = oPC.Properties.ToArray();

                        foreach (DataProperty oDP in properties)
                        {
                            string propName = oPC.DisplayName + "." + oDP.DisplayName;

                            setProp(metadata, propName, oDP);
                        }
                    }
                }

            }

            /// <summary>
            /// Get metadata from Navisworks modelItem and create JObject
            /// </summary>
            /// <returns></returns>
            private JObject getMetadata(ModelItem parentMI)
            {
                JObject metadata = new JObject();
                if (parentMI != null)
                    addMetdataToObject(parentMI, metadata);
                addMetdataToObject(mi, metadata);
                return metadata;
            }

            /// <summary>
            /// Return a material object for a particular modelItem
            /// </summary>
            /// <returns></returns>
            private JObject getMaterial()
            {
                BIMFileMaterial meshMaterial = new BIMFileMaterial(mi);
                return meshMaterial.ToJObject();
            }

            public void addMaterialID(int material_id)
            {
                if (hasGeometry)
                {
                    jsonHeader["geometry"]["material"] = material_id;
                }
            }

            /// <summary>
            /// Convert modelItem bounding box to JSON object
            /// </summary>
            /// <returns></returns>
            private JObject getBoundingBox(BoundingBox3D bbox = null)
            {
                if (bbox == null)
                {
                    bbox = mi.BoundingBox();
                }

                JObject bboxJSON = new JObject();
                JArray bboxMin = new JArray();
                bboxMin.Add(bbox.Min.X); // + (center.X - size.X * 0.5));
                bboxMin.Add(bbox.Min.Y); // + (center.Y - size.Y * 0.5));
                bboxMin.Add(bbox.Min.Z); //+ (center.Z - size.Z * 0.5));

                JArray bboxMax = new JArray();
                bboxMax.Add(bbox.Max.X); //+ (center.X - size.X * 0.5));
                bboxMax.Add(bbox.Max.Y); //+ (center.Y - size.Y * 0.5));
                bboxMax.Add(bbox.Max.Z); // + (center.Z - size.Z * 0.5));

                bboxJSON[BOUNDING_BOX_MIN_FIELD] = bboxMin;
                bboxJSON[BOUNDING_BOX_MAX_FIELD] = bboxMax;
                
                return bboxJSON;
            }

            /// <summary>
            /// Add geometry based on information returned from the geometry processor
            /// </summary>
            /// <param name="geom"></param>
            /// <returns></returns>
            private JObject addGeometry(Geometry geom)
            {
                JObject geomObj = null; // new JObject();

                // If we have some triangles
                if (geom != null && geom.triangleIndices.Count > 0)
                {
                    geomObj = new JObject();

                    geomObj["bbox"] = getBoundingBox();

                    geomObj["numIndices"] = geom.triangleIndices.Count;
                    geomObj["numVertices"] = geom.vertices.Count / 3;

                    geomObj["indices"] = new JArray(geom.triangleOffsets); //indicesOffsets; // addGeometry<uint>(geom.triangleIndices, sizeof(uint));
                    geomObj["vertices"] = new JArray(geom.vertexOffsets); // addGeometry<float>(geom.vertices, sizeof(float));
                    geomObj["normals"] = new JArray(geom.normalOffsets); //addGeometry<float>(geom.normals, sizeof(float));
                }

                return geomObj;
            }

            /// <summary>
            /// Store size of next JSON in list for quick reading
            /// </summary>
            public void SetNextSize(long size)
            {
                jsonHeader["nextSize"] = size;
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
}
