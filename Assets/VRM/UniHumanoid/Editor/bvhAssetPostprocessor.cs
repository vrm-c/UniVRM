using System;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace UniHumanoid
{
    public class bvhAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
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
