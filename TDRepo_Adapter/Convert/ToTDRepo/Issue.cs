﻿/*
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
        public static BH.oM.Adapters.TDRepo.Issue ToTDRepo(this Issue bhomIssue, Audit parentAudit = null, string resourcesFolder = "", int issueIdx = 0)
        {
            BH.oM.Adapters.TDRepo.Issue tdrIssue = new BH.oM.Adapters.TDRepo.Issue();

            // Checks
            if (string.IsNullOrWhiteSpace(bhomIssue.Name))
            {
                BH.Engine.Reflection.Compute.RecordError($"The {nameof(BH.oM.Inspection.Issue)} must be assigned a Name in order for the conversion to be possible.");
                return null;
            }

            if (bhomIssue.Position == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"The {nameof(BH.oM.Inspection.Issue)} must be assigned a position in order for the conversion to be possible.");
                return null;
            }

            // Pick and choose data from the BH.oM.Inspection.Audit and the BH.oM.Inspection.Issue
            // to build the BH.oM.Adapters.TDRepo.Issue, which can be then uploaded to 3DRepo.

            tdrIssue.Name = bhomIssue.Name;
            tdrIssue.Created = bhomIssue.DateCreated.Ticks;
            tdrIssue.AssignedRoles.AddRange(bhomIssue.Assign); // TODO: check
            tdrIssue.Status = bhomIssue.Status;
            tdrIssue.Priority = bhomIssue.Priority;
            tdrIssue.TopicType = bhomIssue.Type;

            // The first media item is picked as the screenshot.
            string screenshotFilePath = !string.IsNullOrWhiteSpace(bhomIssue?.Media?.FirstOrDefault()) ? System.IO.Path.Combine(resourcesFolder ?? "C:\\temp\\", bhomIssue.Media.FirstOrDefault()) : null;
            tdrIssue.Viewpoint = new oM.Adapters.TDRepo.Viewpoint()
            {
                Position = new double[] { bhomIssue.Position.X, bhomIssue.Position.Y, bhomIssue.Position.Z },  // TODO: now this is taking the same Position of the issue. Ideally to take the position of the media's viewpoint.
                Screenshot = Compute.ReadToBase64(screenshotFilePath)
                // TODO: all other properties of 3DRepo's Viewpoint are currently not in any BHoM object. 
            };

            tdrIssue.Position = new double[] {
                bhomIssue.Position.X,
                bhomIssue.Position.Y,
                bhomIssue.Position.Z };

            // The description is where the ParentAuditId is stored currently. 
            // This is needed to map the issue back to the Parent Audit when a Pull with an AuditRequest is done.
            // TODO: look into mapping issues from Audit to TDRepoIssues
            if (parentAudit != null)
                tdrIssue.Description = bhomIssue.Description + $"\nParentAuditId: {parentAudit.BHoM_Guid}";
            return tdrIssue;
        }
    }
}
