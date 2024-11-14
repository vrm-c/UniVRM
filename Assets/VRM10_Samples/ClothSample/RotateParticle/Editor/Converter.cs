using UnityEditor;
using UnityEngine;
using UniVRM10;


namespace RotateParticle.Components
{
    public static class Converter
    {
        const string FROM_VRM10_MENU = "GameObject/CreateRotateParticleFromVrm10";

        [MenuItem(FROM_VRM10_MENU, true)]
        public static bool IsFromVrm10()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                return false;
            }
            return go.GetComponent<Vrm10Instance>() != null;
        }

        [MenuItem(FROM_VRM10_MENU, false)]
        public static void FromVrm10()
        {
            var go = Selection.activeGameObject;
            var instance = go.GetComponent<Vrm10Instance>();

            foreach (var spring in instance.SpringBone.Springs)
            {
                if (spring.Joints == null || spring.Joints[0] == null)
                {
                    continue;
                }

                var root = spring.Joints[0].gameObject;
                if (root == null)
                {
                    continue;
                }

                if (root.GetComponent<Warp>() != null)
                {
                    continue;
                }

                UnityEditor.Undo.IncrementCurrentGroup();
                UnityEditor.Undo.SetCurrentGroupName("VRM10SpringBoneColliderGroup.Separate");
            }
        }
    }
}