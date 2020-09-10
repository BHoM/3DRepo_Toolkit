using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.TDRepo
{
    [Description("Describes an modification done to an existing issue on 3DRepo." +
        "E.g. property = `status`; from = `inactive`; to = `active`.")]
    public class Action
    {
        public string property { get; set; }
        public string from { get; set; }
        public string to { get; set; }
    }
}
