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
        public static BH.oM.Inspection.Issue FromTDRepo(this BH.oM.Adapters.TDRepo.Issue issue, List<string> mediaFileNames = null)
        {
            BH.oM.Inspection.Issue bhomIssue = new BH.oM.Inspection.Issue();

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
            bhomIssue.Name = issue.Name;
            // TODO: Create date is stored in BHOM Audit
            bhomIssue.Assign = issue.AssignedRoles; // TODO: check
            bhomIssue.Status = issue.Status;
            bhomIssue.Priority = issue.Priority;
            bhomIssue.Type = issue.TopicType;
            bhomIssue.Comments = issue.Comments?.Select(c =>
                new Comment()
                {
                    Message = string.IsNullOrWhiteSpace(c.comment) ? $"{c.action.property} changed from `{(string.IsNullOrWhiteSpace(c.action.from) ? "<empty>" : c.action.from)}` to `{c.action.to}`." : c.comment,
                    Owner = c.owner,
                    CommentDate = new DateTime(long.Parse(c.created.ToString()))
                })
                .ToList();

            if (mediaFileNames != null)
              bhomIssue.Media = mediaFileNames;

            bhomIssue.Position = new oM.Geometry.Point()
            {
                X = issue.Position[0],
                Y = issue.Position[1],
                Z = issue.Position[2]
            };

            string toFind = "\nParentAuditId: ";
            int pos = issue.Desc.IndexOf(toFind) + toFind.Length;
            bhomIssue.Description = issue.Desc.Remove(pos);

            return bhomIssue;
        }
    }
}
