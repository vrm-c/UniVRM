using System;
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

            if (awaitCaller == null)
            {
                Debug.LogWarning("GltfUtility.LoadAsync: awaitCaller argument is null. ImmediateCaller is used as the default fallback. When playing, we recommend RuntimeOnlyAwaitCaller.");
                awaitCaller = new ImmediateCaller();
            }

            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: materialGenerator))
            {
                return await loader.LoadAsync(awaitCaller);
            }
        }

        public static async Task<RuntimeGltfInstance> LoadBytesAsync(string path, byte[] bytes, IAwaitCaller awaitCaller = null, IMaterialDescriptorGenerator materialGenerator = null)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (awaitCaller == null)
            {
                Debug.LogWarning("GltfUtility.LoadAsync: awaitCaller argument is null. ImmediateCaller is used as the default fallback. When playing, we recommend RuntimeOnlyAwaitCaller.");
                awaitCaller = new ImmediateCaller();
            }

            using (GltfData data = new GlbBinaryParser(bytes, path).Parse())
            using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: materialGenerator))
            {
                return await loader.LoadAsync(awaitCaller);
            }
        }
    }
}
