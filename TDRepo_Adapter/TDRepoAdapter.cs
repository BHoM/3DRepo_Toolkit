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
using BH.Adapter;
using BH.oM.Adapters.TDRepo;
using BH.oM.Base;
using BH.oM.Base.Attributes;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        //Add any applicable constructors here, such as linking to a specific file or anything else as well as linking to that file through the (if existing) com link via the API
        [Input("teamspace", "Name of the teamspace owning the Model.")]
        [Input("modelId", "Id of the model within the teamspace. Can be found on the Dashboard website by selecting 'Model Settings'.")]
        [Input("userAPIKey", "User API key to allow the upload through the 3D Repo Web API.\n" +
            "This must be generated per individual user from the Dashboard website: go in 'User Profile' to find it.")] // Also see https://www.youtube.com/watch?v=prio5r5zGGc or https://3drepo.github.io/3drepo.io/#api-User-generateApiKey
        [Input("APIUrl", "Address of the API web server.")]
        public TDRepoAdapter(string teamspace = null, string modelId = null, string userAPIKey = null, string APIUrl = "https://api1.www.3drepo.io/api")
        {
            m_AdapterSettings.DefaultPushType = oM.Adapter.PushType.CreateOnly;

            if (string.IsNullOrWhiteSpace(teamspace) || string.IsNullOrWhiteSpace(modelId) || string.IsNullOrWhiteSpace(userAPIKey) || string.IsNullOrWhiteSpace(APIUrl))
            {
                BH.Engine.Base.Compute.RecordWarning("Some of the required inputs to connect to the 3D Repo server are missing or invalid.\n" +
                    "You can still use the Adapter with the Execute action, which allows you to save a .BIM file that you can upload manually on 3DRepo.");
            }
            else
                BH.Engine.Base.Compute.RecordNote($"Note: you should be using your own `userAPIKey`, see input description.\nSharing the user API Key is DANGEROUS.\nIf you didn't input your own key, you might be doing unauthorized changes.");

            m_host = APIUrl;
            m_userAPIKey = userAPIKey;
            m_teamspace = teamspace;
            m_modelId = modelId;

            AdapterIdFragmentType = typeof(TDRepoId); 
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        private string m_host;
        private string m_userAPIKey;
        private string m_teamspace;
        private string m_modelId;
        private static Encoding m_encoding = Encoding.UTF8;

        List<BH.oM.Adapters.TDRepo.TDR_Mesh> m_3DRepoMeshesForOBJexport = new List<BH.oM.Adapters.TDRepo.TDR_Mesh>();


        /***************************************************/
    }
}





