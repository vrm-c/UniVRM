using System.Linq;
using UnityEngine;
using UniGLTF;
using System.IO;
using System;
using UniJSON;
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
        public static string TryParseOrMigrate(string path, bool migrateToVrm1, out GltfParser parser)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            parser = new GltfParser();
            parser.ParsePath(path);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                // success
                return default;
            }

            if (!migrateToVrm1)
            {
                return "vrm1 not found";
            }

            // try migrateion            
            Byte[] migrated = default;
            try
            {
                var src = File.ReadAllBytes(path);
                var glb = UniGLTF.Glb.Parse(src);
                var json = glb.Json.Bytes.ParseAsJson();
                if (!json.TryGet("extensions", out JsonNode extensions))
                {
                    return "no gltf.extensions";
                }
                if (!extensions.TryGet("VRM", out JsonNode vrm0))
                {
                    return "vrm0 not found";
                }

                migrated = MigrationVrm.Migrate(json, glb.Binary.Bytes);
                if (migrated == null)
                {
                    return "cannot migrate";
                }
            }
            catch (Exception)
            {
                return "migration error";
            }

            parser = new GltfParser();
            parser.Parse(path, migrated);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out vrm))
            {
                // success
                return default;
            }

            parser = default;
            return "migrate but no vrm1. unknown";
        }

        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1)
        {
#if VRM_DEVELOP
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            var message = TryParseOrMigrate(scriptedImporter.assetPath, migrateToVrm1, out GltfParser parser);
            if (!string.IsNullOrEmpty(message))
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
