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
        private const string BOUNDING_BOX_MIN_FIELD = "min";
        private const string BOUNDING_BOX_MAX_FIELD = "max";

        /// <summary>
        /// Convert modelItem bounding box to JSON object
        /// </summary>
        /// <returns></returns>
        public static JObject SerialisedBBox(this BoundingBox bbox)
        {
            JObject bboxJSON = new JObject();
            JArray bboxMin = new JArray();
            bboxMin.Add(bbox.Min.X); // + (center.X - size.X * 0.5));
            bboxMin.Add(bbox.Min.Y); // + (center.Y - size.Y * 0.5));
            bboxMin.Add(bbox.Min.Z); //+ (center.Z - size.Z * 0.5));

            JArray bboxMax = new JArray();
            bboxMax.Add(bbox.Max.X); //+ (center.X - size.X * 0.5));
            bboxMax.Add(bbox.Max.Y); //+ (center.Y - size.Y * 0.5));
            bboxMax.Add(bbox.Max.Z); // + (center.Z - size.Z * 0.5));

            bboxJSON[BOUNDING_BOX_MIN_FIELD] = bboxMin;
            bboxJSON[BOUNDING_BOX_MAX_FIELD] = bboxMax;

            return bboxJSON;
        }
    }
}