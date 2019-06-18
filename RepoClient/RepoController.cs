namespace ThreeDRepo
{
    public class RepoController
    {
        public RepoController(string host, string apiKey, string teamspace, string modelId)
        {
            this.host = host;
            this.apiKey = apiKey;
            sceneCreator = new SceneCreator();
        }

        public void AddToScene(Mesh mesh)
        {
            sceneCreator.Add(mesh);
        }

        public void Commit() {
            var filePath = sceneCreator.CreateFile();
            //WebRequest.NewRevision(host, apiKey, teamspace, modelId, filePath);
            sceneCreator.Clear();
        }

        private string host;
        private string apiKey;
        private string teamspace;
        private string modelId;
        private SceneCreator sceneCreator;

    }
}
