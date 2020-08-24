using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Inspection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.TDRepo.Requests
{
    [Description("Retrieves issues.")] // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    public class AuditRequest : IUserAPIKeyRequest, ITeamSpaceRequest, IModelIdRequest
    {
        [Description("Audit whose information you wish to Pull from 3DRepo.")]
        public virtual Audit Audit { get; set; } = null;

        [Description("If nothing is specified, takes the latest revision from the Model.")]
        public virtual string RevisionId { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public virtual string ModelId { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public virtual string TeamSpace { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public virtual string UserAPIKey { get; set; } = null;
    }
}
