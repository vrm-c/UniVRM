#if USE_VRMIMPORTER_MENU
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public static class VRMImporterMenu
    {
        [MenuItem("VRM/import from external file")]
        static void ImportMenu()
        {
            var path = EditorUtility.OpenFilePanel("open vrm", "", "vrm");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            if (ext != ".vrm")
            {
                return;
            }

            if (path.StartsWithUnityAssetPath())
            {
                Debug.LogWarning("vrm in AssetFolder is imported automatically");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace(".vrm", ".prefab").ToUnityRelativePath());
                return;
            }

            var root = VRMImporter.LoadFromPath(path);

            if (!EditorApplication.isPlaying)
            {
                // save root as Asset
                var prefabPath = EditorUtility.SaveFilePanel("save vrm prefab",
                    "Assets",
                    Path.GetFileNameWithoutExtension(path) + ".prefab",
                    "prefab"
                    );

                if (!string.IsNullOrEmpty(prefabPath))
                {
                    VRMAssetWriter.SaveAsPrefab(root, prefabPath);

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath.ToUnityRelativePath());
                    Selection.activeObject = prefab;
                }
            }
        }
    }
}
#endif