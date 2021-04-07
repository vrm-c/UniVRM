using System.Linq;
using UnityEngine;
using UniGLTF;
using System.IO;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public static class VrmScriptedImporterImpl
    {
        /// <summary>
        /// VRM1 で　パースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="migrateToVrm1"></param>
        /// <returns></returns>
        public static GltfParser Parse(string path, bool migrateToVrm1)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            var parser = new GltfParser();
            parser.ParsePath(path);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                return parser;
            }

            if (migrateToVrm1)
            {
                // try migrateion
                var migrated = MigrationVrm.Migrate(File.ReadAllBytes(path));
                parser = new GltfParser();
                parser.Parse(path, migrated);
                return parser;
            }

            return null;
        }

        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1)
        {
#if VRM_DEVELOP            
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            var parser = Parse(scriptedImporter.assetPath, migrateToVrm1);
            if (parser == null)
            {
                // fail to parse vrm1
                return;
            }

            //
            // Import(create unity objects)
            //
            var externalObjectMap = scriptedImporter.GetExternalObjectMap().Select(kv => (kv.Value.name, kv.Value)).ToArray();

            using (var loader = new RuntimeUnityBuilder(parser, externalObjectMap))
            {
                // settings TextureImporters
                foreach (var textureInfo in Vrm10MToonMaterialImporter.EnumerateAllTexturesDistinct(parser))
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalMap);
                }

                loader.Load();
                loader.ShowMeshes();

                loader.TransferOwnership(o =>
                {
                    context.AddObjectToAsset(o.name, o);
                    if (o is GameObject)
                    {
                        // Root GameObject is main object
                        context.SetMainObject(loader.Root);
                    }

                    return true;
                });
            }
        }
    }
}
