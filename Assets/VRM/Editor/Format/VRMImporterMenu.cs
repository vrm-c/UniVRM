using System.IO;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System;
using System.Collections.Generic;

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
            else
            {
                if (path.StartsWithUnityAssetPath())
                {
                    Debug.LogWarningFormat("disallow import from folder under the Assets");
                    return;
                }

                var assetPath = EditorUtility.SaveFilePanel("save prefab", "Assets", Path.GetFileNameWithoutExtension(path), "prefab");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                if (!assetPath.StartsWithUnityAssetPath())
                {
                    Debug.LogWarningFormat("out of asset path: {0}", assetPath);
                    return;
                }

                // import as asset
                var prefabPath = UnityPath.FromUnityPath(assetPath);
                var parser = new GltfParser();
                parser.ParseGlb(File.ReadAllBytes(path));

                Action<IEnumerable<string>> onCompleted = _ =>
                {
                    //
                    // after textures imported
                    //
                    using (var context = new VRMImporterContext(parser))
                    {
                        var editor = new VRMEditorImporterContext(context, prefabPath);
                        context.Load();
                        editor.SaveAsAsset();
                    }
                };

                using (var context = new VRMImporterContext(parser))
                {
                    var editor = new VRMEditorImporterContext(context, prefabPath);
                    editor.ConvertAndExtractImages(UnityPath.FromFullpath(path), onCompleted);
                }
            }
        }
    }
}
