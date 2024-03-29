/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.TDRepo;
using System.IO;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public string CreateOBJfile(IEnumerable<IObject> objs, PushConfig pushConfig = null)
        {
            List<oM.Geometry.Mesh> meshes = objs.OfType<oM.Geometry.Mesh>().ToList();

            if (meshes.Count() != objs.Count())
                BH.Engine.Base.Compute.RecordWarning($"The OBJ file format supports only Push of {typeof(oM.Geometry.Mesh).Name}.");

            // Add to the scene
            IEnumerable<TDR_Mesh> tdRepoMeshes = meshes.Where(tdRepoMesh => tdRepoMesh != null)
                .Select(tdRepoMesh => BH.Adapter.TDRepo.Convert.ToTDRepo(tdRepoMesh));

            m_3DRepoMeshesForOBJexport.AddRange(tdRepoMeshes);

            // Write OBJ file and commit it.
            string OBJFilePath = WriteOBJFile();

            // Commit the objects as serialised in the created OBJ file.
            if (!string.IsNullOrWhiteSpace(OBJFilePath))
                CommitNewRevision(OBJFilePath);

            return OBJFilePath;
        }
    }
}





