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

namespace BH.oM.TDRepo
{
    public class RepoController
    {
        public RepoController(string host, string apiKey, string teamspace, string modelId)
        {
            this.host = host;
            this.apiKey = apiKey;
            this.teamspace = teamspace;
            this.modelId = modelId;
            sceneCreator = new SceneCreator();
        }

        public void AddToScene(Mesh mesh)
        {
            sceneCreator.Add(mesh);
        }

        public bool Commit(ref string error) {
            var filePath = sceneCreator.CreateFile();
            bool success;
            try
            {
                Connector.NewRevision(host, apiKey, teamspace, modelId, filePath);
                success = true;
            } catch(System.Exception e)
            {
                error = e.ToString();
                success = false;
            }
            
            sceneCreator.Clear();
            return success;
        }

        private string host;
        private string apiKey;
        private string teamspace;
        private string modelId;
        private SceneCreator sceneCreator;

    }
}
