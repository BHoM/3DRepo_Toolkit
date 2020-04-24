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
using BH.Adapter;
using BH.Engine.TDRepo;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Reflection;
using BH.oM.TDRepo;
using BH.oM.External.TDRepo.Commands;
using BH.oM.External.TDRepo;
using System.IO;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            PushConfig pushConfig = actionConfig as PushConfig ?? new PushConfig();

            if (!pushConfig.PushBIMFormat)
            {
                UploadOBJ(objects); // Upload using the (soon to be completely superseded) obj format

                return new List<object>();
            }

            // Write .BIM file and commit
            string error = "";
            string BIMFilePath = WriteBIMFile(objects.OfType<IObject>().ToList(), pushConfig.Directory, pushConfig.FileName, pushConfig.DisplayOptions);

            bool success = controller.Commit(BIMFilePath, ref error);

            if (!success)
                BH.Engine.Reflection.Compute.RecordError($"Error on uploading data to 3DRepo:\n{error}");

            return objects.ToList();
        }

    }
}