using System.Collections.Generic;
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

        Dictionary<VRM10SpringBoneCollider.Shape, Vector3> m_moved = new Dictionary<VRM10SpringBoneCollider.Shape, Vector3>();
        VRM10SpringBoneCollider m_recordObject;

        private void OnSceneGUI()
        {
            Gizmos.color = Color.green;

            m_moved.Clear();
            m_recordObject = null;
            EditorGUI.BeginChangeCheck();
            foreach (var x in m_target.Colliders)
            {
                Handles.matrix = x.transform.localToWorldMatrix;
                foreach (var y in x.Shapes)
                {
                    var offset = Handles.PositionHandle(y.Offset, Quaternion.identity);
                    if (offset != y.Offset)
                    {
                        m_recordObject = x;
                        m_moved.Add(y, offset);
                    }
                    DrawShape(x.transform, y);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_recordObject, "VRMSpringBoneColliderGroupEditor");
                foreach (var kv in m_moved)
                {
                    kv.Key.Offset = kv.Value;
                }
            }
        }

        public void DrawShape(Transform transform, VRM10SpringBoneCollider.Shape shape)
        {
            Gizmos.color = Color.cyan;
            Matrix4x4 mat = transform.localToWorldMatrix;
            Gizmos.matrix = mat * Matrix4x4.Scale(new Vector3(
                1.0f / transform.lossyScale.x,
                1.0f / transform.lossyScale.y,
                1.0f / transform.lossyScale.z
                ));

            {
                switch (shape.ShapeType)
                {
                    case VRM10SpringBoneColliderShapeTypes.Sphere:
                        Handles.SphereHandleCap(0, shape.Offset, Quaternion.identity, shape.Radius, EventType.Repaint);
                        break;

                    case VRM10SpringBoneColliderShapeTypes.Capsule:
                        // VRM10SpringBoneCollider.DrawWireCapsule(shape.Offset, shape.Tail, shape.Radius);
                        break;
                }
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
