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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Base;
using BH.oM.Adapter;
using BH.oM.External.TDRepo;
using System.IO;
using BH.oM.Inspection;
using System.Net.Http;
using System.Net;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public bool CreateIssues(IEnumerable<Audit> audits, PushConfig pushConfig)
        {
            // Convert BHoM Audits to 3DRepo issues
            List<BH.oM.External.TDRepo.Issue> issue = audits.Select(a => a.FromBHoM()).ToList();

            // Serialise the object. All property names must have the first letter lowercase for 3DRepo API.
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            string issue_serialised = Newtonsoft.Json.JsonConvert.SerializeObject(issue, serializerSettings);

            // Remove unneded square brackets
            issue_serialised = issue_serialised.Remove(0, 1);
            issue_serialised = issue_serialised.Remove(issue_serialised.Length - 1, 1);
           
            // Endpoint for creating a new issue
            Uri issueEndpoint = null;
            Uri.TryCreate($"{m_host}/{m_teamspace}/{m_modelId}/issues?key={m_userAPIKey}", UriKind.Absolute, out issueEndpoint);

            // POST request
            HttpResponseMessage respMessage;
            string fullResponse = "";
            var httpContent = new StringContent(issue_serialised, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                respMessage = httpClient.PostAsync(issueEndpoint, httpContent).Result;

                // Process response
                fullResponse = respMessage.Content.ReadAsStringAsync().Result;
            }

            if (!respMessage.IsSuccessStatusCode)
                BH.Engine.Reflection.Compute.RecordError($"The server returned a {respMessage.StatusCode} Error:\n" + fullResponse);
    
            return true;
        }
    }
}

