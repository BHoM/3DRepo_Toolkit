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
using BH.oM.Geometry;
using ThreeDRepo;
using BH.oM.Base;

namespace BH.Adapter.ThreeDRepo
{
    public partial class RepoAdapter
    {

        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<IObject> objs)
        {
            //Code for creating a collection of materials in the software
            
            foreach (var obj in objs)
            {
                if (obj is oM.Geometry.Mesh)
                {
                    Logger.Instance.Log("Found Mesh...");
                    controller.AddToScene(Engine._3DRepo_Toolkit.Convert.To3DRepo(obj as oM.Geometry.Mesh));
                }
            }

            return true;
        }


        /***************************************************/
    }
}
