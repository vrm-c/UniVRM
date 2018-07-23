using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;


namespace VRM
{
#if !VRM_STOP_ASSETPOSTPROCESSOR
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
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
            var context = new VRMImporterContext(path);
            context.ParseGlb(File.ReadAllBytes(path.FullPath));

            var prefabPath = path.Parent.Child(path.FileNameWithoutExtension + ".prefab");
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
#endif
}
