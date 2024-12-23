using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class UniGltfEditorDialog
    {
        public const string IMPORT_MENU_NAME = "Import glTF... (*.gltf|*.glb|*.zip)";

        static string extensionString(string[] extensions)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return string.Join(",", extensions);
            }
            else
            {
                // in OSX multi extension cause exception.
                // https://github.com/vrm-c/UniVRM/issues/1837
                // in Linux
                // https://github.com/vrm-c/UniVRM/issues/2515
                return extensions.Length > 0 ? extensions[0] : "";
            }
        }

        public static bool TryOpenFilePanel(string directory, out string path)
        {
            path = EditorUtility.OpenFilePanel(IMPORT_MENU_NAME, directory,
                // https://github.com/vrm-c/UniVRM/issues/1837
                Application.platform == RuntimePlatform.WindowsEditor ? "gltf,glb,zip" : "glb"
            );
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return true;
        }

        static string s_lastExportDir;
        public static bool TrySaveFilePanel(string title, string name, string[] extensions, out string path)
        {
            string directory = s_lastExportDir;
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetParent(Application.dataPath).ToString();
            }

            path = EditorUtility.SaveFilePanel(title, directory, name, extensionString(extensions));
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            s_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            return true;
        }

        public static bool TryGetDir(string title, string dir, out string path)
        {
            string directory = string.IsNullOrEmpty(dir) ? s_lastExportDir : dir;
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetParent(Application.dataPath).ToString();
            }

            path = EditorUtility.SaveFolderPanel(title, directory, null);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            s_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");
            return true;
        }
    }
}