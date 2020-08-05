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
using BH.oM.External.TDRepo;

namespace BH.Engine.External.TDRepo
{
    public static partial class Compute
    {
        // Fallback case
        public static BH.oM.Geometry.Mesh IMeshRepresentation(IObject obj, DisplayOptions displayOptions = null)
        {
            if (obj is BH.oM.Geometry.Mesh)
                return obj as BH.oM.Geometry.Mesh;

            BH.oM.Geometry.Mesh meshRepresentation = Compute.MeshRepresentation(obj as dynamic, displayOptions);

            if (meshRepresentation != null)
                return meshRepresentation;

            // Else, see if we can get some BHoM geometry out of the BHoMObject itself to represent it.
            IGeometry geom = (obj as IBHoMObject)?.IGeometry();
            if (geom != null)
                meshRepresentation = Compute.MeshRepresentation(geom as dynamic, displayOptions);


            BH.Engine.Reflection.Compute.RecordError("Couldn't compute the Mesh representation out of the provided bhom representation object.");
            return null;
        }
    }
}
