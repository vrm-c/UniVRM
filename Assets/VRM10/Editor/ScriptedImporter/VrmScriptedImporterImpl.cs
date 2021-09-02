using System.Linq;
using UnityEngine;
using UniGLTF;
using System;
using VRMShaders;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public static class VrmScriptedImporterImpl
    {
        static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(RenderPipelineTypes renderPipeline)
        {
            switch (renderPipeline)
            {
                case RenderPipelineTypes.BuiltinRenderPipeline:
                    return new Vrm10MaterialDescriptorGenerator();

                case RenderPipelineTypes.UniversalRenderPipeline:
                    return new Vrm10UrpMaterialDescriptorGenerator();

                default:
                    throw new NotImplementedException();
            }
        }

        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1, RenderPipelineTypes renderPipeline)
        {
#if VRM_DEVELOP
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            if (!Vrm10Data.TryParseOrMigrate(scriptedImporter.assetPath, migrateToVrm1, out Vrm10Data result))
            {
                // fail to parse vrm1
                return;
            }

            //
            // Import(create unity objects)
            //
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .Where(kv => kv.Value != null)
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            var materialGenerator = GetMaterialDescriptorGenerator(renderPipeline);

            using (var loader = new Vrm10Importer(result, extractedObjects, materialGenerator: materialGenerator))
            {
                // settings TextureImporters
                foreach (var textureInfo in loader.TextureDescriptorGenerator.Get().GetEnumerable())
                {
                    VRMShaders.TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalTextures);
                }

                var loaded = loader.Load();
                loaded.ShowMeshes();

                loaded.TransferOwnership((key, o) =>
                {
                    context.AddObjectToAsset(key.Name, o);
                });
                var root = loaded.Root;
                GameObject.DestroyImmediate(loaded);

                context.AddObjectToAsset(root.name, root);
                context.SetMainObject(root);
            }
        }
    }
}
