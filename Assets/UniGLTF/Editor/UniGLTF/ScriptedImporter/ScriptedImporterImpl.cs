using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

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
        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, Axises reverseAxis)
        {
#if VRM_DEVELOP            
            Debug.Log("OnImportAsset to " + context.assetPath);
#endif

            var loaded = Load(context.assetPath,
                scriptedImporter.GetExternalObjectMap(),
                reverseAxis
            );

            AddSubAssets(context, loaded);
        }

        static IEnumerable<(string, UnityEngine.Object)> EnumerateExists(Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> exclude,
            GltfParser parser, UnityPath dir)
        {
            foreach (var texParam in parser.EnumerateTextures())
            {
                switch (texParam.TextureType)
                {
                    case GetTextureParam.METALLIC_GLOSS_PROP:
                    case GetTextureParam.OCCLUSION_PROP:
                        break;

                    default:
                        {
                            var gltfTexture = parser.GLTF.textures.First(y => y.name == texParam.Name);
                            var gltfImage = parser.GLTF.images[gltfTexture.source];
                            if (!string.IsNullOrEmpty(gltfImage.uri))
                            {
                                var child = dir.Child(gltfImage.uri);
                                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(child.Value);
                                // Debug.Log($"exists: {child}: {asset}");
                                if (!exclude.Any(kv => kv.Value.name == asset.name))
                                {
                                    yield return (asset.name, asset);
                                }
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Parse して、UnityObject 化する。
        /// 
        /// TODO: すべての UnityObject を ImporterContext が所有する。(Disposeで削除できる)
        /// </summary>
        /// <param name="assetPath">glbのパス</param>
        /// <param name="externalObjectMap">ScriptedImporter外部に作成済みのAssetへの参照</param>
        /// <param name="reverseAxis">gltf から unityへの座標変換オプション</param>
        /// <returns></returns>
        static ImporterContext Load(string assetPath, Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> externalObjectMap, Axises reverseAxis)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            var parser = new GltfParser();
            parser.ParsePath(assetPath);

            //
            // Import(create unity objects)
            //
            var context = new ImporterContext(parser, null,
                externalObjectMap.Where(x => x.Value != null).Select(x => (x.Value.name, x.Value)).Concat(
                EnumerateExists(externalObjectMap, parser, UnityPath.FromUnityPath(assetPath).Parent)));
            context.InvertAxis = reverseAxis;
            context.Load();
            context.ShowMeshes();

            return context;
        }

        /// <summary>
        /// UnityObjectをSubAsset化する。
        /// 
        /// TODO: SubAsset化されると、ImporterContext から所有権を除去する
        /// </summary>
        /// <param name="context"></param>
        /// <param name="loaded"></param>
        static void AddSubAssets(AssetImportContext context, ImporterContext loaded)
        {
            // Texture
            foreach (var info in loaded.TextureFactory.Textures)
            {
                if (info.IsSubAsset)
                {
                    var texture = info.Texture;
                    context.AddObjectToAsset(texture.name, texture);
                }
            }

            // Material
            foreach (var info in loaded.MaterialFactory.Materials)
            {
                if (info.IsSubAsset)
                {
                    var material = info.Asset;
                    context.AddObjectToAsset(material.name, material);
                }
            }

            // Mesh
            foreach (var mesh in loaded.Meshes.Select(x => x.Mesh))
            {
                // all mesh is subasset
                context.AddObjectToAsset(mesh.name, mesh);
            }

            // Animation
            foreach (var clip in loaded.AnimationClips)
            {
                // all animation is subasset
                context.AddObjectToAsset(clip.name, clip);
            }

            // Root GameObject is main object
            context.AddObjectToAsset(loaded.Root.name, loaded.Root);
            context.SetMainObject(loaded.Root);
        }
    }
}
