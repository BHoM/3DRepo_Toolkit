using BH.oM.Adapter;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.External.TDRepo.Commands
{
    [Description("")]
    public class WriteBIMFileCommand : IExecuteCommand
    {
        public string Filepath { get; set; } = null;
        public string Filename { get; set; } = null;
        public List<IObject> objectsToWrite { get; set; }
    }
}
