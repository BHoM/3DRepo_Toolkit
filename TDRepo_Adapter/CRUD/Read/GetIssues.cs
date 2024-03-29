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
using BH.oM.Adapters.TDRepo.Requests;
using System.Net.Http;
using Newtonsoft.Json.Serialization;
using BH.Engine.Adapters.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public List<oM.Inspection.Issue> GetIssues(IssueRequest ir, PullConfig pullConfig, bool enableMessages = true, Dictionary<Issue, List<string>> mediaFileNames = null)
        {
            List<Issue> allIssues = new List<Issue>();
            List<oM.Inspection.Issue> allBHoMIssues = new List<oM.Inspection.Issue>();

            string modelId = ir?.ModelId ?? m_modelId;
            string teamsSpace = ir?.TeamSpace ?? m_teamspace;
            string userAPIkey = "";

            if (!string.IsNullOrWhiteSpace(ir.UserAPIKey))
                userAPIkey = ir.UserAPIKey;
            else if (!string.IsNullOrWhiteSpace(m_userAPIKey))
                userAPIkey += m_userAPIKey;


            bool singleIssue = !string.IsNullOrWhiteSpace(ir.IssueId);
            if (singleIssue)
            {
                // Get the Issue corresponding to the specified IssueId. 

                if (enableMessages)
                    BH.Engine.Base.Compute.RecordNote($"Getting {nameof(Issue)} with id {ir.IssueId} \nfrom the 3DRepo Model `{modelId}` in the Teamspace `{teamsSpace}`.");

                Issue issue = GetIssue(ir.IssueId, teamsSpace, modelId, userAPIkey);

                allIssues.Add(issue);
            }
            else
            {
                // If no IssueId is specified, get *all the issues* attached to a specific Revision.
                // (In 3DRepo, the issues are attached to a specific Model's Revision.)

                // If no RevisionId is specified in the Request, consider the latest revision (First()).
                string revisionId = ir.RevisionId ?? GetRevisions(new RevisionRequest(), false).First().Id;

                if (enableMessages)
                    BH.Engine.Base.Compute.RecordNote($"Getting all of the {nameof(Issue)}s attached to RevisionId {revisionId} \nfrom the 3DRepo Model `{modelId}` in the Teamspace `{teamsSpace}`.");

                allIssues = GetAllIssues(teamsSpace, modelId, revisionId, userAPIkey);
            }

            if (pullConfig.DownloadResources)
            {
                // Attempt the pull of the resources.
                foreach (Issue issue in allIssues)
                {
                    // Dictionary whose Key is filename (fullPath), Value is the base64 string representation.
                    Dictionary<string, string> base64resources = new Dictionary<string, string>();

                    // Get all resources files. https://3drepo.github.io/3drepo.io/#api-Resources-getResource
                    foreach (var resource in issue.Resources)
                    {
                        string endpoint = $"https://api1.www.3drepo.io/api/{teamsSpace}/{modelId}/resources/{resource._id}?key={userAPIkey}";
                        // GET request
                        HttpResponseMessage respMessage;
                        byte[] fullResponse = null; 
                        using (var httpClient = new HttpClient())
                        {
                            respMessage = httpClient.GetAsync(endpoint).Result;

                            // Process response
                            fullResponse = respMessage.Content.ReadAsByteArrayAsync().Result;
                            string base64 = System.Convert.ToBase64String(fullResponse);
                            base64resources.Add(Path.Combine(pullConfig.ResourceDownloadDirectory, resource.name), base64);

                        }
                    }

                    // Get resources (image) attached in the Comments, if any. (TODO)
                    foreach (var comment in issue?.Comments)
                    {
                        var screenshot = comment.viewpoint?.Screenshot;
                        if (screenshot != null)
                            base64resources.Add(Path.Combine(pullConfig.ResourceDownloadDirectory, comment.comment), screenshot);

                        // TODO: Apparently, the actual image is not pulled (`Comments` property is empty) when using the GET Issue endpoint.
                        // There is another endpoint/way to pull the image that was posted in the Comments:
                        // `getViewpoint` endpoint is to be used to get image from the Viewpoint. https://3drepo.github.io/3drepo.io/#api-Viewpoints-findViewpoint
                    }

                    // Save the pulled resource to file. If the base64 string is missing, it will simply create an empty file.
                    base64resources.ToList().ForEach(kv => Compute.WriteToByteArray(kv.Value, kv.Key, false));

                    // Store the pulled images Filenames in the resources. 
                    // This allows to pass this information to the Convert method, see the Pull.
                    if (mediaFileNames != null)
                        mediaFileNames[issue] = issue.Comments.Select(c => c.comment).ToList();
                }
            }

            foreach (Issue issue in allIssues)
            {
                List<string> fileNames = new List<string>();
                if (issue.Resources != null)
                {
                    fileNames = issue.Resources.Select(x => x.name).ToList();
                }

                oM.Inspection.Issue bhomIssue = issue.FromTDRepo(fileNames);
                allBHoMIssues.Add(bhomIssue);
            }

            return allBHoMIssues;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        // Get an issue on 3DRepo, given a specific issueId.
        private Issue GetIssue(string issueId, string teamsSpace, string modelId, string userAPIkey = null)
        {
            Issue issue = null;

            // Get the Issue corresponding to the specified IssueId. See https://3drepo.github.io/3drepo.io/#api-Issues-findIssue
            string endpoint = $"https://api1.www.3drepo.io/api/{teamsSpace}/{modelId}/issues/{issueId}";

            if (!string.IsNullOrWhiteSpace(userAPIkey))
                endpoint += $"?key={userAPIkey}";

            // GET request
            HttpResponseMessage respMessage;
            string fullResponse = "";
            using (var httpClient = new HttpClient())
            {
                respMessage = httpClient.GetAsync(endpoint).Result;

                // Process response
                fullResponse = respMessage.Content.ReadAsStringAsync().Result;
            }

            if (!respMessage.IsSuccessStatusCode)
            {
                BH.Engine.Base.Compute.RecordError($"The server returned a {respMessage.StatusCode} Error:\n" + fullResponse);
                return null;
            }

            // Deserialise the object.
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new IssueContractResolver();

            try
            {
                issue = Newtonsoft.Json.JsonConvert.DeserializeObject<Issue>(fullResponse, serializerSettings);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e.Message);
            }

            return issue;
        }

        /***************************************************/

        private List<Issue> GetAllIssues(string teamsSpace, string modelId, string revisionId, string userAPIkey = null)
        {
            List<Issue> allIssues = new List<Issue>();

            // See https://3drepo.github.io/3drepo.io/#api-Issues-listIssues.
            // This is a "lightweight" endpoint supposed to retrieve only the essential info about all the issues,
            // e.g. retrieved issues do not include any existing Comment.
            string endpoint = $"https://api1.www.3drepo.io/api/{teamsSpace}/{modelId}/revision/{revisionId}/issues";

            if (!string.IsNullOrWhiteSpace(userAPIkey))
                endpoint += $"?key={userAPIkey}";

            // GET request
            HttpResponseMessage respMessage = null;
            string fullResponse = "";
            using (var httpClient = new HttpClient())
            {
                try
                {
                    respMessage = httpClient.GetAsync(endpoint).Result;
                }
                catch (Exception e)
                {
                    BH.Engine.Base.Compute.RecordError($"Error while pulling all issues belonging to model {modelId}, revision {revisionId}:\n {e.Message}.");
                    return new List<Issue>();
                }

                // Process response
                fullResponse = respMessage.Content.ReadAsStringAsync().Result;
            }

            if (!respMessage.IsSuccessStatusCode)
            {
                fullResponse = fullResponse.GetResponseBody();

                BH.Engine.Base.Compute.RecordError($"The server returned a {respMessage.StatusCode} Error:\n" + fullResponse);
                return new List<Issue>();
            }

            // Deserialise the object.
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new IssueContractResolver();

            // First pull a list of all existing issues. Since this is pulled with the "allIssues" endpoint,
            // single issues contain only minimal information (i.e. no comments, etc.).
            // We will use these only to get the Id and then fetch each full issue individually.
            List<Issue> issues_MinimalInfo_deserialised = new List<Issue>();

            try
            {
                issues_MinimalInfo_deserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Issue>>(fullResponse, serializerSettings);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e.Message);
            }

            // Fetch each full Issue individually. These will include all the needed info.
            foreach (var issue in issues_MinimalInfo_deserialised)
            {
                allIssues.Add(GetIssue(issue.Id, teamsSpace, modelId, userAPIkey));
            }

            return allIssues;
        }

        /***************************************************/

        private class IssueContractResolver : DefaultContractResolver
        {
            private Dictionary<string, string> PropertyMappings { get; set; }

            public IssueContractResolver()
            {
                this.PropertyMappings = new Dictionary<string, string>
                {
                    {nameof(Issue.Name), "name"},
                    {nameof(Issue.AssignedRoles), "assigned_roles"},
                    {nameof(Issue.DueDate), "due_date" },
                    {nameof(Issue.Status), "status"},
                    {nameof(Issue.Priority), "priority"},
                    {nameof(Issue.TopicType), "topic_type"},
                    {nameof(Issue.Viewpoint), "viewpoint"},
                    {nameof(Issue.Desc), "desc"},
                    {nameof(Issue.Position), "position"},
                    {nameof(Issue.Comments), "comments"},
                    {nameof(Issue.Id), "_id"},
                    {nameof(Issue.RevisionId), "rev_id"},

                };
            }

            protected override string ResolvePropertyName(string propertyName)
            {
                string resolvedName = null;
                var resolved = this.PropertyMappings.TryGetValue(propertyName, out resolvedName);
                if (!resolved)
                { 
                    resolvedName = this.PropertyMappings
                    .FirstOrDefault(x => x.Value == propertyName)
                    .Key;
                    if (resolvedName != null)
                    {
                        resolved = true;
                    }
                }
                return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
            }
        }
    }
}





