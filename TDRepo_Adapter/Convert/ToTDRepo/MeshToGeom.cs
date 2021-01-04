/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.TDRepo;
using RepoFileExporter.dataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.TDrepo
{
    public static class Convert
    {
        public static Geometry MeshToGeometry(this BH.oM.Geometry.Mesh mesh, int materialIdx = 1)
        {
            if (mesh == null)
                return null;

            // Make sure input mesh is triangle mesh
            mesh = BH.Engine.Geometry.Modify.Triangulate(mesh);

            //Geometry testGeometry = new Geometry(
            //        new List<double> {
            //            0, 0, 0,
            //            1, 0, 0,
            //            1, 1, 0,
            //            0, 1, 0 },
            //        new List<uint> { 0, 1, 2, 0, 2, 3 }, null,
            //        materialIdx);

            Geometry geometry = null;

            List<double> pointsCoords = new List<double>();
            mesh.Vertices.ForEach(v => {
                pointsCoords.Add(v.X);
                pointsCoords.Add(v.Y);
                pointsCoords.Add(v.Z);
            });

            List<uint> faces = new List<uint>();
            mesh.Faces.ForEach(f => {
                faces.Add((uint)f.A);
                faces.Add((uint)f.B);
                faces.Add((uint)f.C);
            });

            geometry = new Geometry(pointsCoords, faces, null, materialIdx);

            return geometry;
        }
    }
}
