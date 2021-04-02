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
            var externalObjectMap = scriptedImporter.GetExternalObjectMap().Select(kv => (kv.Value.name, kv.Value)).ToArray();

            var externalTextures = EnumerateTexturesFromUri(externalObjectMap, parser, UnityPath.FromUnityPath(scriptedImporter.assetPath).Parent).ToArray();

            using (var loader = new ImporterContext(parser, externalObjectMap.Concat(externalTextures)))
            {
                // settings TextureImporters
                foreach (var textureInfo in GltfTextureEnumerator.EnumerateAllTexturesDistinct(parser))
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalMap);
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

        public static IEnumerable<(string, UnityEngine.Object)> EnumerateTexturesFromUri(IEnumerable<(string, UnityEngine.Object)> exclude,
            GltfParser parser, UnityPath dir)
        {
            var used = new HashSet<Texture2D>();
            foreach (var texParam in GltfTextureEnumerator.EnumerateAllTexturesDistinct(parser))
            {
                switch (texParam.TextureType)
                {
                    case TextureImportTypes.StandardMap:
                        break;

                    default:
                        {
                            if (!string.IsNullOrEmpty(texParam.Uri) && !texParam.Uri.StartsWith("data:"))
                            {
                                var child = dir.Child(texParam.Uri);
                                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(child.Value);
                                if (asset == null)
                                {
                                    throw new System.IO.FileNotFoundException($"{child}");
                                }

                                if (exclude != null && exclude.Any(kv => kv.Item2.name == asset.name))
                                {
                                    // exclude. skip
                                }
                                else
                                {
                                    if (used.Add(asset))
                                    {
                                        yield return (asset.name, asset);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
