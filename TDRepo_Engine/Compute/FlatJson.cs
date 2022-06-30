/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BHG = BH.oM.Geometry;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Adapters.TDRepo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BH.oM.Base.Attributes;

namespace BH.Engine.Adapters.TDRepo
{
    public static partial class Compute
    {
        [Description("Flattens a json string into a Dictionary<string, object>. All the nested values in the json are flattened at the top level of the dictionary.")]
        [Input("json", "Json string with a collection of values.")]
        [Output("A Dictionary with all the values from the json flattened at the same level.")]
        public static Dictionary<string, object> FlattenJsonToDictionary(string json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            JToken token = JToken.Parse(json);
            FillDictionaryFromJToken(dict, token, "");
            return dict;
        }

        private static void FillDictionaryFromJToken(Dictionary<string, object> dict, JToken token, string prefix)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty prop in token.Children<JProperty>())
                    {
                        FillDictionaryFromJToken(dict, prop.Value, Join(prefix, prop.Name));
                    }
                    break;

                case JTokenType.Array:
                    int index = 0;
                    foreach (JToken value in token.Children())
                    {
                        FillDictionaryFromJToken(dict, value, Join(prefix, index.ToString()));
                        index++;
                    }
                    break;

                default:
                    dict.Add(prefix, ((JValue)token).Value);
                    break;
            }
        }

        private static string Join(string prefix, string name)
        {
            return (string.IsNullOrEmpty(prefix) ? name : prefix + "." + name);
        }
    }
}


