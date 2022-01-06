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
using BH.oM.Reflection.Attributes;
using System.IO;

namespace BH.Engine.Adapters.TDRepo
{
    public static partial class Compute
    {
        [Description("Writes a Base64 String into a file.")]
        public static bool WriteToByteArray(string base64string, string fileFullPath, bool enableError = true)
        {
            if (string.IsNullOrWhiteSpace(fileFullPath))
                return false;

            byte[] imageArray = new byte[] { };

            try
            {
                imageArray = System.Convert.FromBase64String(base64string);
            }
            catch (Exception e)
            {
                if (enableError)
                    throw e;
            }

            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));
                System.IO.File.WriteAllBytes(fileFullPath, imageArray);

            }
            catch (Exception e)
            {
                if (enableError)
                    throw e;
            }

            return true;
        }
    }
}


