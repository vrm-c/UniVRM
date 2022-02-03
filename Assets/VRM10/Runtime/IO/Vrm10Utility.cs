using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10Utility
    {
        // public delegate IMaterialDescriptorGenerator MaterialGeneratorCallback(VRM.glTF_VRM_extensions vrm);
        public delegate void MetaCallback(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta1, Migration.Vrm0Meta meta0);
        public static async Task<RuntimeGltfInstance> LoadAsync(string path,
            bool doMigrate,
            bool doNormalize,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            MetaCallback metaCallback = null
            )
        {
            using (var data = Vrm10Data.ParseOrMigrate(path, doMigrate, out Vrm10Data vrm1Data, out MigrationData migration))
            {
                if (vrm1Data == null)
                {
                    return default;
                }
                using (var loader = new Vrm10Importer(vrm1Data, materialGenerator: materialGenerator, doNormalize: doNormalize))
                {
                    // migrate しても thumbnail は同じ
                    if (metaCallback != null)
                    {
                        var thumbnail = await loader.LoadVrmThumbnailAsync();
                        metaCallback(thumbnail, vrm1Data.VrmExtension.Meta, migration.OriginalMetaBeforeMigration);
                    }

                    var instance = await loader.LoadAsync(awaitCaller);
                    return instance;
                }
            }
        }
    }
}
