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
using System.ComponentModel;
using System.IO;
using System.Net;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        [Description("Creates a new Revision and commits with the content of a .BIM or .OBJ file.")]
        public bool CommitNewRevision(string filePath)
        {
            bool success;

            try
            {
                NewRevision(m_host, m_userAPIKey, m_teamspace, m_modelId, filePath);
                success = true;
            }
            catch (System.Exception e)
            {
                BH.Engine.Base.Compute.RecordError($"Committing to server failed. Error:\n{e.Message}");
                success = false;
            }

            m_3DRepoMeshesForOBJexport.Clear();
            return success;
        }

        private static void NewRevision(string host, string apiKey, string teamspace, string modelId, string filePath)
        {
            // Read the saved .bim/.obj file
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            // Add the read data to the post request parameters
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("file", new FileParameter(data, filePath, "application/octet-stream"));

            Uri result = null;

            // Endpoint for creating a new revision
            Uri.TryCreate($"{host}/{teamspace}/{modelId}/upload?key={apiKey}", UriKind.Absolute, out result);

            string uri = result.ToString();

            // Create request and receive response
            HttpWebResponse webResponse = MultipartFormDataPost(uri, null, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();

            if (fullResponse.Contains("The remote server returned an error"))
                throw new Exception(fullResponse);
        }
    }
}


