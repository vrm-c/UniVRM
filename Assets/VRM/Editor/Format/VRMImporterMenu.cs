using System.IO;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VRM
{
    public static class VRMImporterMenu
    {
        [MenuItem(VRMVersion.MENU + "/Import", priority = 1)]
        static void ImportMenu()
        {
            var path = EditorUtility.OpenFilePanel("open vrm", "", "vrm");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Application.isPlaying)
            {
                ImportRuntime(path);
                return;
            }

            if (path.StartsWithUnityAssetPath())
            {
                Debug.LogWarningFormat("disallow import from folder under the Assets");
                return;
            }

            var prefabPath = EditorUtility.SaveFilePanel("save prefab", "Assets", Path.GetFileNameWithoutExtension(path), "prefab");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ImportAsset(path, UnityPath.FromFullpath(prefabPath));
        }

        static void ImportRuntime(string path)
        {
            // load into scene
            var parser = new GltfParser();
            parser.ParsePath(path);

            using (var context = new VRMImporterContext(parser))
            {
                context.Load();
                context.EnableUpdateWhenOffscreen();
                context.ShowMeshes();
                context.DisposeOnGameObjectDestroyed();
                Selection.activeGameObject = context.Root;
            }
        }

        static void ImportAsset(string path, UnityPath prefabPath)
        {
            if (!prefabPath.IsUnderAssetsFolder)
            {
                Debug.LogWarningFormat("out of asset path: {0}", prefabPath);
                return;
            }

            // import as asset
            // var prefabPath = UnityPath.FromUnityPath(prefabPath);
            var parser = new GltfParser();
            parser.ParseGlb(File.ReadAllBytes(path));

            Action<IEnumerable<UnityPath>> onCompleted = texturePaths =>
            {
                //
                // after textures imported
                //
                var map = texturePaths.Select(x =>
                {
                    var texture = x.LoadAsset<Texture2D>() as UnityEngine.Object;
                    return (texture.name, texture);
                }).ToArray();

                using (var context = new VRMImporterContext(parser, map))
                {
                    var editor = new VRMEditorImporterContext(context, prefabPath);
                    foreach (var textureInfo in new VRMMtoonMaterialImporter(context.VRM).EnumerateAllTexturesDistinct(parser))
                    {
                        TextureImporterConfigurator.Configure(textureInfo, map.ToDictionary(x => x.name, x => x.texture as Texture2D));
                    }
                    context.Load();
                    editor.SaveAsAsset();
                }
            };

            using (var context = new VRMImporterContext(parser))
            {
                var editor = new VRMEditorImporterContext(context, prefabPath);
                editor.ConvertAndExtractImages(onCompleted);
            }
        }
    }
}
