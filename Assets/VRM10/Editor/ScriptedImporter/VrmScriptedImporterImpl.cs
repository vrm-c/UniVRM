using System.Linq;
using UnityEngine;
using UniGLTF;
using System.IO;
using System;
using UniJSON;
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
        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1)
        {
#if VRM_DEVELOP
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            if (!Vrm10Parser.TryParseOrMigrate(scriptedImporter.assetPath, migrateToVrm1, out Vrm10Parser.Result result, out string message))
            {
                // fail to parse vrm1
                return;
            }

            //
            // Import(create unity objects)
            //
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            using (var loader = new Vrm10Importer(result.Parser, result.Vrm, extractedObjects))
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

                context.AddObjectToAsset(loaded.name, loaded.gameObject);
                context.SetMainObject(loaded.gameObject);
            }
        }
    }
}
