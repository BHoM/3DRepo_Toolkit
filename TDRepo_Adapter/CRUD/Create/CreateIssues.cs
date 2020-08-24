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
using BH.oM.Adapters.TDRepo;
using System.IO;
using BH.oM.Inspection;
using System.Net.Http;
using System.Net;
using BH.Engine.Adapters.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        bool m_MediaPathAlert = true;

        public bool Create(IEnumerable<oM.Inspection.Issue> bhomIssues, Audit parentAudit = null, PushConfig pushConfig = null)
        {
            bool success = true;
            pushConfig = pushConfig ?? new PushConfig();

            string mediaDirectory = pushConfig.MediaDirectory;

            if (m_MediaPathAlert && string.IsNullOrWhiteSpace(mediaDirectory))
            {
                BH.Engine.Reflection.Compute.RecordNote($"Media directory not specified in the `{nameof(PushConfig)}`. This defaults to 'C:\temp\'. " +
                    $"\nTo specify a media directory, insert a `{nameof(PushConfig)}` into this Push component's `{nameof(ActionConfig)}` input.");
                mediaDirectory = @"C:\temp\";

                m_MediaPathAlert = false;
            }

            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            foreach (oM.Inspection.Issue bhomIssue in bhomIssues)
            {
                // Convert BHoM Audits to 3DRepo issues.
                // NOTE: ONLY THE FIRST ISSUE OF THE AUDIT IS CONVERTED. 
                BH.oM.Adapters.TDRepo.Issue issue = bhomIssue.FromBHoM(parentAudit, mediaDirectory);

                // Serialise the object. All property names must have the first letter lowercase for 3DRepo API.
                string issue_serialised = Newtonsoft.Json.JsonConvert.SerializeObject(issue, serializerSettings);

                // Endpoint for creating a new issue
                string issueEndpoint = $"{m_host}/{m_teamspace}/{m_modelId}/issues?key={m_userAPIKey}";

                // POST request
                HttpResponseMessage respMessage;
                string fullResponse = "";
                var httpContent = new StringContent(issue_serialised, Encoding.UTF8, "application/json");
                using (HttpClient httpClient = new HttpClient())
                {
                    respMessage = httpClient.PostAsync(issueEndpoint, httpContent).Result;

                    // Process response
                    fullResponse = respMessage.Content.ReadAsStringAsync().Result;
                }

                // Deserialise the response. 
                serializerSettings.ContractResolver = new IssueContractResolver();
                BH.oM.Adapters.TDRepo.Issue issue_deserial = Newtonsoft.Json.JsonConvert.DeserializeObject<BH.oM.Adapters.TDRepo.Issue>(fullResponse, serializerSettings);

                if (!respMessage.IsSuccessStatusCode)
                {
                    BH.Engine.Reflection.Compute.RecordError($"The server returned a {respMessage.StatusCode} Error:\n" + fullResponse);
                    success = false;
                }

                // Assign the CreatedIssue Id to the Audit.
                // NOTE: ONLY THE FIRST ISSUE OF THE AUDIT IS CREATED (SEE THE CONVERT)
                string issueId = issue_deserial.Id;
                bhomIssue.CustomData[Convert.AdapterIdName] = issueId;

                // Media attachment (resources) requires a separate endpoint
                AttachResources(bhomIssue, issueId, pushConfig);
            }

            return success;
        }
    }
}

