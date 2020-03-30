using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Geometry;

namespace BH.Adapter.TDRepo
{
    public static partial class Convert
    {
        public static JObject SerialiseBIMGeometry(BH.oM.TDRepo.BIMGeom geom)
        {
            JObject geomObj = null; // new JObject();

            // If we have some triangles
            if (geom != null && geom.triangleIndices.Count > 0)
            {
                geomObj = new JObject();

                geomObj["bbox"] = geom.BoundingBox.SerialisedBBox();

                geomObj["numIndices"] = geom.triangleIndices.Count;
                geomObj["numVertices"] = geom.vertices.Count / 3;

                geomObj["indices"] = new JArray(geom.triangleOffsets); //indicesOffsets; // addGeometry<uint>(geom.triangleIndices, sizeof(uint));
                geomObj["vertices"] = new JArray(geom.vertexOffsets); // addGeometry<float>(geom.vertices, sizeof(float));
                geomObj["normals"] = new JArray(geom.normalOffsets); //addGeometry<float>(geom.normals, sizeof(float));
            }

            return geomObj;
        }
    }
}