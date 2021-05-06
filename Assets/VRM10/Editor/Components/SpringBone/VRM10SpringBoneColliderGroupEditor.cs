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
                foreach (var y in x.Shapes)
                {
                    var offset = Handles.PositionHandle(y.Offset, Quaternion.identity);
                    if (offset != y.Offset)
                    {
                        changed = true;
                        y.Offset = offset;
                    }
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

            foreach (var collider in target.Colliders)
            {
                foreach (var shape in collider.Shapes)
                {
                    var offset = shape.Offset;
                    offset.x *= -1f;
                    shape.Offset = offset;
                }
            }
        }
    }
}
