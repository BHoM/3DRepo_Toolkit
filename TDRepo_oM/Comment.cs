using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.TDRepo
{
    public class Comment
    {
        public string guid { get; set; }
        public object created { get; set; }
        public string owner { get; set; }
        public string comment { get; set; }
        public Viewpoint viewpoint { get; set; }
        public Action action { get; set; }
    }
}
