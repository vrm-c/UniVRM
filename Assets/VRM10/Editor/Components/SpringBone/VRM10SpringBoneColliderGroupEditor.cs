using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneColliderGroup))]
    public class VRM10SpringBoneColliderGroupEditor : Editor
    {
        VRM10SpringBoneColliderGroup m_target;

        private void OnEnable()
        {
            m_target = (VRM10SpringBoneColliderGroup)target;
        }

        private void OnSceneGUI()
        {
            Undo.RecordObject(m_target, "VRMSpringBoneColliderGroupEditor");

            Handles.matrix = m_target.transform.localToWorldMatrix;
            Gizmos.color = Color.green;

            bool changed = false;

            foreach (var x in m_target.Colliders)
            {
                var offset = Handles.PositionHandle(x.Offset, Quaternion.identity);
                if (offset != x.Offset)
                {
                    changed = true;
                    x.Offset = offset;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(m_target);
            }
        }

        [MenuItem("CONTEXT/VRM10SpringBoneColliderGroup/X Mirror")]
        private static void InvertOffsetX(MenuCommand command)
        {
            var target = command.context as VRM10SpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "X Mirror");

            foreach (var sphereCollider in target.Colliders)
            {
                var offset = sphereCollider.Offset;
                offset.x *= -1f;
                sphereCollider.Offset = offset;
            }
        }

        [MenuItem("CONTEXT/VRM10SpringBoneColliderGroup/Sort Colliders by Radius")]
        private static void SortByRadius(MenuCommand command)
        {
            var target = command.context as VRM10SpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders by Radius");

            target.Colliders = target.Colliders.OrderBy(x => -x.Radius).ToList();
        }

        [MenuItem("CONTEXT/VRM10SpringBoneColliderGroup/Sort Colliders by Offset Y")]
        private static void SortByOffsetY(MenuCommand command)
        {
            var target = command.context as VRM10SpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders by Offset Y");

            target.Colliders = target.Colliders.OrderBy(x => -x.Offset.y).ToList();
        }
    }
}
