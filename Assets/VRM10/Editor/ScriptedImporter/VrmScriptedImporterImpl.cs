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
        /// <summary>
        /// VRM1 で　パースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="migrateToVrm1"></param>
        /// <returns></returns>
        public static bool TryParseOrMigrate(string path, bool migrateToVrm1, out GltfParser parser, out string error)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            parser = new GltfParser();
            parser.ParsePath(path);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                // success
                error = default;
                return true;
            }

            if (!migrateToVrm1)
            {
                error = "vrm1 not found";
                return false;
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
                    error = "no gltf.extensions";
                    return false;
                }
                if (!extensions.TryGet("VRM", out JsonNode vrm0))
                {
                    error = "vrm0 not found";
                    return false;
                }

                migrated = MigrationVrm.Migrate(json, glb.Binary.Bytes);
                if (migrated == null)
                {
                    error = "cannot migrate";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = $"migration error: {ex}";
                return false;
            }

            parser = new GltfParser();
            parser.Parse(path, migrated);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out vrm))
            {
                // success
                error = default;
                return true;
            }

            error = "migrate but no vrm1. unknown";
            return false;
        }

        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1)
        {
#if VRM_DEVELOP
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            if (!TryParseOrMigrate(scriptedImporter.assetPath, migrateToVrm1, out GltfParser parser, out string message))
            {
                // fail to parse vrm1
                return;
            }

            //
            // Import(create unity objects)
            //
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            using (var loader = new Vrm10Importer(parser, extractedObjects))
            {
                // settings TextureImporters
                foreach (var textureInfo in loader.TextureDescriptorGenerator.Get().GetEnumerable())
                {
                    VRMShaders.TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalTextures);
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
