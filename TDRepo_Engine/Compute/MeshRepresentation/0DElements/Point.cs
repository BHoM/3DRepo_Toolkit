﻿/*
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
using BH.oM.External.TDRepo;

namespace BH.Engine.External.TDRepo
{
    public static partial class Compute
    {
        [Description("Returns a BHoM Mesh representation for the Node based on its DOF, e.g. a box for fully fixed, a cone with sphere on top for pin.")]
        public static BH.oM.Geometry.Mesh MeshRepresentation(this BH.oM.Geometry.Point point, DisplayOptions displayOptions = null)
        {
            displayOptions = displayOptions ?? new DisplayOptions();

            double radius = 0.12 * displayOptions.Element0DScale;
            Sphere sphere = BH.Engine.Geometry.Create.Sphere(point, radius);

            return sphere.MeshRepresentation(displayOptions);
        }
      
    }
}

