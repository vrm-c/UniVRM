using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public static class LookAtEditor
    {
        public static void Draw2D(VRM10Controller target)
        {

        }

        public static void Draw3D(VRM10Controller target, Transform head)
        {
            if (target == null)
            {
                return;
            }
            if (head == null)
            {
                return;
            }

            OnSceneGUIOffset(target, head);
            if (!Application.isPlaying)
            {
                // offset
                var p = target.Vrm.LookAt.OffsetFromHead;
                Handles.Label(head.position, $"fromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]");
            }
            else
            {
                target.Vrm.LookAt.OnSceneGUILookAt(head);
            }
        }

        static void OnSceneGUIOffset(VRM10Controller m_target, Transform head)
        {
            if (!m_target.Vrm.LookAt.DrawGizmo)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var worldOffset = head.localToWorldMatrix.MultiplyPoint(m_target.Vrm.LookAt.OffsetFromHead);
            worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

            Handles.DrawDottedLine(head.position, worldOffset, 5);
            Handles.SphereHandleCap(0, head.position, Quaternion.identity, 0.02f, Event.current.type);
            Handles.SphereHandleCap(0, worldOffset, Quaternion.identity, 0.02f, Event.current.type);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_target, "Changed FirstPerson");

                m_target.Vrm.LookAt.OffsetFromHead = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
            }
        }
    }
}
