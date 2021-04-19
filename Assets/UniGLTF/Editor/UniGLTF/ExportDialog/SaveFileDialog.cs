using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class SaveFileDialog
    {
        static string m_lastExportDir;
        public static string GetPath(string title, string name, params string[] extensions)
        {
            string directory = m_lastExportDir;
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetParent(Application.dataPath).ToString();
            }

            var path = EditorUtility.SaveFilePanel(title, directory, name, string.Join(",", extensions));
            if (!string.IsNullOrEmpty(path))
            {
                m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            }
            return path;
        }
    }
}
