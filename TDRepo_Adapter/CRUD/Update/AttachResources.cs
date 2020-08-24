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
        public bool AttachResources(oM.Inspection.Issue bhomIssue, string tdrepoIssueId, PushConfig pushConfig = null)
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


            // // - The media needs to be attached as a "Resource" of the issue.
            // // - This requires a MultipartFormData request. See https://3drepo.github.io/3drepo.io/#api-Issues-attachResource
            //if (bhomIssue.Media.Count > 1) // actually, let's do this for all resources, including the first one that was used as issue screenshot.
            using (HttpClient httpClient = new HttpClient())
                {
                    string issueResourceEndpoint = $"{m_host}/{m_teamspace}/{m_modelId}/issue/{tdrepoIssueId}/resources?key={m_userAPIKey}";


                    foreach (string mediaPath in bhomIssue.Media)
                    {
                        // Remember that BHoMIssues have media attached as a partial file path.
                        string fullMediaPath = System.IO.Path.Combine(mediaDirectory ?? "C:\\temp\\", bhomIssue.Media.FirstOrDefault());

                        var imageContent = new ByteArrayContent(Compute.ReadToByte(fullMediaPath));
                        imageContent.Headers.ContentType =
                            System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/jpeg");

                        var requestContent = new MultipartFormDataContent();
                        requestContent.Add(imageContent, "image", "image.jpg");

                        // POST request
                        HttpResponseMessage respMessage = httpClient.PostAsync(issueResourceEndpoint, requestContent).Result;

                        // Process response
                        string fullResponse = respMessage.Content.ReadAsStringAsync().Result;

                        if (!respMessage.IsSuccessStatusCode)
                        {
                            BH.Engine.Reflection.Compute.RecordWarning($"While attaching multiple resources (media) for issue `{tdrepoIssueId}` named `{bhomIssue.Name}`," +
                                $"\nthe server returned a {respMessage.StatusCode} error for media {mediaPath}:\n" + fullResponse);
                            success = false;
                        }
                    }
                }


            return success;
        }
    }
}

