using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine._3DRepo_Toolkit;
using BH.oM.Base;
using TDRepo;

namespace BH.Adapter.ThreeDRepo
{
    public partial class TDRepoAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        //Add any applicable constructors here, such as linking to a specific file or anything else as well as linking to that file through the (if existing) com link via the API
        public TDRepoAdapter(string teamspace, string modelId, string apiKey, string url = "https://api1.www.3drepo.io/api")
        {
            m_AdapterSettings.DefaultPushType = oM.Adapter.PushType.CreateOnly;

            Logger.Instance.Log("Establishing repo controller with URL: " + url + " api key: " + apiKey + " teamspace: " + teamspace + "modelID: " + modelId);
            controller = new RepoController(url, apiKey, teamspace, modelId);

            AdapterIdName = BH.Engine._3DRepo_Toolkit.Convert.AdapterIdName;   //Set the "AdapterId" to "SoftwareName_id". Generally stored as a constant string in the convert class in the SoftwareName_Engine
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        //Add any comlink object as a private field here, example named:

        private RepoController controller;


        /***************************************************/


    }
}
