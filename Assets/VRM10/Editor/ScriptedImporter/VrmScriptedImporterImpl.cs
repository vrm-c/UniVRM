using System.Linq;
using UnityEngine;
using UniGLTF;
using System;
using VRM10.Settings;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public static class VrmScriptedImporterImpl
    {
        static void Process(Vrm10Data result, ScriptedImporter scriptedImporter, AssetImportContext context, ImporterRenderPipelineTypes renderPipeline)
        {
            //
            // Import(create unity objects)
            //
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .Where(kv => kv.Value != null)
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            var materialGenerator = GetMaterialDescriptorGenerator(renderPipeline);

            using (var loader = new Vrm10Importer(result, externalObjectMap: extractedObjects, materialGenerator: materialGenerator))
            {
                // settings TextureImporters
                foreach (var textureInfo in loader.TextureDescriptorGenerator.Get().GetEnumerable())
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalTextures);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptedImporter"></param>
        /// <param name="context"></param>
        /// <param name="doMigrate">vrm0 だった場合に vrm1 化する</param>
        /// <param name="renderPipeline"></param>
        /// <param name="doNormalize">normalize する</param>
        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool doMigrate, ImporterRenderPipelineTypes renderPipeline)
        {
            if (Symbols.VRM_DEVELOP)
            {
                Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
            }

            // 1st parse as vrm1
            using (var data = new GlbFileParser(scriptedImporter.assetPath).Parse())
            {
                var vrm1Data = Vrm10Data.Parse(data);
                if (vrm1Data != null)
                {
                    // successfully parsed vrm-1.0
                    Process(vrm1Data, scriptedImporter, context, renderPipeline);
                }

                if (!doMigrate)
                {
                    return;
                }

                // try migration...
                MigrationData migration;
                using (var migrated = Vrm10Data.Migrate(data, out vrm1Data, out migration))
                {
                    if (vrm1Data != null)
                    {
                        Process(vrm1Data, scriptedImporter, context, renderPipeline);
                    }
                }

                // fail to migrate...
                if (migration != null)
                {
                    Debug.LogWarning(migration.Message);
                }
                return;
            }
        }

        private static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(ImporterRenderPipelineTypes renderPipeline)
        {
            var settings = Vrm10ProjectEditorSettings.instance;
            if (settings.MaterialDescriptorGeneratorFactory != null)
            {
                return settings.MaterialDescriptorGeneratorFactory.Create();
            }

            return renderPipeline switch
            {
                ImporterRenderPipelineTypes.Auto => Vrm10MaterialDescriptorGeneratorUtility.GetValidVrm10MaterialDescriptorGenerator(),
                ImporterRenderPipelineTypes.BuiltinRenderPipeline => Vrm10MaterialDescriptorGeneratorUtility.GetVrm10MaterialDescriptorGenerator(RenderPipelineTypes.BuiltinRenderPipeline),
                ImporterRenderPipelineTypes.UniversalRenderPipeline => Vrm10MaterialDescriptorGeneratorUtility.GetVrm10MaterialDescriptorGenerator(RenderPipelineTypes.UniversalRenderPipeline),
                _ => Vrm10MaterialDescriptorGeneratorUtility.GetValidVrm10MaterialDescriptorGenerator(),
            };
        }
    }
}