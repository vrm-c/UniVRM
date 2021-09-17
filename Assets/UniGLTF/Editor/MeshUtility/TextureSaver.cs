using System.IO;
using UnityEditor;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF.MeshUtility
{
    public static class EditorChangeTextureType
    {
        public static void SaveAsPng(bool sRGB)
        {
            var texture = Selection.activeObject as Texture2D;
            var path = SaveDialog(AssetsPath.FromAsset(texture));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var (tex, mime) = new EditorTextureSerializer().ExportBytesWithMime(texture, sRGB ? ColorSpace.sRGB : ColorSpace.Linear);

            File.WriteAllBytes(path, tex);
            Debug.Log($"save: {path}");

            var assetsPath = AssetsPath.FromFullpath(path);

            EditorApplication.delayCall += () =>
            {
                assetsPath.ImportAsset();
                var importer = assetsPath.GetImporter<TextureImporter>();
                if (importer == null)
                {
                    Debug.LogWarningFormat("fail to get TextureImporter: {0}", assetsPath);
                }
                importer.sRGBTexture = sRGB;
                importer.SaveAndReimport();
                Debug.Log($"sRGB: {sRGB}");
            };
        }

        private static string m_lastExportDir;
        static string SaveDialog(AssetsPath assetsPath)
        {
            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save png",
                    assetsPath.Parent.FullPath,
                    $"{assetsPath.FileNameWithoutExtension}.png",
                    "png");
            if (!string.IsNullOrEmpty(path))
            {
                m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            }
            return path;
        }
    }
}
