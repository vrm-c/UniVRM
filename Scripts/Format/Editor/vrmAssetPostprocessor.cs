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
                    ImportVrm(path);
                }
            }
        }

        static void ImportVrm(string path)
        {
            var context = new VRMImporterContext(path);
            context.ParseGlb(File.ReadAllBytes(path));

            var prefabPath = (Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".prefab").Replace("\\", "/");
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
