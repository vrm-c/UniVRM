using System.IO;
using UnityEditor;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    public static class VRMImporterMenu
    {
        [MenuItem(VRMVersion.VRM_VERSION + "/Import", priority =1)]
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
                Selection.activeGameObject = VRMImporter.LoadFromPath(path);
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
                Import(path, UnityPath.FromUnityPath(assetPath));
            }
        }

        static void Import(string readPath, UnityPath prefabPath)
        {
            //var bytes = File.ReadAllBytes(readPath);
            var context = new VRMImporterContext(UnityPath.FromFullpath(readPath));
            context.ParseGlb(File.ReadAllBytes(readPath));
            context.SaveTexturesAsPng(prefabPath);

            EditorApplication.delayCall += () =>
            {
                // delay and can import png texture
                VRMImporter.LoadFromBytes(context);
                context.SaveAsAsset(prefabPath);
                context.Destroy(false);
            };
        }
    }
}
