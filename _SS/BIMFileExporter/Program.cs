using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Rhinoceros;
using BH.Adapter.TDRepo;
using BH.oM.Base;

namespace BIMFileExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Rhino.Geometry.Point3d pt0 = new Rhino.Geometry.Point3d(0, 0, 0);
            //Rhino.Geometry.Point3d pt1 = new Rhino.Geometry.Point3d(10, 10, 10);
            //Rhino.Geometry.BoundingBox box = new Rhino.Geometry.BoundingBox(pt0, pt1);

            //Rhino.Geometry.Mesh rhinoMesh = Rhino.Geometry.Mesh.CreateFromBox(box, 1, 1, 1); // 1 face in each direction
            //BH.oM.Geometry.Mesh bhomMesh = rhinoMesh.FromRhino();

            //BH.Adapter.TDRepo.TDRepoAdapter.WriteBIMFile(new List<IObject>() { bhomMesh });

        }
    }
}

