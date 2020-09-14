using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.TDRepo
{
    public class Resource
    {
        public string _id { get; set; }
        public string name { get; set; }
        public int createdAt { get; set; }
        public int size { get; set; }
    }
}
