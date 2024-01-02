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

using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Inspection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.TDRepo.Requests
{
    [Description("Retrieves issues.")] // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    public class AuditRequest : IUserAPIKeyRequest, ITeamSpaceRequest, IModelIdRequest
    {
        [Description("Audit whose information you wish to Pull from 3DRepo.")]
        public virtual Audit Audit { get; set; } = null;

        [Description("If nothing is specified, takes the latest revision from the Model.")]
        public virtual string RevisionId { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public virtual string ModelId { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public virtual string TeamSpace { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public virtual string UserAPIKey { get; set; } = null;
    }
}



