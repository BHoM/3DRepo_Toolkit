namespace BH.oM.TDRepo
{
    public class RepoController
    {
        public RepoController(string host, string apiKey, string teamspace, string modelId)
        {
            this.host = host;
            this.apiKey = apiKey;
            this.teamspace = teamspace;
            this.modelId = modelId;
            sceneCreator = new SceneCreator();
        }

        public void AddToScene(Mesh mesh)
        {
            Logger.Instance.Log("Adding mesh to scene.");
            sceneCreator.Add(mesh);
        }

        public bool Commit(ref string error) {
            Logger.Instance.Log("Creating file...");
            var filePath = sceneCreator.CreateFile();
            Logger.Instance.Log("Created file at : " + filePath);
            bool success;
            try
            {
                Connector.NewRevision(host, apiKey, teamspace, modelId, filePath);
                success = true;
            } catch(System.Exception e)
            {
                error = e.ToString();
                success = false;
            }
            
            sceneCreator.Clear();
            return success;
        }

        private string host;
        private string apiKey;
        private string teamspace;
        private string modelId;
        private SceneCreator sceneCreator;

    }
}
