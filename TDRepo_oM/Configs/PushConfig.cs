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
using BH.oM.Base;
using BH.oM.Adapter;
using System.ComponentModel;
using System.IO;
using BH.oM.Graphics;

namespace BH.oM.External.TDRepo
{
    public class PushConfig : ActionConfig
    {
        [Description("Sets whether to create (export) models following the .BIM format convention." +
            "If false, file export follows .obj convention (no material).")]
        public bool PushBIMFormat { get; set; } = true;

        [Description("Directory where to save the .BIM file that is to be uploaded.")]
        public string Directory { get; set; } = Path.Combine("C:\\temp", "3DRepoToolkitBIMFiles");

        [Description("Name of the .BIM file that is to be uploaded.")]
        public string FileName { get; set; } = Guid.NewGuid().ToString();

        [Description("Toggles whether to delete or keep the .BIM file once it is uploaded.")]
        public bool KeepBIMFile { get; set; } = true;

        public RenderMeshOptions RenderMeshOptions { get; set; } = new RenderMeshOptions();
    }
}
