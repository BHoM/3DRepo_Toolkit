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

using BH.oM.Geometry;
using System.Linq;
using System.ComponentModel;
using BH.Engine.Rhinoceros;
using System.Collections.Generic;

namespace BH.Engine.External.TDRepo
{
    public static partial class Compute
    {
        [Description("Returns a BHoM mesh representation for the BHoM Bar.")]
        public static BH.oM.Geometry.Mesh MeshRepresentation(this Line line, BH.oM.External.TDRepo.DisplayOptions displayOptions = null)
        {
            displayOptions = displayOptions ?? new BH.oM.External.TDRepo.DisplayOptions();

            return line.RhinoMeshRepresentation(displayOptions).FromRhino();
        }

        private static Rhino.Geometry.Mesh RhinoMeshRepresentation(this Line line, BH.oM.External.TDRepo.DisplayOptions displayOptions)
        {
            // Returns the piped centreline.

            try
            {
                // This only works for Rhino 6.
                return Rhino.Geometry.Mesh.CreateFromCurvePipe(line.ToRhino().ToNurbsCurve(), 0.01, 3, 1, Rhino.Geometry.MeshPipeCapStyle.None, true);
            }
            catch { }

            // Conversion for Rhino 5.
            List<Rhino.Geometry.Brep> pipe = Rhino.Geometry.Brep.CreatePipe(line.ToRhino().ToNurbsCurve(), 0.01, true, Rhino.Geometry.PipeCapMode.None, true, 0.01, 0.01).ToList();
            Rhino.Geometry.Mesh rhinoMesh = new Rhino.Geometry.Mesh();
            List<Rhino.Geometry.Mesh> pipeMeshes = pipe.SelectMany(p => Rhino.Geometry.Mesh.CreateFromBrep(p, Rhino.Geometry.MeshingParameters.Minimal)).ToList();
            pipeMeshes.ForEach(m => rhinoMesh.Append(m));

            return rhinoMesh;
        }
    }
}

