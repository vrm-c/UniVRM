using System.IO;
using UnityEditor;
using UnityEngine;

namespace MeshUtility
{
    public static class EditorChangeTextureType
    {
        [MenuItem("Assets/SaveAsPng", true)]
        static bool IsTextureAsset()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/SaveAsPng")]
        static void SaveAsPng()
        {
            var texture = Selection.activeObject as Texture2D;
            var path = SaveDialog(texture.name);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Debug.Log($"save: ${path}");
        }

        private static string m_lastExportDir;
        static string SaveDialog(string name)
        {
            string directory;
            if (string.IsNullOrEmpty(m_lastExportDir))
                directory = Directory.GetParent(Application.dataPath).ToString();
            else
                directory = m_lastExportDir;

            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save png",
                    directory,
                    $"{name}.png",
                    "png");
            if (!string.IsNullOrEmpty(path))
            {
                m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            }
            return path;
        }
    }
}
