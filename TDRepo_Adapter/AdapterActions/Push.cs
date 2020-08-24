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
using BH.Adapter;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Reflection;
using BH.oM.Adapters.TDRepo.Commands;
using BH.oM.Adapters.TDRepo;
using System.IO;
using BH.oM.Inspection;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            if (string.IsNullOrWhiteSpace(m_host) || string.IsNullOrEmpty(m_teamspace) || string.IsNullOrEmpty(m_userAPIKey) || string.IsNullOrEmpty(m_modelId))
            {
                BH.Engine.Reflection.Compute.RecordError("One or more of the needed parameters in the TDRepoAdapter is missing or invalid (modelId, userAPIkey, etc).");
                return new List<object>();
            }

            m_MediaPathAlert = true;

            IEnumerable<IObject> iObjs = objects.OfType<IObject>();
            if (iObjs.Count() != objects.Count())
                BH.Engine.Reflection.Compute.RecordError($"Push to 3DRepo currently supports only objects implementing {nameof(IObject)}.");

            List<object> createdObjects = new List<object>();

            PushConfig pushConfig = actionConfig as PushConfig ?? new PushConfig();

            // Separate Audits and Issues from the rest of the objects. Those have to be created last.
            IEnumerable<oM.Inspection.Audit> audits = iObjs.OfType<oM.Inspection.Audit>();
            IEnumerable<oM.Inspection.Issue> issues = iObjs.OfType<oM.Inspection.Issue>();

            iObjs = iObjs.Except(audits);
            iObjs = iObjs.Except(issues);

            // Create and commit the objects
            if (iObjs.Any())
            {
                string filePath = "";
                if (pushConfig.PushBIMFormat)
                    filePath = CreateBIMFile(iObjs, pushConfig);
                else
                    filePath = CreateOBJfile(iObjs); // Upload using the old obj format

                createdObjects.AddRange(iObjs);
            }

            // Create the audits/issues
            if (audits.Any() && Create(audits, pushConfig))
                createdObjects.AddRange(audits);

            if (issues.Any() && Create(issues, pushConfig))
                createdObjects.AddRange(issues);

            return createdObjects;
        }
    }
}
