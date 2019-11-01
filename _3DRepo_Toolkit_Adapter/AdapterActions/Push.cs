using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine._3DRepo_Toolkit;
using BH.oM.Base;
using ThreeDRepo;

namespace BH.Adapter.ThreeDRepo
{
    public partial class RepoAdapter : BHoMAdapter
    {
        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            Create(objects);
            return base.Push(objects, tag, config);
        }
    }
}
