using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    /// <summary>
    /// High-level VRM-1.0 loading API.
    /// </summary>
    public static class Vrm10
    {
        public delegate void MetaCallback(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta1, Migration.Vrm0Meta meta0);
        public static async Task<RuntimeGltfInstance> LoadPathAsync(string path,
            bool doMigrate,
            bool doNormalize,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            MetaCallback metaCallback = null,
            CancellationToken ct = default
            )
        {
            return await LoadBytesAsync(path, System.IO.File.ReadAllBytes(path), doMigrate, doNormalize, awaitCaller, materialGenerator, metaCallback, ct);
        }

        public static async Task<RuntimeGltfInstance> LoadBytesAsync(string path, byte[] bytes,
            bool doMigrate,
            bool doNormalize,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            MetaCallback metaCallback = null,
            CancellationToken ct = default
            )
        {
            using (var data = new GlbLowLevelParser(path, bytes).Parse())
            {
                // 1. Try parsing as vrm-1.0
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
                        if (ct.IsCancellationRequested && instance != null)
                        {
                            instance.Dispose();
                        }
                        return instance;
                    }
                }

                if (!doMigrate)
                {
                    return default;
                }

                // 2. Try migration from vrm-0.x into vrm-1.0
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
                            if (ct.IsCancellationRequested && instance != null)
                            {
                                instance.Dispose();
                            }
                            return instance;
                        }
                    }
                }

                // 3. failed
                if (migration != null)
                {
                    Debug.LogWarning(migration.Message);
                }
                return default;
            }
        }
    }
}
