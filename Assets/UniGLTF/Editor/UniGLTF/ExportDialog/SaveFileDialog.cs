using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class SaveFileDialog
    {
        static string m_lastExportDir;

        static string extensionString(string[] extensions)
        {
#if UNITY_EDITOR_OSX
            // in OSX multi extension cause exception.
            // https://github.com/vrm-c/UniVRM/issues/1837
            return extensions.Length > 0 ? extensions[0] : "";
#else
            return string.Join(",", extensions);
#endif
        }

        public static string GetPath(string title, string name, params string[] extensions)
        {
            string directory = m_lastExportDir;
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetParent(Application.dataPath).ToString();
            }

            var path = EditorUtility.SaveFilePanel(title, directory, name, extensionString(extensions));
            if (!string.IsNullOrEmpty(path))
            {
                m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            }
            return path;
        }

        public static string GetDir(string title, string dir = null)
        {
            string directory = string.IsNullOrEmpty(dir) ? m_lastExportDir : dir;
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetParent(Application.dataPath).ToString();
            }

            var path = EditorUtility.SaveFolderPanel(title, directory, null);
            if (!string.IsNullOrEmpty(path))
            {
                m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            }

            return path;
        }
    }
}
