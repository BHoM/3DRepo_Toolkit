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
using BH.oM.Base;
using BH.oM.Adapter;
using BH.oM.Adapters.TDRepo;
using System.IO;
using BH.oM.Inspection;
using System.Net.Http;
using System.Net;
using BH.Engine.Adapters.TDRepo;
using System.Net.Http.Headers;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public bool AttachResources(oM.Inspection.Issue bhomIssue, string tdrepoIssueId, PushConfig pushConfig, bool raiseErrors = true)
        {
            bool success = true;

            if (!bhomIssue.Media?.Any() ?? true)
                return true;

            CheckMediaPath(pushConfig);

            // // - The media needs to be attached as a "Resource" of the issue.
            // // - This requires a MultipartFormData request. See https://3drepo.github.io/3drepo.io/#api-Issues-attachResource
            using (HttpClient httpClient = new HttpClient())
            {
                string issueResourceEndpoint = $"{m_host}/{m_teamspace}/{m_modelId}/issues/{tdrepoIssueId}/resources?key={m_userAPIKey}";

                foreach (string mediaPath in bhomIssue.Media)
                {
                    // Remember that BHoMIssues have media attached as a partial file path.
                    string fullMediaPath = System.IO.Path.Combine(pushConfig.MediaDirectory ?? "C:\\temp\\", mediaPath);
                    var f = System.IO.File.OpenRead(fullMediaPath);

                    StreamContent imageContent = new StreamContent(f);
                    MultipartFormDataContent mpcontent = new MultipartFormDataContent();
                    mpcontent.Add(imageContent, "file", mediaPath);
                    StringContent nameContent = new StringContent(Path.GetFileNameWithoutExtension(mediaPath));
                    mpcontent.Add(nameContent, "names");

                    // POST request
                    HttpResponseMessage respMessage = null;
                    try
                    {
                        respMessage = httpClient.PostAsync(issueResourceEndpoint, mpcontent).Result;
                    }
                    catch (AggregateException e)
                    {
                        string errors = $"Error(s) attaching multiple resources (media) for issue `{tdrepoIssueId}` named `{bhomIssue.Name}`:";

                        foreach (var innerException in e.Flatten().InnerExceptions)
                            errors += $"\n\t{innerException.Message}\n{innerException.InnerException}";

                        if (raiseErrors)
                            BH.Engine.Base.Compute.RecordError(errors);

                        return false;
                    }
                    finally
                    {
                        f.Close();
                    }

                    // Process response
                    string fullResponse = respMessage?.Content.ReadAsStringAsync().Result;

                    if (respMessage != null && !respMessage.IsSuccessStatusCode)
                    {
                        fullResponse = fullResponse.GetResponseBody();

                        BH.Engine.Base.Compute.RecordWarning($"While attaching multiple resources (media) for issue `{tdrepoIssueId}` named `{bhomIssue.Name}`," +
                            $"\nthe server returned a '{respMessage.StatusCode}' error for media `{mediaPath}`:\n ==>" + fullResponse);
                        success = false;
                    }
                }
            }

            return success;
        }
    }
}





