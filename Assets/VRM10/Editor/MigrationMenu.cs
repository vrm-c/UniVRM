using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public static class MigrationMenu
    {
        static string s_lastPath = Application.dataPath;

        const string CONTEXT_MENU = "Assets/Migration: Vrm1";

        [MenuItem(CONTEXT_MENU, true)]
        static bool Enable()
        {
            var path = UniGLTF.UnityPath.FromAsset(Selection.activeObject);
            var isVrm = path.Extension.ToLower() == ".vrm";
            return isVrm;
        }

        [MenuItem(CONTEXT_MENU, false)]
        static void Exec()
        {
            var path = UniGLTF.UnityPath.FromAsset(Selection.activeObject);
            var isVrm = path.Extension.ToLower() == ".vrm";

            var vrm1Bytes = MigrationVrm.Migrate(File.ReadAllBytes(path.FullPath));

            var dst = EditorUtility.SaveFilePanel(
                "Save vrm1 file",
                s_lastPath,
                $"{path.FileNameWithoutExtension}_vrm1",
                "vrm");
            if (string.IsNullOrEmpty(dst))
            {
                return;
            }
            s_lastPath = Path.GetDirectoryName(dst);

            // write result
            File.WriteAllBytes(dst, vrm1Bytes);

            // immediately import for GUI update
            UniGLTF.UnityPath.FromFullpath(dst).ImportAsset();
        }
    }
}
