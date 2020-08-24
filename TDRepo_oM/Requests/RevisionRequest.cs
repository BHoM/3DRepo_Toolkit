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
    [Description("Retrieves Revisions.")] // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    public class RevisionRequest : IUserAPIKeyRequest, ITeamSpaceRequest, IModelIdRequest  
    {
        [Description("If nothing is specified, query all the revisions.")]
        public List<string> RevisionId { get; set; } = new List<string>();

        [Description("If nothing is specified, takes the one specified in the Adapter.")]
        public string ModelId { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public string TeamSpace { get; set; } = null;

        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        public string UserAPIKey { get; set; } = null;
    }
}

