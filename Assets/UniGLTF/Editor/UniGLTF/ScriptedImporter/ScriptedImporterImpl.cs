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
            var externalObjectMap = scriptedImporter.GetExternalObjectMap();

            using (var loaded = new ImporterContext(parser, null,
                externalObjectMap.Where(x => x.Value != null).Select(x => (x.Value.name, x.Value)).Concat(
                EnumerateTexturesFromUri(externalObjectMap, parser, UnityPath.FromUnityPath(scriptedImporter.assetPath).Parent))))
            {
                loaded.InvertAxis = reverseAxis;
                loaded.Load();
                loaded.ShowMeshes();

                loaded.TransferOwnership(o =>
                {
#if VRM_DEVELOP
                    Debug.Log($"[{o.GetType().Name}] {o.name} will not destroy");
#endif

                    context.AddObjectToAsset(o.name, o);
                    if (o is GameObject)
                    {
                        // Root GameObject is main object
                        context.SetMainObject(loaded.Root);
                    }

                    return true;
                });
            }
        }

        public static IEnumerable<(string, UnityEngine.Object)> EnumerateTexturesFromUri(Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> exclude,
            GltfParser parser, UnityPath dir)
        {
            foreach (var texParam in GltfTextureEnumerator.Enumerate(parser.GLTF))
            {
                switch (texParam.TextureType)
                {
                    case GetTextureParam.METALLIC_GLOSS_PROP:
                    case GetTextureParam.OCCLUSION_PROP:
                        break;

                    default:
                        {
                            var gltfTexture = parser.GLTF.textures.First(y => y.name == texParam.GltflName);
                            var gltfImage = parser.GLTF.images[gltfTexture.source];
                            if (!string.IsNullOrEmpty(gltfImage.uri))
                            {
                                var child = dir.Child(gltfImage.uri);
                                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(child.Value);
                                if (asset == null)
                                {
                                    throw new System.IO.FileNotFoundException($"{child}");
                                }
                                // Debug.Log($"exists: {child}: {asset}");
                                if (exclude == null || !exclude.Any(kv => kv.Value.name == asset.name))
                                {
                                    yield return (asset.name, asset);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
