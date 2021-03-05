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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;

namespace BH.oM.Adapters.TDRepo
{
    [Description("3DRepo's description object. The only mandatory fields for POST are Name and Position.")]
    public class Issue : IObject // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    {
        [Description("The name of the issue.")]
        public virtual string Name { get; set; }

        [Description("The roles assigned to the issue. " +
            "Even though its an array (for future support of multiple assigned jobs), " +
            "currently it has one or none elements correspoing to the available jobs in the teamaspace.")]
        public virtual List<string> AssignedRoles { get; set; } = new List<string>();

        [Description("The status of the issue. It can have a value of `open`,`in progress`,`for approval` or `closed`.")]
        public virtual string Status { get; set; }

        [Description("The priority of the issue. It can have a value of `none`, `low`, `medium` or `high`.")]
        public virtual string Priority { get; set; }

        [Description("Topic type of the issue. Its value has to be one of the defined topic_types for the model." +
            "\nDefaults to `unassigned`. Other valid examples: `Clash`; `for_information`." +
            "\nSee https://3drepo.github.io/3drepo.io/#api-Model-createModel for more details.")]
        public virtual string TopicType { get; set; } = "unassigned";

        [Description("The viewpoint of the issue, defining the position of the camera and the screenshot for that position.")]
        public virtual Viewpoint Viewpoint { get; set; }

        [Description("The description of the created issue.")]
        public virtual string Desc { get; set; } // `desc` is the json property name required by 3DRepo when creating issues

        [Description("The due date of the created issue.")]
        public virtual string DueDate { get; set; } // `desc` is the json property name required by 3DRepo when creating issues

        [Description("The vector defining the pin of the issue (X, Y, Z).")]
        public virtual double[] Position { get; set; }

        [Description("Comments attached to the Issue.")]
        public virtual List<Comment> Comments { get; set; }

        [Description("Resources attached to the Issue")]
        public virtual List<Resource> Resources { get; set; }

        [Description("The GUID of the Issue on 3DRepo. This is assigned by the server.")]
        public virtual string Id { get; set; } = null;

        [Description("The GUID of the Revision hosting the Issue on 3DRepo. This is assigned by the server.")]
        public virtual string RevisionId { get; set; } = null;

        [Description("DateTime of the created issue, in UNIX format. When null, this is assigned by the server.")]
        public virtual long? Created { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }
}
