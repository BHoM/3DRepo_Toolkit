using System.Collections.Generic;
using System.IO;

namespace BH.oM.TDRepo
{
    class SceneCreator
    {
        internal SceneCreator() { }

        internal void Add(Mesh mesh) {
            meshes.Add(mesh);
        }

        internal void Clear() {
            meshes.Clear();
        }

        internal string CreateFile() {
            // This current creates a .obj (no .mtl) for the sake of simplicity.
            // For full support this really should be generating a .bim to support rich BIM data.
            string filePath = Path.GetTempPath() + System.Guid.NewGuid() + ".obj";
            using (var file = new System.IO.StreamWriter(filePath))
            {
                int startIdx = 0;
                foreach (var mesh in meshes)
                {
                    Dictionary<int, int> indexToFullIdx = new Dictionary<int, int>();
                    file.WriteLine("o " + mesh.name);

                    int idxCount = 0;
                    foreach (var v in mesh.vertices)
                    {
                        indexToFullIdx[idxCount++] = idxCount + startIdx;
                        file.WriteLine("v " + v.x + " " + v.y + " " + v.z);
                    }

                    foreach (var f in mesh.faces)
                    {
                        string line = "f ";
                        foreach (var index in f.indices) {
                            line += indexToFullIdx[index] + " ";
                        }
                        file.WriteLine(line);

                    }

                    startIdx += idxCount;
                }
            }
            return filePath;
        }

        private List<Mesh> meshes = new List<Mesh>();
    }
}
