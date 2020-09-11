using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.TDRepo.Requests
{
    [Description("Retrieves issues.")] // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    public class IssueRequest : IUserAPIKeyRequest, ITeamSpaceRequest, IModelIdRequest 
    {
        [Description("If nothing is specified, query all the Issues.")]
        public virtual string IssueId { get; set; } = null;

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
