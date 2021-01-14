using System.IO;
using UnityEditor;
using UnityEngine;

namespace MeshUtility
{
    public static class EditorChangeTextureType
    {
        [MenuItem("Assets/SaveAsPng", true)]
        [MenuItem("Assets/SaveAsPngLinear", true)]
        static bool IsTextureAsset()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/SaveAsPng")]
        static void SaveAsPng()
        {
            SaveAsPng(true);
        }

        [MenuItem("Assets/SaveAsPngLinear")]
        static void SaveAsPngLinear()
        {
            SaveAsPng(false);
        }

        static void SaveAsPng(bool sRGB)
        {
            var texture = Selection.activeObject as Texture2D;
            var path = SaveDialog(AssetsPath.FromAsset(texture));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
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
