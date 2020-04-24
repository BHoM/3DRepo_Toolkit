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
using BH.oM.Reflection.Attributes;
using BH.oM.TDRepo;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        //Add any applicable constructors here, such as linking to a specific file or anything else as well as linking to that file through the (if existing) com link via the API
        [Input("teamspace", "Name of the teamspace owning the Model.")]
        [Input("modelId", "Id of the model.")]
        [Input("modelId", "Id of the model within the teamspace.")]
        [Input("userAPIKey", "User API key to allow the upload through the 3D Repo Web API.\n" +
            "This must be generated for the specific model from the Model Manager website. See https://www.youtube.com/watch?v=prio5r5zGGc")] // or https://3drepo.github.io/3drepo.io/#api-User-generateApiKey
        [Input("url", "Address of the web server.")]
        public TDRepoAdapter(string teamspace = null, string modelId = null, string apiKey = null, string url = "https://api1.www.3drepo.io/api")
        {
            m_AdapterSettings.DefaultPushType = oM.Adapter.PushType.CreateOnly;

            if (string.IsNullOrWhiteSpace(teamspace) || string.IsNullOrWhiteSpace(modelId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(url))
            {
                BH.Engine.Reflection.Compute.RecordWarning("Some of the required inputs to connect to the 3D Repo server is missing or invalid.\n" +
                    "You can still use the Adapter with the Execute action, which allows you to save a .BIM file that you can upload manually on 3DRepo.");
            }

            BH.Engine.Reflection.Compute.RecordNote($"Establishing repo controller with:\n URL: {url}\nApi key: {apiKey}\nTeamspace: {teamspace}\nModelID: {modelId}");

            controller = new RepoController(url, apiKey, teamspace, modelId);

            AdapterIdName = BH.Engine.TDRepo.Convert.AdapterIdName;   //Set the "AdapterId" to "SoftwareName_id". Generally stored as a constant string in the convert class in the SoftwareName_Engine
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        //Add any comlink object as a private field here, example named:

        private RepoController controller;


        /***************************************************/


    }
}

