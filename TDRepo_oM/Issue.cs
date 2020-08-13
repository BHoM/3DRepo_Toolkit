using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.External.TDRepo
{
    public class Issue : IObject // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    {
        [Description("The name of the issue.")]
        public virtual string Name { get; set; }

        [Description("The roles assigned to the issue. " +
            "Even though its an array (for future support of multiple assigned jobs), " +
            "currently it has one or none elements correspoing to the available jobs in the teamaspace.")]
        public virtual List<string> AssignedRoles { get; set; } = new List<string>();

        [Description("The status of the issue. It can have a value of `open`,`in progress`,`for approval` or `closed`.")]
        public virtual string Status { get; set; }

        [Description("The priority of the issue. It can have a value of `none`, `low`, `medium` or `high`.")]
        public virtual string Priority { get; set; }

        [Description("Topic type of the issue. Its value has to be one of the defined topic_types for the model." +
            "\nDefaults to `unassigned`." +
            "\nSee https://3drepo.github.io/3drepo.io/#api-Model-createModel for more details.")]
        public virtual string TopicType { get; set; } = "unassigned";

        [Description("The viewpoint of the issue, defining the position of the camera and the screenshot for that position.")]
        public virtual Viewpoint Viewpoint { get; set; }

        [Description("The description of the created issue.")]
        public virtual string Description { get; set; }

        [Description("The vector defining the pin of the issue (X, Y, Z).")]
        public virtual double[] Position { get; set; }
    }
}
