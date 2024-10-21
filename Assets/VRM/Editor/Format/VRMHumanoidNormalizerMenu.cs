using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    public static class VRMHumanoidNormalizerMenu
    {
        public const string MENU_NAME = "VRM 0.x Freeze T-Pose";
        public static bool NormalizeValidation()
        {
            var root = Selection.activeObject as GameObject;
            if (root == null)
            {
                return false;
            }

            if (root.TryGetComponent<Animator>(out var animator))
            {
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
            else
            {
                return false;
            }
        }

        public static void Normalize(bool bakeCurrentBlendShape)
        {
            var go = Selection.activeObject as GameObject;

            VRMBoneNormalizer.Execute(go, true, bakeCurrentBlendShape);
        }
    }
}
