/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
    public static partial class Query
    {
        [Description("Returns the Body text of an HTTP response message's Result.")]
        public static string GetResponseBody(this string respMessageResult)
        {
            string parsedMessage = "";

            bool includeTitle = false;
            if (!includeTitle)
            {
                string startTag = "<title>";
                string endTag = "</title>";
                int startIndex = respMessageResult.IndexOf(startTag) + startTag.Length;
                parsedMessage = respMessageResult.Remove(startIndex, respMessageResult.IndexOf(endTag) - startIndex);
            }

            System.Text.RegularExpressions.Regex oRegex = new System.Text.RegularExpressions.Regex(".*?<body.*?>(.*?)</body>.*?", System.Text.RegularExpressions.RegexOptions.Multiline);
            string htmlTagPattern = "<.*?>";
            parsedMessage = oRegex.Replace(parsedMessage, string.Empty);
            parsedMessage = System.Text.RegularExpressions.Regex.Replace(parsedMessage, htmlTagPattern, string.Empty);
            parsedMessage = System.Text.RegularExpressions.Regex.Replace(parsedMessage, @"^\s+$[\r\n]*", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            parsedMessage = parsedMessage.Replace("&nbsp;", string.Empty);

            return parsedMessage;
        }
    }
}



