using BH.oM.External.TDRepo;
using RepoFileExporter.dataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.TDrepo
{
    public static class Convert
    {
        public static Geometry MeshToGeometry(this BH.oM.Geometry.Mesh mesh, int materialIdx = 1)
        {
            // Make sure input mesh is triangle mesh
            mesh = BH.Engine.Geometry.Modify.Triangulate(mesh);

            //Geometry testGeometry = new Geometry(
            //        new List<double> {
            //            0, 0, 0,
            //            1, 0, 0,
            //            1, 1, 0,
            //            0, 1, 0 },
            //        new List<uint> { 0, 1, 2, 0, 2, 3 }, null,
            //        materialIdx);

            Geometry geometry = null;

            List<double> pointsCoords = new List<double>();
            mesh.Vertices.ForEach(v => {
                pointsCoords.Add(v.X);
                pointsCoords.Add(v.Y);
                pointsCoords.Add(v.Z);
            });

            List<uint> faces = new List<uint>();
            mesh.Faces.ForEach(f => {
                faces.Add((uint)f.A);
                faces.Add((uint)f.B);
                faces.Add((uint)f.C);
            });

            geometry = new Geometry(pointsCoords, faces, null, materialIdx);

            return geometry;
        }
    }
}
