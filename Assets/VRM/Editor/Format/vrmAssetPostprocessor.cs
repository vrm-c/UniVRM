using System;
using System.IO;
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

            var parser = new GltfParser();
            parser.ParseGlb(File.ReadAllBytes(path.FullPath));

            var prefabPath = path.Parent.Child(path.FileNameWithoutExtension + ".prefab");

            Action onCompleted = () =>
            {
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
                editor.ConvertAndExtractImages(path, onCompleted);
            }
        }
    }
#endif
}
