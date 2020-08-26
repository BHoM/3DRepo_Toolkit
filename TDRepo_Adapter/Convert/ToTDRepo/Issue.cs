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
using BH.oM.Inspection;
using BH.Engine.Adapters.TDRepo;



namespace BH.Adapter.TDRepo
{
    public static partial class Convert
    {
        public static BH.oM.Adapters.TDRepo.Issue ToTDRepo(this Issue issue, Audit parentAudit = null, string resourcesFolder = "", int issueIdx = 0)
        {
            BH.oM.Adapters.TDRepo.Issue tdrIssue = new BH.oM.Adapters.TDRepo.Issue();

            // Checks
            if (string.IsNullOrWhiteSpace(issue.Name))
            {
                BH.Engine.Reflection.Compute.RecordError($"The {nameof(BH.oM.Inspection.Issue)} must be assigned a Name in order for the conversion to be possible.");
                return null;
            }

            if (issue.Position == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"The {nameof(BH.oM.Inspection.Issue)} must be assigned a position in order for the conversion to be possible.");
                return null;
            }

            // Pick and choose data from the BH.oM.Inspection.Audit and the BH.oM.Inspection.Issue
            // to build the BH.oM.Adapters.TDRepo.Issue, which can be then uploaded to 3DRepo.

            tdrIssue.Name = issue.Name;
            // TODO: To be replaced by issue property
            tdrIssue.Created = (parentAudit?.IssueDateUtc.Ticks ?? 0) == 0 ? tdrIssue.Created : parentAudit.IssueDateUtc.Ticks;

            tdrIssue.AssignedRoles.Add(issue.Assign); // TODO: check
            tdrIssue.Status = issue.Status;
            tdrIssue.Priority = issue.Priority;
            tdrIssue.TopicType = issue.Type;

            // The first media item is picked as the screenshot.
            string screenshotFilePath = !string.IsNullOrWhiteSpace(issue.Media.FirstOrDefault()) ? System.IO.Path.Combine(resourcesFolder ?? "C:\\temp\\", issue.Media.FirstOrDefault()) : null;
            tdrIssue.Viewpoint = new oM.Adapters.TDRepo.Viewpoint()
            {
                Position = new double[] { issue.Position.X, issue.Position.Y, issue.Position.Z },  // TODO: now this is taking the same Position of the issue. Ideally to take the position of the media's viewpoint.
                Screenshot = Compute.ReadToBase64(screenshotFilePath)
                // TODO: all other properties of 3DRepo's Viewpoint are currently not in any BHoM object. 
            };

            tdrIssue.Position = new double[] {
                issue.Position.X,
                issue.Position.Y,
                issue.Position.Z };

            // The description is where the ParentAuditId is stored currently. 
            // This is needed to map the issue back to the Parent Audit when a Pull with an AuditRequest is done.
            // TODO: look into mapping issues from Audit to TDRepoIssues
            tdrIssue.Description = issue.Description + $"\nParentAuditId: {parentAudit.BHoM_Guid}";
            return tdrIssue;
        }
    }
}
