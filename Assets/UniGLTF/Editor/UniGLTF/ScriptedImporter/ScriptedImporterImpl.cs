using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
            using (var loader = new ImporterContext(parser, GetExternalTextures(scriptedImporter, parser)))
            {
                // settings TextureImporters
                foreach (var (key, textureInfo) in GltfTextureEnumerator.EnumerateAllTexturesDistinct(parser))
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalTextures);
                }

                loader.InvertAxis = reverseAxis;
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

        private static IReadOnlyDictionary<SubAssetKey, Object> GetExternalTextures(ScriptedImporter scriptedImporter, GltfParser parser)
        {
            // 2 回目以降の Asset Import において、 Importer の設定で Extract した Textures が入る
            var externalTextures = scriptedImporter.GetExternalObjectMap()
                .Where(kv => kv.Value is Texture)
                .ToDictionary(kv => kv.Value.TextureSubAssetKey(), kv => kv.Value);

            // return externalTextures;

            var assetDirectoryPath = UnityPath.FromUnityPath(scriptedImporter.assetPath).Parent;
            foreach (var (key, obj) in EnumerateExternalTexturesReferencedByMaterials(parser, assetDirectoryPath))
            {
                if (!externalTextures.ContainsKey(key))
                {
                    externalTextures.Add(key, obj);
                }
            }

            return externalTextures;
        }

        /// <summary>
        /// glTF image の uri が参照する外部テクスチャファイルのうち、存在するもの
        /// </summary>
        private static IEnumerable<(SubAssetKey, Object)> EnumerateExternalTexturesReferencedByMaterials(GltfParser parser, UnityPath dir)
        {
            var used = new HashSet<Texture2D>();
            foreach (var (key, texParam) in GltfTextureEnumerator.EnumerateAllTexturesDistinct(parser))
            {
                if (string.IsNullOrEmpty(texParam.Uri)) continue;
                if (texParam.Uri.StartsWith("data:")) continue;

                var texPath = dir.Child(texParam.Uri);
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath.Value);
                if (asset == null)
                {
                    throw new System.IO.FileNotFoundException($"{texPath}");
                }
                if (used.Add(asset))
                {
                    yield return (key, asset);
                }
            }
        }

        private static SubAssetKey TextureSubAssetKey(this Object obj)
        {
            return new SubAssetKey(typeof(Texture), obj.name);
        }
    }
}
