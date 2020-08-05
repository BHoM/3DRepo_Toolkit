/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BHG = BH.oM.Geometry;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Structure.Elements;
using BH.Engine.Structure;
using BH.Engine.Rhinoceros;
using BH.oM.Structure.Constraints;
using BH.oM.Analytical.Elements;

namespace BH.Engine.External.TDRepo
{
    public static partial class Compute
    {
        [Description("Returns a BHoM mesh representation for the BHoM Bar.")]
        public static BH.oM.Geometry.Mesh MeshRepresentation(this Sphere sphere, BH.oM.External.TDRepo.DisplayOptions displayOptions = null)
        {
            displayOptions = displayOptions ?? new BH.oM.External.TDRepo.DisplayOptions();

            return sphere.RhinoMeshRepresentation(displayOptions).FromRhino();
        }

        public static Rhino.Geometry.Mesh RhinoMeshRepresentation(this BH.oM.Geometry.Sphere sphere, BH.oM.External.TDRepo.DisplayOptions displayOptions)
        {
            return Rhino.Geometry.Mesh.CreateFromSphere(sphere.ToRhino(), 8, 4);
        }
    }

}

