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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.TDRepo
{
    [Description("Class to encapsulate output of geometry processor")]
    public class BIMGeom
    {
        public List<uint> triangleIndices;  // 0 1 2 0 3 4
        public List<double> vertices;       // x y z x y z ...
        public List<float> normals;         // 0 1 0 v1 v2 v3 // normalised

        public List<int> triangleOffsets;   // 135th byte where triangle starts
        public List<int> vertexOffsets;     // 
        public List<int> normalOffsets;

        public BH.oM.Geometry.BoundingBox BoundingBox;
    }
}
