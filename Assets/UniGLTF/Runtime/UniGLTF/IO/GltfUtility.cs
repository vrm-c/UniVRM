using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class GltfUtility
    {
        public static async Task<RuntimeGltfInstance> LoadAsync(string path, IAwaitCaller awaitCaller = null, IMaterialDescriptorGenerator materialGenerator = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            Debug.LogFormat("{0}", path);
            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: materialGenerator))
            {
                return await loader.LoadAsync(awaitCaller);
            }
        }
    }
}
