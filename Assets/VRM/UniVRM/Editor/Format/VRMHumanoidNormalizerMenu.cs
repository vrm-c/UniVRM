using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    public static class VRMHumanoidNorimalizerMenu
    {
        const string MENU_KEY = VRMVersion.MENU + "/Freeze T-Pose";
        [MenuItem(MENU_KEY, true, 1)]
        private static bool ExportValidate()
        {
            var root = Selection.activeObject as GameObject;
            if (root == null)
            {
                return false;
            }

            var animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                return false;
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                return false;
            }

            if (!avatar.isValid)
            {
                return false;
            }

            if (!avatar.isHuman)
            {
                return false;
            }

            return true;
        }

        [MenuItem(MENU_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;

            GameObject normalizedRoot = null;
            using (new VRMExportSettings.RecordDisposer(go.transform.Traverse().ToArray(), "before normalize"))
            {
                var normalized = BoneNormalizer.Execute(go, true, false);
                VRMExportSettings.CopyVRMComponents(go, normalized.Root, normalized.BoneMap);
                normalizedRoot = normalized.Root;
            }
            Selection.activeGameObject = normalizedRoot;
        }
    }
}
