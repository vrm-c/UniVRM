using System;
using System.IO;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public static class VrmUtility
    {
        public delegate IMaterialDescriptorGenerator MaterialGeneratorCallback(VRM.glTF_VRM_extensions vrm);
        public delegate void MetaCallback(VRMMetaObject meta);
        public static async Task<RuntimeGltfInstance> LoadAsync(string path,
            IAwaitCaller awaitCaller = null,
            MaterialGeneratorCallback materialGeneratorCallback = null,
            MetaCallback metaCallback = null,
            ITextureDeserializer textureDeserializer = null,
            bool loadAnimation = false
            )
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            if (awaitCaller == null)
            {
                UniGLTFLogger.Warning("VrmUtility.LoadAsync: awaitCaller argument is null. ImmediateCaller is used as the default fallback. When playing, we recommend RuntimeOnlyAwaitCaller.");
                awaitCaller = new ImmediateCaller();
            }

            using (GltfData data = new AutoGltfFileParser(path).Parse())
            {
                try
                {
                    var vrm = new VRMData(data);
                    IMaterialDescriptorGenerator materialGen = default;
                    if (materialGeneratorCallback != null)
                    {
                        materialGen = materialGeneratorCallback(vrm.VrmExtension);
                    }
                    var importerContextSettings = new ImporterContextSettings(loadAnimation);
                    using (var loader = new VRMImporterContext(
                               vrm,
                               textureDeserializer: textureDeserializer,
                               materialGenerator: materialGen,
                               settings: importerContextSettings))
                    {
                        if (metaCallback != null)
                        {
                            var meta = await loader.ReadMetaAsync(awaitCaller, true);
                            metaCallback(meta);
                        }
                        return await loader.LoadAsync(awaitCaller);
                    }
                }
                catch (NotVrm0Exception)
                {
                    // retry
                    UniGLTFLogger.Warning("file extension is vrm. but not vrm ?");
                    using (var loader = new UniGLTF.ImporterContext(data))
                    {
                        return await loader.LoadAsync(awaitCaller);
                    }
                }
            }
        }


        public static async Task<RuntimeGltfInstance> LoadBytesAsync(string path,
            byte[] bytes,
            IAwaitCaller awaitCaller = null,
            MaterialGeneratorCallback materialGeneratorCallback = null,
            MetaCallback metaCallback = null,
            ITextureDeserializer textureDeserializer = null,
            bool loadAnimation = false,
            IVrm0XSpringBoneRuntime springboneRuntime = null
            )
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (awaitCaller == null)
            {
                UniGLTFLogger.Warning("VrmUtility.LoadAsync: awaitCaller argument is null. ImmediateCaller is used as the default fallback. When playing, we recommend RuntimeOnlyAwaitCaller.");
                awaitCaller = new ImmediateCaller();
            }

            using (GltfData data = new GlbBinaryParser(bytes, path).Parse())
            {
                try
                {
                    var vrm = new VRMData(data);
                    IMaterialDescriptorGenerator materialGen = default;
                    if (materialGeneratorCallback != null)
                    {
                        materialGen = materialGeneratorCallback(vrm.VrmExtension);
                    }
                    var importerContextSettings = new ImporterContextSettings(loadAnimation: loadAnimation);
                    using (var loader = new VRMImporterContext(
                               vrm,
                               textureDeserializer: textureDeserializer,
                               materialGenerator: materialGen,
                               settings: importerContextSettings,
                               springboneRuntime: springboneRuntime
                               ))
                    {
                        if (metaCallback != null)
                        {
                            var meta = await loader.ReadMetaAsync(awaitCaller, true);
                            metaCallback(meta);
                        }
                        return await loader.LoadAsync(awaitCaller);
                    }
                }
                catch (NotVrm0Exception)
                {
                    // retry
                    UniGLTFLogger.Warning("file extension is vrm. but not vrm ?");
                    using (var loader = new UniGLTF.ImporterContext(data))
                    {
                        return await loader.LoadAsync(awaitCaller);
                    }
                }
            }
        }
    }
}
