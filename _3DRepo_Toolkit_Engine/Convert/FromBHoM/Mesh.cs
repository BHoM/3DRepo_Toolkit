using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;

namespace BH.Engine._3DRepo_Toolkit
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static TDRepo.Mesh FromBHoM(Mesh mesh)
        {
            var faces = mesh.Faces.Select(face =>
                new TDRepo.Face(new int[]{ face.A, face.B, face.C, face.D })
            );

            var points = mesh.Vertices.Select(vertex =>
                new TDRepo.Point(vertex.X, vertex.Y, vertex.Z)
            );

            return new TDRepo.Mesh("Mesh", points.ToArray(), faces.ToArray());
        }

        /***************************************************/
    }
}
