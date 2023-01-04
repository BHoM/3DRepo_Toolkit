/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

namespace BH.oM.Adapters.TDRepo
{
    public class PushConfig : ActionConfig
    {
        [Description("Sets whether to create (export) models following the .BIM format convention." +
            "If false, file export follows .obj convention (no material).")]
        public bool PushBIMFormat { get; set; } = true;

        [Description("The .BIM file that is to be uploaded will be assembled at this location." +
            "\n(In order to Push, a .BIM file is to be assembled with the input BHoMObjects)")]
        public string Directory { get; set; } = Path.Combine("C:\\temp", "3DRepoToolkitBIMFiles");

        [Description("The .BIM file that is to be uploaded will be assembled with this name." +
            "\n(In order to Push, a .BIM file is to be assembled with the input BHoMObjects)")]
        public string FileName { get; set; } = Guid.NewGuid().ToString();

        [Description("Toggles whether to delete or keep the assembled .BIM file once it is uploaded.")]
        public bool KeepBIMFile { get; set; } = true;

        [Description("Options for the computation of the Render Mesh, used to represent the objects in 3D repo.")]
        public RenderMeshOptions RenderMeshOptions { get; set; } = new RenderMeshOptions();

        [Description("Directory where the media are stored.")]
        public string MediaDirectory { get; set; }
    }
}



