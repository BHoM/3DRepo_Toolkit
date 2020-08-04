using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.External.TDRepo
{
    public class TDRIssue : BHoMObject
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Priority { get; set; }
        public virtual string Status { get; set; }
        public virtual List<string> AssignedRoles { get; set; } = new List<string>();

        [Description("Topic type of the issue.")]
        public virtual string Type { get; set; }

        public virtual object DueDate { get; set; }
        public virtual string Description { get; set; }

        [Description("X, Y, Z values.")]
        public virtual string[] Position { get; set; }

        [Description("Issue View Point. This includes a path to the file containing an image picturing the issue.")]
        public virtual string IssueViewPoint { get; set; }
    }
}
