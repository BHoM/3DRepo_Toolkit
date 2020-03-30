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
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace RepoNET
{
    namespace BIMFileExporter
    {
        /// <summary>
        /// Class to handle conversion between NavisWorks properties and JObject
        /// </summary>
        class BIMFileMaterial
        {
            public enum ColorChannel { red = 0, green = 1, blue = 2 }

            private float[] diffuse;
            private float[] specular;
            private float[] emissive;
            private float[] ambient;
            private float transparency;
            private float shininess;

            private const string MATERIAL_CATEGORY_NAME = "Material";
            private const string DIFFUSE_PROPERTY_NAME = "diffuse";
            private const float HASH_DP = 1000.0f;

            public bool isValid = false;

            public BIMFileMaterial(ModelItem mi)
            {
                diffuse = new float[3];
                specular = new float[3];
                emissive = new float[3];
                ambient = new float[3];

                foreach (PropertyCategory oPC in mi.PropertyCategories)
                {
                    string category = oPC.DisplayName;

                    if (category == MATERIAL_CATEGORY_NAME)
                    {
                        var properties = oPC.Properties.ToArray();

                        foreach (DataProperty oDP in properties)
                        {
                            if (oDP.Value.IsDouble)
                            {
                                string[] property = oDP.DisplayName.Split('.');

                                // If property is of length two it has a sub-property and is a colour.
                                if (property.Length == 2)
                                    this.setColor(property[0].ToLower(), property[1].ToLower(), (float)oDP.Value.ToDouble());
                                else
                                    this.setFloatProperty(property[0].ToLower(), (float)oDP.Value.ToDouble());
                            }
                        }

                        isValid = true;
                    }
                }

                // If the model has geometry then set the diffuse material properties to replicate Navisworks 
                // editor colouration.
                if (mi.HasGeometry && ((mi.Geometry.PrimitiveTypes & PrimitiveTypes.Triangles) == PrimitiveTypes.Triangles))
                {                
                    this.setColor(DIFFUSE_PROPERTY_NAME, ColorChannel.red.ToString(), (float)mi.Geometry.PermanentColor.R);
                    this.setColor(DIFFUSE_PROPERTY_NAME, ColorChannel.green.ToString(), (float)mi.Geometry.PermanentColor.G);
                    this.setColor(DIFFUSE_PROPERTY_NAME, ColorChannel.blue.ToString(), (float)mi.Geometry.PermanentColor.B);

                    isValid = true;
                }
            }

            /// <summary>
            /// Set c
            /// </summary>
            /// <param name="propName"></param>
            /// <param name="subProperty"></param>
            /// <param name="value"></param>
            private void setColor(string propName, string subProperty, float value)
            {
                ColorChannel result;
                if (Enum.TryParse(subProperty, out result))
                {
                    if (propName == "diffuse")
                        diffuse[(int)result] = value;
                    else if (propName == "ambient")
                        ambient[(int)result] = value;
                    else if (propName == "specular")
                        specular[(int)result] = value;
                    else if (propName == "emissive")
                        this.emissive[(int)result] = value;
                }
            }

            private void setFloatProperty(string propName, float value)
            {
                if (propName == "transparency")
                    this.transparency = value;
                else if (propName == "shininess")
                    this.shininess = value;
            }

            // taken from http://stackoverflow.com/questions/6899392/generic-hash-function-for-all-stl-containers			
            private int hashCombine<T>(int seed, T v)
            {
                return (int)(seed ^ (v.GetHashCode() + 0x9e3779b9 + (seed << 6) + (seed >> 2)));
            }

            private int hashColor(float[] color)
            {
                int hash = hashCombine<int>(0, (int)(color[0] * HASH_DP));
                hash = hashCombine<int>(hash, (int)(color[1] * HASH_DP));
                hash = hashCombine<int>(hash, (int)(color[2] * HASH_DP));

                return hash;
            }

            public override int GetHashCode()
            {
                int hash = 0;

                hash = hashCombine<int>(hash, hashColor(ambient));
                hash = hashCombine<int>(hash, hashColor(diffuse));
                hash = hashCombine<int>(hash, hashColor(emissive));
                hash = hashCombine<int>(hash, hashColor(specular));

                hash = hashCombine<int>(hash, (int)(transparency * HASH_DP));
                hash = hashCombine<int>(hash, (int)(shininess * HASH_DP));
                
                return hash;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as BIMFileMaterial);
            }

            public bool Equals(BIMFileMaterial obj)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }

            public JObject ToJObject()
            {
                JObject material = new JObject();

                material["diffuse"] = new JArray(diffuse);
                material["ambient"] = new JArray(ambient);
                material["specular"] = new JArray(specular);
                material["emissive"] = new JArray(emissive);

                material["transparency"] = transparency;
                material["shininess"] = shininess;

                return material;
            }
        }
    }
}
