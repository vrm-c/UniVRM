using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10Utility
    {
        public delegate void MetaCallback(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta1, Migration.Vrm0Meta meta0);
        public static async Task<RuntimeGltfInstance> LoadPathAsync(string path,
            bool doMigrate,
            bool doNormalize,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            MetaCallback metaCallback = null
            )
        {
            return await LoadBytesAsync(path, System.IO.File.ReadAllBytes(path), doMigrate, doNormalize, awaitCaller, materialGenerator, metaCallback);
        }

        public static async Task<RuntimeGltfInstance> LoadBytesAsync(string path, byte[] bytes,
            bool doMigrate,
            bool doNormalize,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            MetaCallback metaCallback = null
            )
        {
            // 1st parse as vrm1
            using (var data = new GlbLowLevelParser(path, bytes).Parse())
            {
                var vrm1Data = Vrm10Data.Parse(data);
                if (vrm1Data != null)
                {
                    // successfully parsed vrm-1.0
                    using (var loader = new Vrm10Importer(vrm1Data, materialGenerator: materialGenerator, doNormalize: doNormalize))
                    {
                        if (metaCallback != null)
                        {
                            var thumbnail = await loader.LoadVrmThumbnailAsync();
                            metaCallback(thumbnail, vrm1Data.VrmExtension.Meta, null);
                        }
                        var instance = await loader.LoadAsync(awaitCaller);
                        return instance;
                    }
                }

                if (!doMigrate)
                {
                    return default;
                }

                // try migration...
                MigrationData migration;
                using (var migrated = Vrm10Data.Migrate(data, out vrm1Data, out migration))
                {
                    if (vrm1Data != null)
                    {
                        // successfully migrated from vrm-0.x
                        using (var loader = new Vrm10Importer(vrm1Data, materialGenerator: materialGenerator, doNormalize: doNormalize))
                        {
                            if (metaCallback != null)
                            {
                                var thumbnail = await loader.LoadVrmThumbnailAsync();
                                metaCallback(thumbnail, vrm1Data.VrmExtension.Meta, null);
                            }
                            var instance = await loader.LoadAsync(awaitCaller);
                            return instance;
                        }
                    }
                }

                // fail to migrate...
                if (migration != null)
                {
                    Debug.LogWarning(migration.Message);
                }
                return default;
            }
        }
    }
}
