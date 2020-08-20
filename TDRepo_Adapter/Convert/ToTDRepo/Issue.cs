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
using BH.Engine.External.TDRepo;



namespace BH.Adapter.TDRepo
{
    public static partial class Convert
    {
        public static BH.oM.External.TDRepo.Issue FromBHoM(this Audit audit, int issueIdx = 0)
        {
            BH.oM.External.TDRepo.Issue tdrIssue = new BH.oM.External.TDRepo.Issue();

            Issue issue = audit.Issues.First(); // TODO proper selection/conversion for multiple issues.

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
            // to build the BH.oM.External.TDRepo.Issue, which can be then uploaded to 3DRepo.

            tdrIssue.Name = issue.Name;
            tdrIssue.AssignedRoles.Add(issue.Assign); // check
            tdrIssue.Status = issue.Status;
            tdrIssue.Priority = issue.Priority;
            tdrIssue.TopicType = issue.Type;

            tdrIssue.Viewpoint = new oM.External.TDRepo.Viewpoint()
            {
                Position = new double[] { issue.Position.X, issue.Position.Y, issue.Position.Z },  // TODO now this is taking the same Position of the issue. Ideally to take the position of the media's viewpoint.
                Screenshot = Compute.ReadToBase64(issue.Media.FirstOrDefault()) // TODO proper selection/conversion for multiple media.
                                                                                // TODO all other properties of 3DRepo's Viewpoint are currently not in any BHoM object. 
            };

            tdrIssue.Position = new double[] {
                issue.Position.X,
                issue.Position.Y,
                issue.Position.Z }; // Check
            tdrIssue.Description = issue.Description;
            return tdrIssue;
        }
    }
}
