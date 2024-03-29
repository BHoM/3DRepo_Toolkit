/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using System.IO;

namespace BH.Engine.Adapters.TDRepo
{
    public static partial class Compute
    {
        [Description("Reads a file and returns its base64 representation.")]
        public static string ReadToBase64(string filePath, bool enableError = true)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "";

            byte[] imageArray = null;
            string base64Representation = null;

            try
            {
                imageArray = System.IO.File.ReadAllBytes(filePath);
                base64Representation = System.Convert.ToBase64String(imageArray);
            }
            catch (Exception e)
            {
                if (enableError)
                    BH.Engine.Base.Compute.RecordWarning($"Error: {e.Message}.");
            }

            return base64Representation;
        }
    }
}




