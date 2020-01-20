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
using BH.oM.Structure.Elements;
using BH.oM.Base;
using BH.oM.Adapter;
using BH.oM.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        // This gets called by the Push component.
        protected override bool ICreate<T>(IEnumerable<T> objects, oM.Adapter.ActionConfig actionConfig = null)
        {
            Logger.Instance.Log("Create Called");
            //This is the main dispatcher method, calling the specific implementation methods for the other toolkits.

            bool success = true;        //boolean returning if the creation was successfull or not

            success = CreateCollection(objects as dynamic); //Calls the correct CreateCollection method based on dynamic casting

            Logger.Instance.Log("Committing changes.");

            string error = "";
            success = controller.Commit(ref error);

            if (success)
                Logger.Instance.Log("Done.");
            else
                BH.Engine.Reflection.Compute.RecordError($"Error when sending data to 3DRepo:\n{error}");

            return success;             //Finally return if the creation was successful or not
        }


        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<oM.Geometry.Mesh> objs)
        {

            foreach (var obj in objs)
            {
                controller.AddToScene(Engine.TDRepo.Convert.FromBHoM(obj as oM.Geometry.Mesh));
            }

            return true;
        }

        private bool CreateCollection(IEnumerable<IBHoMObject> objs)
        {
            BH.Engine.Reflection.Compute.RecordError($"3DRepo adatper can't yet export objects of type {objs.First().GetType().Name}");
            return false;
        }
    }
}
