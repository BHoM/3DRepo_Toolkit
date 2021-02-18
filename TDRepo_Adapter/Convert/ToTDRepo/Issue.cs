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
using BH.oM.Inspection;
using BH.Engine.Adapters.TDRepo;



namespace BH.Adapter.TDRepo
{
    public static partial class Convert
    {
        public static BH.oM.Adapters.TDRepo.Issue ToTDRepo(this Issue bhomIssue, string resourcesFolder = "")
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

            //TDRepo uses UNIX time, needs converted value from UTC
            DateTimeOffset UTCdt = bhomIssue.DateCreated;
            long dateCreatedUNIX = UTCdt.ToUnixTimeMilliseconds();

            // Pick and choose data from the BH.oM.Inspection.Issue
            // to build the BH.oM.Adapters.TDRepo.Issue, which can be then uploaded to 3DRepo.

            tdrIssue.Name = bhomIssue.Name;
            tdrIssue.Created = dateCreatedUNIX; 
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
                // Note: all other properties of the 3DRepo's Viewpoint object are not currently supported by BHoM, and therefore are not populated.
            };

            tdrIssue.Position = new double[] {
                bhomIssue.Position.X,
                bhomIssue.Position.Y,
                bhomIssue.Position.Z };

            tdrIssue.Desc = bhomIssue.Description;

            tdrIssue.Desc += $"\nParentAuditId: {bhomIssue.AuditID}";

            return tdrIssue;
        }
    }
}

