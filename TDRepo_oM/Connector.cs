
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BH.oM.TDRepo
{
    class Connector
    {
        internal static void NewRevision(string host, string apiKey, string teamspace, string modelId, string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("file", new MultipartForm.FileParameter(data, filePath, "application/octet-stream"));

            
            string uri = host + "/" + teamspace + "/" + modelId + "/upload?key=" + apiKey;
            Logger.Instance.Log("Posting a new revision at : " + uri);
            // Create request and receive response
            HttpWebResponse webResponse = MultipartForm.MultipartFormDataPost(uri, null, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();
            if (fullResponse.Contains("The remote server returned an error"))
            {
                Logger.Instance.Log("Errored: " + fullResponse);
                throw new Exception(fullResponse);
            }
            
        }
    }
}
