using System;
using System.IO;
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
                    Debug.LogFormat("Skip StreamingAssets: {0}", path);
                    continue;
                }

                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".bvh")
                {
                    Debug.LogFormat("ImportBvh: {0}", path);
                    var context = new BvhImporterContext();
                    try
                    {
                        context.Parse(path);
                        context.Load();
                        context.SaveAsAsset();
                        context.Destroy(false);
                    }
                    catch(Exception ex)
                    {
                        Debug.LogError(ex);
                        context.Destroy(true);
                    }
                }
            }
        }
    }
}
