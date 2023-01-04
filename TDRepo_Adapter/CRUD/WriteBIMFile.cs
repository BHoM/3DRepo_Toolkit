/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.Base;
using BH.oM.Base;
using RepoFileExporter;
using RepoFileExporter.dataStructures;
using BH.oM.Adapters.TDRepo;
using BH.oM.Geometry;
using BH.oM.Graphics;
using BH.Engine.Representation;

namespace BH.Adapter.TDRepo
{
    public partial class TDRepoAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private static string WriteBIMFile(List<IObject> objectsToWrite, string directory = null, string fileName = null, RenderMeshOptions renderMeshOptions = null)
        {
            // --------------------------------------------- //
            //                    Set-up                     //
            // --------------------------------------------- //

            directory = directory ?? Path.Combine("C:\\temp", "BIMFileFormat");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            fileName = fileName ?? Guid.NewGuid().ToString();
            string bimFilePath = Path.Combine(directory, fileName + ".bim");

            renderMeshOptions = renderMeshOptions ?? new RenderMeshOptions();

            // --------------------------------------------- //
            //             Compute representation            //
            // --------------------------------------------- //

            List<Mesh> representationMeshes = new List<Mesh>();
            List<Tuple<IObject, Mesh>> objsAndRepresentations = new List<Tuple<IObject, Mesh>>();

            IBHoMObject bHoMObject = null;
            for (int i = 0; i < objectsToWrite.Count; i++)
            {
                IObject obj = objectsToWrite[i];

                Mesh meshRepresentation = null;

                // See if there is a custom BHoM mesh representation for that BHoMObject.
                bHoMObject = obj as IBHoMObject;
                RenderMesh renderMesh = null;

                if (bHoMObject != null)
                    bHoMObject.TryGetRendermesh(out renderMesh);

                if (renderMesh == null && meshRepresentation == null)
                    renderMesh = BH.Engine.Representation.Compute.IRenderMesh(obj, renderMeshOptions);

                if (renderMesh != null) //convert to Mesh
                    meshRepresentation = new Mesh() { Faces = renderMesh.Faces, Vertices = renderMesh.Vertices.Select(v => new oM.Geometry.Point() { X = v.Point.X, Y = v.Point.Y, Z = v.Point.Z }).ToList() };

                representationMeshes.Add(meshRepresentation);

                if (bHoMObject != null)
                {
                    // Add/update the RenderMesh in the BHoMObject
                    bHoMObject.ISetRendermesh(meshRepresentation);
                    obj = bHoMObject;
                }

                objsAndRepresentations.Add(new Tuple<IObject, Mesh>(obj, meshRepresentation));
            }


            // --------------------------------------------- //
            //                File preparation               //
            // --------------------------------------------- //

            BIMDataExporter exporter = new BIMDataExporter();

            // Prepare default material
            TDR_Material defaultMat = new TDR_Material() { MaterialArray = new List<float> { 1f, 1f, 1f, 1f } };
            int defaultMatIdx = exporter.AddMaterial(defaultMat.MaterialArray);

            // Prepare transformation matrix 
            List<float> transfMatrix = new List<float>
                {
                    1, 0, 0, 2,
                    0, 1, 0, 2,
                    0, 0, 1, 2,
                    0, 0, 0, 1
                };

            // Prepare root node
            int rootNodeIdx = exporter.AddNode("root", -1, null);

