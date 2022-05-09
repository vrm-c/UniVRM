using System.IO;
using UnityEditor;
using UnityEngine;
using UniGLTF;

namespace VRM
{
    public static class VRMImporterMenu
    {
        public static void OpenImportMenu()
        {
            var path = EditorUtility.OpenFilePanel("open vrm", "", "vrm");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Application.isPlaying)
            {
                // import vrm to scene without asset creation
                ImportRuntime(path);
            }
            else
            {
                // import vrm to asset
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

                vrmAssetPostprocessor.ImportVrmAndCreatePrefab(path, UnityPath.FromFullpath(prefabPath));
            }
        }

        /// <summary>
        /// load into scene
        /// </summary>
        /// <param name="path">vrm path</param>
        static void ImportRuntime(string path)
        {
            using (var data = new GlbFileParser(path).Parse())
            using (var context = new VRMImporterContext(new VRMData(data)))
            {
                var loaded = context.Load();
                loaded.EnableUpdateWhenOffscreen();
                loaded.ShowMeshes();
                Selection.activeGameObject = loaded.gameObject;
            }
        }
    }
}
