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

using System.Collections.Generic;
using System.IO;

namespace BH.oM.TDRepo
{
    class SceneCreator
    {
        internal SceneCreator() { }

        internal void Add(Mesh mesh) {
            meshes.Add(mesh);
        }

        internal void Clear() {
            meshes.Clear();
        }

        internal string CreateFile() {
            // This current creates a .obj (no .mtl) for the sake of simplicity.
            // For full support this really should be generating a .bim to support rich BIM data.
            string filePath = Path.GetTempPath() + System.Guid.NewGuid() + ".obj";
            using (var file = new System.IO.StreamWriter(filePath))
            {
                int startIdx = 0;
                foreach (var mesh in meshes)
                {
                    Dictionary<int, int> indexToFullIdx = new Dictionary<int, int>();
                    file.WriteLine("o " + mesh.name);

                    int idxCount = 0;
                    foreach (var v in mesh.vertices)
                    {
                        indexToFullIdx[idxCount++] = idxCount + startIdx;
                        file.WriteLine("v " + v.x + " " + v.y + " " + v.z);
                    }

                    foreach (var f in mesh.faces)
                    {
                        string line = "f ";
                        foreach (var index in f.indices) {
                            line += indexToFullIdx[index] + " ";
                        }
                        file.WriteLine(line);

                    }

                    startIdx += idxCount;
                }
            }
            return filePath;
        }

        private List<Mesh> meshes = new List<Mesh>();
    }
}