            // Process the meshes
            for (int i = 0; i < objsAndRepresentations.Count; i++)
            {
                BH.oM.Geometry.Mesh m = representationMeshes[i];
                Tuple<IObject, BH.oM.Geometry.Mesh> objAndRepr = objsAndRepresentations[i];

                // Check if a colour has been specified in the BHoMObject's Fragment
                bHoMObject = objAndRepr.Item1 as IBHoMObject;
                int customMatIdx = defaultMatIdx;
                if (bHoMObject != null)
                {
                    Color? colour = bHoMObject.FindFragment<ColourFragment>()?.Colour;

                    if (colour != null)
                    {
                        Color col = (Color)colour;

                        float r = (float)col.R / 255;
                        float g = (float)col.G / 255;
                        float b = (float)col.B / 255;
                        float a = (float)col.A / 255;

                        TDR_Material customMat = new TDR_Material() { MaterialArray = new List<float> { r, g, b, a } };
                        customMatIdx = exporter.AddMaterial(customMat.MaterialArray);
                    }
                }

                // Convert object representation mesh to a 
                Geometry geometry = BH.Adapter.TDrepo.Convert.MeshToGeometry(objAndRepr.Item2, customMatIdx);

                // Add metadata
                Dictionary<string, RepoVariant> metadata = new Dictionary<string, RepoVariant>();

                // Serialize the object
                string serialisedBHoMData = BH.Engine.Serialiser.Convert.ToJson(objAndRepr.Item1);

                // Flatten the JSON in a Dictionary. Nested properties names are concatenated with fullstops.
                Dictionary<string, object> flattenedObj = BH.Engine.Adapters.TDRepo.Compute.FlattenJsonToDictionary(serialisedBHoMData);

                // For each entry in the flattened object, add a metadata with the value.
                flattenedObj.ToList().ForEach(
                    kv =>
                    {
                        if (kv.Value is int)
                            metadata.Add(kv.Key, RepoVariant.Int((int)kv.Value));
                        else if (kv.Value is double)
                            metadata.Add(kv.Key, RepoVariant.Double((double)kv.Value));
                        else if (kv.Value is bool)
                            metadata.Add(kv.Key, RepoVariant.Boolean((bool)kv.Value));
                        else
                            metadata.Add(kv.Key, RepoVariant.String(kv.Value?.ToString()));
                    }
                );

                metadata.Add("ZippedBHoM", RepoVariant.String(BH.Engine.Serialiser.Convert.ToZip(serialisedBHoMData)));

                //metadata.Add("Area", RepoVariant.Int(1));
                //metadata.Add("Boolean Test", RepoVariant.Boolean(true));
                //metadata.Add("Double", RepoVariant.Double(1.3242524));

                // Add node to exporter.
                exporter.AddNode("mesh" + i, rootNodeIdx, transfMatrix, geometry, metadata);
            }

            exporter.ExportToFile(bimFilePath);

            return bimFilePath;
        }

        private static RenderMesh JoinRenderMeshes(List<RenderMesh> renderMeshes)
        {
            List<RenderPoint> vertices = new List<RenderPoint>();
            List<Face> faces = new List<Face>();

            vertices.AddRange(renderMeshes[0].Vertices);
            faces.AddRange(renderMeshes[0].Faces);

            for (int i = 1; i < renderMeshes.Count; i++)
            {
                int lastVerticesCount = vertices.Count;
                vertices.AddRange(renderMeshes[i].Vertices);
                faces.AddRange(
                    renderMeshes[i].Faces.Select(f =>
                        new Face() { A = f.A + lastVerticesCount, B = f.B + lastVerticesCount, C = f.C + lastVerticesCount, D = f.D == -1 ? f.D : f.D + lastVerticesCount }));
            }

            return new RenderMesh() { Vertices = vertices, Faces = faces };
        }

        private static Mesh JoinMeshes(List<Mesh> meshes)
        {
            List<BH.oM.Geometry.Point> vertices = new List<BH.oM.Geometry.Point>();
            List<Face> faces = new List<Face>();

            vertices.AddRange(meshes[0].Vertices);
            faces.AddRange(meshes[0].Faces);

            for (int i = 1; i < meshes.Count; i++)
            {
                int lastVerticesCount = vertices.Count;
                vertices.AddRange(meshes[i].Vertices);
                faces.AddRange(
                    meshes[i].Faces.Select(f =>
                        new Face() { A = f.A + lastVerticesCount, B = f.B + lastVerticesCount, C = f.C + lastVerticesCount, D = f.D == -1 ? f.D : f.D + lastVerticesCount }));
            }

            return new Mesh() { Vertices = vertices, Faces = faces };
        }
    }
}



