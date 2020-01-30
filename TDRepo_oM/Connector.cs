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
using System.Net;

namespace BH.oM.TDRepo
{
    class Connector
    {
        internal static void NewRevision(string host, string apiKey, string teamspace, string modelId, string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("file", new MultipartForm.FileParameter(data, filePath, "application/octet-stream"));

            
            string uri = host + "/" + teamspace + "/" + modelId + "/upload?key=" + apiKey;
            Logger.Instance.Log("Posting a new revision at : " + uri);
            // Create request and receive response
            HttpWebResponse webResponse = MultipartForm.MultipartFormDataPost(uri, null, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();
            if (fullResponse.Contains("The remote server returned an error"))
            {
                Logger.Instance.Log("Errored: " + fullResponse);
                throw new Exception(fullResponse);
            }
            
        }
    }
}
