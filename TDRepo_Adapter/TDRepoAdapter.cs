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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.TDRepo;
using BH.oM.Base;
using BH.oM.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        //Add any applicable constructors here, such as linking to a specific file or anything else as well as linking to that file through the (if existing) com link via the API
        public TDRepoAdapter(string teamspace, string modelId, string apiKey, string url = "https://api1.www.3drepo.io/api")
        {
            AdapterIdName = BH.Engine.TDRepo.Convert.AdapterIdName;
            m_AdapterSettings.DefaultPushType = oM.Adapter.PushType.CreateOnly;

            Logger.Instance.Log("Establishing repo Adapter controller with URL: " + url + " api key: " + apiKey + " teamspace: " + teamspace + "modelID: " + modelId);

            this.host = url;
            this.apiKey = apiKey;
            this.teamspace = teamspace;
            this.modelId = modelId;
            sceneCreator = new SceneCreator();
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        private string host;
        private string apiKey;
        private string teamspace;
        private string modelId;
        private SceneCreator sceneCreator;

        private bool Commit(ref string error)
        {
            Logger.Instance.Log("Creating file...");
            var filePath = sceneCreator.CreateFile();
            Logger.Instance.Log("Created file at : " + filePath);
            bool success;
            try
            {
                Connector.NewRevision(host, apiKey, teamspace, modelId, filePath);
                success = true;
            }
            catch (System.Exception e)
            {
                error = e.ToString();
                success = false;
            }

            sceneCreator.Clear();
            return success;
        }

        /***************************************************/


    }
}

