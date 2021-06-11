using System.Linq;
using UnityEngine;
using VRMShaders;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    public static class ScriptedImporterImpl
    {
        /// <summary>
        /// glb をパースして、UnityObject化、さらにAsset化する
        /// </summary>
        /// <param name="scriptedImporter"></param>
        /// <param name="context"></param>
        /// <param name="reverseAxis"></param>
        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, Axes reverseAxis)
        {
#if VRM_DEVELOP
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            //
            // Parse(parse glb, parser gltf json)
            //
            var parser = new GltfParser();
            parser.ParsePath(scriptedImporter.assetPath);

            //
            // Import(create unity objects)
            //

            // 2 回目以降の Asset Import において、 Importer の設定で Extract した UnityEngine.Object が入る
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .Where(x => x.Value != null)
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            using (var loader = new ImporterContext(parser, extractedObjects))
            {
                // Configure TextureImporter to Extracted Textures.
                foreach (var textureInfo in loader.TextureDescriptorGenerator.Get().GetEnumerable())
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalTextures);
                }

                loader.InvertAxis = reverseAxis;
                var loaded = loader.Load();
                loaded.ShowMeshes();

                loaded.TransferOwnership((k, o) =>
                {
                    context.AddObjectToAsset(k.Name, o);
                });
                var root = loaded.Root;
                GameObject.DestroyImmediate(loaded);

                context.AddObjectToAsset(root.name, root);
                context.SetMainObject(root);
            }
        }
    }
}
