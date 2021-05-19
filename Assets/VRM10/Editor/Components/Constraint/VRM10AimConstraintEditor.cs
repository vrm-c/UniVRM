using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10AimConstraint))]
    public class VRM10AimConstraintEditor : Editor
    {
        VRM10AimConstraint m_target;

        void OnEnable()
        {
            m_target = (VRM10AimConstraint)target;
        }

        static GUIStyle s_style;
        static GUIStyle Style
        {
            get
            {
                if (s_style == null)
                {
                    s_style = new GUIStyle("box");
                }
                return s_style;
            }
        }

        static void DrawAimUp(Quaternion rot, Vector3 pos, Color c)
        {
            Handles.matrix = Matrix4x4.identity;
            Handles.color = c;
            // aim
            var aim = pos + rot * Vector3.forward * 0.3f;
            Handles.DrawLine(pos, aim);
            Handles.Label(aim, "aim");
            // up
            var up = pos + rot * Vector3.up * 0.3f;
            Handles.DrawLine(pos, up);
            Handles.Label(up, "up");
        }

        public void OnSceneGUI()
        {
            if (m_target.Source == null)
            {
                return;
            }

            // this to target line
            Handles.color = Color.yellow;
            Handles.DrawLine(m_target.Source.position, m_target.transform.position);

            var pos = m_target.transform.position;

            var pr = TR.FromParent(m_target.transform);

            if (m_target.m_src == null)
            {
                EditorGUI.BeginChangeCheck();
                TR.FromWorld(m_target.transform).Draw(0.2f);

                Handles.matrix = Matrix4x4.identity;
                var rot = Handles.RotationHandle(pr.Rotation * m_target.DestinationOffset, pos);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target, "Rotated RotateAt Point");
                    m_target.DestinationOffset = Quaternion.Inverse(pr.Rotation) * rot;
                }

                DrawAimUp(rot, pos, Color.yellow);
            }
            else
            {
                var init = pr.Rotation * m_target.m_src.LocalInitial.Rotation;
                DrawAimUp(init * m_target.DestinationOffset, m_target.transform.position, Color.yellow);
                new TR(init, m_target.transform.position).Draw(0.2f);
                DrawAimUp(init * m_target.DestinationOffset * m_target.Delta, m_target.transform.position, Color.magenta);
            }

            // Target UPVector
            Handles.color = Color.green;
            Handles.DrawLine(m_target.transform.position, m_target.transform.position + m_target.UpVector);
        }
    }
}
