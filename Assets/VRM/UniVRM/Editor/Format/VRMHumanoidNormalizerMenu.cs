using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    public static class VRMHumanoidNormalizerMenu
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

            // BoneNormalizer.Execute はコピーを正規化する。UNDO無用
            Selection.activeGameObject = BoneNormalizer.Execute(go, true, false);
        }
    }
}
