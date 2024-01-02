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
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Adapters.TDRepo.Commands;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter
    {
        public override Output<List<object>, bool> Execute(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            return Execute(command as dynamic, actionConfig);
        }

        public Output<List<object>, bool> Execute(WriteBIMFileCommand cmd, ActionConfig actionConfig = null)
        {
            string filePath = WriteBIMFile(cmd.objectsToWrite, cmd.Filepath, cmd.Filename, cmd.DisplayOptions);

            return new Output<List<object>, bool> { Item1 = new List<object>(){ filePath }, Item2 = true };
        }
    }
}




