using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDRepo
{
    public class RepoController
    {
        public RepoController(string host, string apiKey) {
            this.host = host;
            this.apiKey = apiKey;

        }

        private string host;
        private string apiKey;
    }
}
