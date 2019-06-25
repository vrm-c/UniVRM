using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
#if !VRM_STOP_ASSETPOSTPROCESSOR
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (UnityPath.FromUnityPath(path).IsStreamingAsset)
                {
                    Debug.LogFormat("Skip StreamingAssets: {0}", path);
                    continue;
                }

                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".vrm")
                {
                    ImportVrm(UnityPath.FromUnityPath(path));
                }
            }
        }

        static void ImportVrm(UnityPath path)
        {
            if (!path.IsUnderAssetsFolder)
            {
                throw new Exception();
            }
            var context = new VRMImporterContext();
            context.ParseGlb(File.ReadAllBytes(path.FullPath));

            var prefabPath = path.Parent.Child(path.FileNameWithoutExtension + ".prefab");

            // save texture assets !
            context.ExtractImages(prefabPath);

            EditorApplication.delayCall += () =>
            {
                //
                // after textures imported
                //
                context.Load();
                context.SaveAsAsset(prefabPath);
                context.EditorDestroyRoot();
            };
        }
    }
#endif
}
