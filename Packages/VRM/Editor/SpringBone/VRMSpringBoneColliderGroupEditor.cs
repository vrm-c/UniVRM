using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMSpringBoneColliderGroup))]
    public class VRMSpringBoneColliderGroupEditor : Editor
    {
        VRMSpringBoneColliderGroup m_target;

        private void OnEnable()
        {
            m_target = (VRMSpringBoneColliderGroup)target;
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

        override public void OnInspectorGUI()
        {
            // show validate information
            foreach (var v in m_target.Validate())
            {
                v.DrawGUI();
            }

            base.OnInspectorGUI();
        }

        [MenuItem("CONTEXT/VRMSpringBoneColliderGroup/X Mirror")]
        private static void InvertOffsetX(MenuCommand command)
        {
            var target = command.context as VRMSpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "X Mirror");

            foreach (var sphereCollider in target.Colliders)
            {
                var offset = sphereCollider.Offset;
                offset.x *= -1f;
                sphereCollider.Offset = offset;
            }
        }

        [MenuItem("CONTEXT/VRMSpringBoneColliderGroup/Sort Colliders by Radius")]
        private static void SortByRadius(MenuCommand command)
        {
            var target = command.context as VRMSpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders by Radius");

            target.Colliders = target.Colliders.OrderBy(x => -x.Radius).ToArray();
        }

        [MenuItem("CONTEXT/VRMSpringBoneColliderGroup/Sort Colliders Reversed")]
        private static void SortReverse(MenuCommand command)
        {
            var target = command.context as VRMSpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders Reversed");

            target.Colliders = target.Colliders.Reverse().ToArray();
        }

        [MenuItem("CONTEXT/VRMSpringBoneColliderGroup/Sort Colliders by Offset X")]
        private static void SortByOffsetX(MenuCommand command)
        {
            var target = command.context as VRMSpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders by Offset X");

            target.Colliders = target.Colliders.OrderBy(x => x.Offset.x).ToArray();
        }

        [MenuItem("CONTEXT/VRMSpringBoneColliderGroup/Sort Colliders by Offset Y")]
        private static void SortByOffsetY(MenuCommand command)
        {
            var target = command.context as VRMSpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders by Offset Y");

            target.Colliders = target.Colliders.OrderBy(x => x.Offset.y).ToArray();
        }

        [MenuItem("CONTEXT/VRMSpringBoneColliderGroup/Sort Colliders by Offset Z")]
        private static void SortByOffsetZ(MenuCommand command)
        {
            var target = command.context as VRMSpringBoneColliderGroup;
            if (target == null) return;

            Undo.RecordObject(target, "Sort Colliders by Offset Z");

            target.Colliders = target.Colliders.OrderBy(x => x.Offset.z).ToArray();
        }
    }
}
