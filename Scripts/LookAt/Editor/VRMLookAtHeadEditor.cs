using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMLookAtHead))]
    public class VRMLookAtHeadEditor : Editor
    {
        VRMLookAtHead m_target;

        void OnEnable()
        {
            m_target = (VRMLookAtHead)target;
        }

        const float RADIUS = 0.5f;

        void OnSceneGUI()
        {
            if (!Application.isPlaying) return;
            if (!m_target.DrawGizmo) return;
            if (m_target.Target == null) return;
            if (m_target.Head.Transform == null) return;

            {
                EditorGUI.BeginChangeCheck();
                var newTargetPosition = Handles.PositionHandle(m_target.Target.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target.Target, "Change Look At Target Position");
                    m_target.Target.position = newTargetPosition;
                }
            }

            Handles.color = new Color(1, 1, 1, 0.6f);
            Handles.DrawDottedLine(m_target.Head.Transform.position, m_target.Target.position, 4.0f);

            Handles.matrix = m_target.Head.InitialWorldMatrix;
            Handles.Label(Vector3.zero, string.Format("Yaw: {0:0.}degree\nPitch: {1:0.}degree",
                m_target.Yaw,
                m_target.Pitch));

            Handles.color = new Color(0, 1, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    m_target.Head.OffsetRotation.GetColumn(1),
                    m_target.Head.OffsetRotation.GetColumn(2),
                    m_target.Yaw,
                    RADIUS);

            Handles.matrix = m_target.Head.InitialWorldMatrix * m_target.YawMatrix;
            Handles.color = new Color(1, 0, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    m_target.Head.OffsetRotation.GetColumn(0),
                    m_target.Head.OffsetRotation.GetColumn(2),
                    -m_target.Pitch,
                    RADIUS);
        }
    }
}
