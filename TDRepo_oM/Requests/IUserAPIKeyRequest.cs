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
    public interface IUserAPIKeyRequest : IRequest
    {
        [Description("If nothing is specified, takes the one specified in the Adapter. Otherwise, this takes precedence.")]
        string UserAPIKey { get; set; }
    }
}
