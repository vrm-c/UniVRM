using System;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace UniHumanoid
{
    public class bvhAssetPostprocessor : AssetPostprocessor
    {
        static bool IsStreamingAsset(string path)
        {
            var baseFullPath = Path.GetFullPath(Application.dataPath + "/..").Replace("\\", "/");
            path = Path.Combine(baseFullPath, path).Replace("\\", "/");
            return path.StartsWith(Application.streamingAssetsPath + "/");
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (IsStreamingAsset(path))
                {
                    UniGLTFLogger.Log($"Skip StreamingAssets: {path}");
                    continue;
                }

                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".bvh")
                {
                    UniGLTFLogger.Log($"ImportBvh: {path}");
                    var context = new BvhImporterContext();
                    try
                    {
                        context.Parse(path);
                        context.Load();
                        context.SaveAsAsset();
                        context.Destroy(false);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        context.Destroy(true);
                    }
                }
            }
        }
    }
}
