using System;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".vrm")
                {
                    var context = new VRMImporterContext(path);
                    try
                    {
                        VRMImporter.LoadFromPath(context);

                        /*
                        var prefabPath = String.Format("{0}/{1}.prefab",
                            Path.GetDirectoryName(path),
                            Path.GetFileNameWithoutExtension(path));

                        VRMAssetWriter.SaveAsPrefab(context.Root, prefabPath);

                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath.ToUnityRelativePath());
                        Selection.activeObject = prefab;
                        */

                        context.SaveAsAsset();
                        context.Destroy(false);
                    }
                    catch(Exception ex)
                    {
                        Debug.LogError(ex);
                        if (context != null)
                        {
                            context.Destroy(true);
                        }
                    }
                }
            }
        }
    }
}
