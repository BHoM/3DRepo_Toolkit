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
using BH.oM.Adapters.TDRepo.Requests;
using System.Net.Http;
using Newtonsoft.Json.Serialization;
using BH.Engine.Adapters.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public List<Issue> GetIssues(IssueRequest ir, PullConfig pullConfig, bool enableMessages = true, Dictionary<Issue, List<string>> mediaFileNames = null)
        {
            string modelId = ir?.ModelId ?? m_modelId;
            string teamsSpace = ir?.TeamSpace ?? m_teamspace;

            string endpoint = "";

            string issueId = ir.IssueId;
            bool singleIssue = !string.IsNullOrWhiteSpace(issueId);

            if (singleIssue)
            {
                // Get the Issue corresponding to the specified IssueId
                endpoint = $"https://api1.www.3drepo.io/api/{teamsSpace}/{modelId}/issues/{issueId}";

                if (enableMessages)
                    BH.Engine.Reflection.Compute.RecordNote($"Getting {nameof(Issue)} with id {issueId} \nfrom the 3DRepo Model `{modelId}` in the Teamspace `{teamsSpace}`.");
            }
            else
            {
                // If no IssueId is specified, get *all the issues* attached to a specific Revision.
                // (In 3DRepo, the issues are attached to a specific Model's Revision)

                // If no RevisionId is specified in the Request, consider the latest revision (First()).
                string revisionId = ir.RevisionId ?? GetRevisions(new RevisionRequest(), false).First().Id;

                endpoint = $"https://api1.www.3drepo.io/api/{teamsSpace}/{modelId}/revision/{revisionId}/issues";

                if (enableMessages)
                    BH.Engine.Reflection.Compute.RecordNote($"Getting all of the {nameof(Issue)}s attached to RevisionId {revisionId} \nfrom the 3DRepo Model `{modelId}` in the Teamspace `{teamsSpace}`.");
            }

            if (!string.IsNullOrWhiteSpace(ir.UserAPIKey))
                endpoint += $"?key={ir.UserAPIKey}";
            else if (!string.IsNullOrWhiteSpace(m_userAPIKey))
                endpoint += $"?key={m_userAPIKey}";

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
                fullResponse = fullResponse.GetResponseBody();

                BH.Engine.Reflection.Compute.RecordError($"The server returned a {respMessage.StatusCode} Error:\n" + fullResponse);
                return new List<Issue>();
            }

            // Deserialise the object.
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new IssueContractResolver();

            List<Issue> issues_deserialised = new List<Issue>();

            try
            {
                if (singleIssue)
                    issues_deserialised.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Issue>(fullResponse, serializerSettings));
                else
                    issues_deserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Issue>>(fullResponse, serializerSettings);
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError(e.Message);
            }

            if (pullConfig.DownloadResources)
            {
                // Attempt the pull of the resources.
                foreach (Issue issue in issues_deserialised)
                {
                    // Currently Resources are stored as Comments attached to the issue.
                    // Pull the resources from the Comments.
                    if (issue.Comments.Count() != 0)
                    {
                        // Dictionary whose Key is filename (fullPath), Value is the base64 string representation.
                        Dictionary<string, string> base64resources = new Dictionary<string, string>();

                        // TODO: Apparently, the actual image is not pulled (`Comments` property is empty) when using the GET Issue endpoint.
                        // There must be another endpoint/way to pull the image that was posted in the Comments (SEE BELOW)

                        // TODO: Add getVIewpoint endpoint to get image from the Viewpoint. https://3drepo.github.io/3drepo.io/#api-Viewpoints-findViewpoint
                        // Get the Base64 string image, as it should be attached to the `Comments` property of the pulled Issues.
                        // Store it in the above dictionary.
                        base64resources =
                            issue.Comments
                            .ToDictionary(c => Path.Combine(pullConfig.ResourceDownloadDirectory, c.comment ?? ""), c => c.viewpoint?.Screenshot);

                        // Save the pulled resource to file. If the base64 string is missing, it will simply create an empty file.
                        base64resources.ToList().ForEach(kv => Compute.WriteToByteArray(kv.Value, kv.Key, false));

                        // Store the pulled images Filenames in the resources. 
                        // This allows to pass this information to the Convert method, see the Pull.
                        if (mediaFileNames != null)
                            mediaFileNames[issue] = issue.Comments.Select(c => c.comment).ToList(); 
                    }
                }
            }

            return issues_deserialised;
        }

        public class IssueContractResolver : DefaultContractResolver
        {
            private Dictionary<string, string> PropertyMappings { get; set; }

            public IssueContractResolver()
            {
                this.PropertyMappings = new Dictionary<string, string>
                {
                    {nameof(Issue.Name), "name"},
                    {nameof(Issue.AssignedRoles), "assigned_roles"},
                    {nameof(Issue.Status), "status"},
                    {nameof(Issue.Priority), "priority"},
                    {nameof(Issue.TopicType), "topic_type"},
                    {nameof(Issue.Viewpoint), "viewpoint"},
                    {nameof(Issue.Description), "desc"},
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
                return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
            }
        }
    }
}

