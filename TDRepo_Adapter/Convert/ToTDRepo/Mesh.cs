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
using BH.oM.Geometry;
using BH.oM.Adapters.TDRepo;

namespace BH.Adapter.TDRepo
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BH.oM.Adapters.TDRepo.TDR_Mesh ToTDRepo(BH.oM.Geometry.Mesh mesh)
        {
            var faces = mesh.Faces.Select(face =>
                new BH.oM.Adapters.TDRepo.TDR_Face(new int[]{ face.A, face.B, face.C, face.D })
            );

            var points = mesh.Vertices.Select(vertex =>
                new BH.oM.Adapters.TDRepo.TDR_Point(vertex.X, vertex.Y, vertex.Z)
            );

            return new BH.oM.Adapters.TDRepo.TDR_Mesh("Mesh", points.ToArray(), faces.ToArray());
        }

        /***************************************************/
    }
}




