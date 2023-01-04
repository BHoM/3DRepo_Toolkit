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
using BH.oM.Base;
using BH.oM.Adapter;
using BH.oM.Adapters.TDRepo;
using System.IO;
using BH.oM.Adapters.TDRepo.Requests;
using System.Net.Http;
using Newtonsoft.Json.Serialization;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public List<Revision> GetRevisions(RevisionRequest rr, bool enableMessages = true)
        {
            string modelId = rr.ModelId ?? m_modelId;
            string teamsSpace = rr.TeamSpace ?? m_teamspace;

            string endpoint = $"https://api1.www.3drepo.io/api/{teamsSpace}/{modelId}/revisions.json";

            if (!string.IsNullOrWhiteSpace(rr.UserAPIKey))
                endpoint += $"?key={rr.UserAPIKey}";
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

            // Deserialise the object. 
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new RevisionContractResolver();

            List<Revision> revisions_deserialised = new List<Revision>();
            try
            {
                revisions_deserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Revision>>(fullResponse, serializerSettings);
            } catch
            {
                Revision rev =  Newtonsoft.Json.JsonConvert.DeserializeObject<Revision>(fullResponse, serializerSettings);
                revisions_deserialised.Add(rev);
            }

            if (enableMessages)
                BH.Engine.Base.Compute.RecordNote($"Returning {nameof(Revision)}s \nfrom the 3DRepo Model `{modelId}` in the Teamspace `{teamsSpace}`.");

            return revisions_deserialised;
        }

        public class RevisionContractResolver : DefaultContractResolver
        {
            private Dictionary<string, string> PropertyMappings { get; set; }

            public RevisionContractResolver()
            {
                this.PropertyMappings = new Dictionary<string, string>
                {
                    {nameof(Revision.Id), "_id"},
                    {nameof(Revision.Author), "author"},
                    {nameof(Revision.TimeStampUTC), "timestamp"},
                    {nameof(Revision.Name), "name"},
                    {nameof(Revision.Branch), "branch"},
                    {nameof(Revision.FileType), "fileType"},
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




