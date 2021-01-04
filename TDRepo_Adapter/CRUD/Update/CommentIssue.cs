/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
        public bool CommentIssue(oM.Inspection.Issue bhomIssue, string tdrepoIssueId, PushConfig pushConfig = null)
        {
            bool success = true;
            CheckMediaPath(pushConfig);

            foreach (var mediaPath in bhomIssue.Media)
            {
                string mediaFullPath = pushConfig.MediaDirectory + "/" + mediaPath;
                success &= CommentIssue(bhomIssue.Name, tdrepoIssueId, mediaPath, mediaFullPath);
            }

            return success;
        }

        public bool CommentIssue(string issueName, string tdrepoIssueId, string commentText = null, string commentScreenshotFullpath = null)
        {
            bool success = true;

            using (HttpClient httpClient = new HttpClient())
            {
                string issueCommentEndpoint = $"{m_host}/{m_teamspace}/{m_modelId}/issues/{tdrepoIssueId}/comments?key={m_userAPIKey}";

                Dictionary<string, object> commentData = new Dictionary<string, object>();

                if (string.IsNullOrWhiteSpace(commentText) && string.IsNullOrWhiteSpace(commentScreenshotFullpath))
                    return true; // nothing to do.

                if (!string.IsNullOrWhiteSpace(commentText))
                    commentData["comment"] = commentText;

                if (!string.IsNullOrWhiteSpace(commentScreenshotFullpath))
                    commentData["viewpoint"] = new Dictionary<string, string>() { { "screenshot", Compute.ReadToBase64(commentScreenshotFullpath) } };

                string comment_serialised = Newtonsoft.Json.JsonConvert.SerializeObject(commentData);

                // POST request
                HttpResponseMessage respMessage;
                string fullResponse = "";
                var httpContent = new StringContent(comment_serialised, Encoding.UTF8, "application/json");

                respMessage = httpClient.PostAsync(issueCommentEndpoint, httpContent).Result;

                // Process response
                fullResponse = respMessage.Content.ReadAsStringAsync().Result;

                if (respMessage != null && !respMessage.IsSuccessStatusCode)
                {
                    fullResponse = fullResponse.GetResponseBody();

                    BH.Engine.Reflection.Compute.RecordWarning($"While attaching Comments for issue `{tdrepoIssueId}` named `{issueName}`," +
                        $"\nthe server returned a '{respMessage.StatusCode}':\n\t" + fullResponse);
                    success = false;
                }
            }

            return success;
        }

    }
}


