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
using BHG = BH.oM.Geometry;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Structure.Elements;
using BH.Engine.Structure;
using BH.Engine.Rhinoceros;
using BH.oM.Structure.Constraints;

namespace BH.Engine.External.TDRepo
{
    public static partial class Compute
    {

        [Description("Returns a BHoM Mesh representation for the Node based on its DOF, e.g. a box for fully fixed, a cone with sphere on top for pin.")]
        public static BH.oM.Geometry.Mesh MeshRepresentation(this Node node, BH.oM.External.TDRepo.DisplayOptions displayOptions = null, bool isSubObject = false)
        {
            displayOptions = displayOptions ?? new BH.oM.External.TDRepo.DisplayOptions();

            if (node.Position == null)
            {
                Reflection.Compute.RecordError("Specified Node does not have a position defined.");
                return null;
            }

            if (node.Support == null || !displayOptions.Detailed1DElements) // If there is no support information, or by choice...
                return node.Position().MeshRepresentation(displayOptions, isSubObject); // ...just return the representation for the point.

            // -------------------------------------------- //
            // -------- Compute the representation -------- //
            // -------------------------------------------- //

            // Different representation for different DOF type.
            DOFType[] fixedDOFTypes = new[] { oM.Structure.Constraints.DOFType.Fixed, oM.Structure.Constraints.DOFType.FixedNegative, oM.Structure.Constraints.DOFType.FixedPositive,
            oM.Structure.Constraints.DOFType.Spring, oM.Structure.Constraints.DOFType.Friction, oM.Structure.Constraints.DOFType.Damped, oM.Structure.Constraints.DOFType.SpringPositive, oM.Structure.Constraints.DOFType.SpringNegative};

            bool fixedToTranslation = fixedDOFTypes.Contains(node.Support.TranslationX) || fixedDOFTypes.Contains(node.Support.TranslationY) || fixedDOFTypes.Contains(node.Support.TranslationZ);
            bool fixedToRotation = fixedDOFTypes.Contains(node.Support.RotationX) || fixedDOFTypes.Contains(node.Support.RotationY) || fixedDOFTypes.Contains(node.Support.RotationZ);

            if (fixedToTranslation && fixedToRotation)
            {
                // Fully fixed: box
                double boxDims = 0.12 * displayOptions.Element0DScale;

                var centrePoint = node.Position;
                BoundingBox bbox = BH.Engine.Geometry.Create.BoundingBox(
                    new Point() { X = centrePoint.X + 2 * boxDims, Y = centrePoint.Y + 2 * boxDims, Z = centrePoint.Z },
                    new Point() { X = centrePoint.X - 2 * boxDims, Y = centrePoint.Y - 2 * boxDims, Z = centrePoint.Z - 3 * boxDims });

                return MeshRepresentation(bbox);
            }

            if (fixedToTranslation && !fixedToRotation)
            {
                // Pin: cone + sphere
                double radius = 0.12 * displayOptions.Element0DScale;

                CompositeGeometry compositeGeometry = new CompositeGeometry();

                Sphere sphere = BH.Engine.Geometry.Create.Sphere(node.Position(), radius);
                compositeGeometry.Elements.Add(sphere);

                Cone cone = BH.Engine.Geometry.Create.Cone(
                    new Point() { X = node.Position.X, Y = node.Position.Y, Z = node.Position.Z - radius },
                    new Vector() { X = 0, Y = 0, Z = -1 },
                    4 * radius,
                    3 * radius
                    );
                compositeGeometry.Elements.Add(cone);

                return compositeGeometry.MeshRepresentation();
            }

            // Else: we could add more for other DOFs; for now just return a sphere.
            if (isSubObject)
                return null; //do not return spheres if the Nodes are sub-objects (e.g. of a bar)
            else 
                return node.RhinoMeshRepresentation(displayOptions).FromRhino();
        }
    }
}

