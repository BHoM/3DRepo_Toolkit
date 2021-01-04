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

using BH.oM.Adapter;
using BH.oM.Base;
using BH.Engine.Base;
using BH.oM.Data.Requests;
using BH.oM.Adapters.TDRepo.Requests;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Adapters.TDRepo;
using BH.oM.Inspection;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            PullConfig pullConfig = actionConfig as PullConfig ?? new PullConfig();

            RevisionRequest rr = request as RevisionRequest;
            if (rr != null)
                return GetRevisions(rr).OfType<object>();

            IssueRequest ir = request as IssueRequest;
            if (ir != null)
                return GetIssues(ir, pullConfig).OfType<object>();

            AuditRequest ar = request as AuditRequest;
            if (ar != null)
            {
                if (ar.Audit == null)
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Please specify the Audit whose Issues you want to pull from 3DRepo in the request.");
                    return null;
                }

                ir = new IssueRequest()
                {
                    ModelId = ar.ModelId,
                    RevisionId = ar.RevisionId,
                    TeamSpace = ar.TeamSpace,
                    UserAPIKey = ar.UserAPIKey
                };

                Audit audit = ar.Audit.DeepClone();

                // The conversion between 3DRepo issues and BHoM Issues
                // will need to be passed any Pulled media file Name, if they were pulled with the Issues.
                Dictionary<oM.Adapters.TDRepo.Issue, List<string>> mediaFilenames_perIssue = new Dictionary<oM.Adapters.TDRepo.Issue, List<string>>();
                List<oM.Adapters.TDRepo.Issue> pulledIssues = GetIssues(ir, pullConfig,true, mediaFilenames_perIssue);

                // Replace the specified BHoM Audit Issues with the pulled ones, 
                // in order to enable comparison.
                audit.Issues = new List<oM.Inspection.Issue>();

                foreach (var pulledIssue in pulledIssues)
                {
                    List<string> mediaFilenames = new List<string>();
                    if (pulledIssue != null)
                    {
                        mediaFilenames_perIssue.TryGetValue(pulledIssue, out mediaFilenames);

                        audit.Issues.Add(Convert.FromTDRepo(pulledIssue, mediaFilenames));
                    }
                }

                // Return the Audit with the Pulled issues from 3DRepo.
                return new List<object>() { audit };
            }

            BH.Engine.Reflection.Compute.RecordWarning($"The specified request is not compatible with {this.GetType().Name}.");
            return new List<object>();
        }
    }
}



