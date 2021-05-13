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

        public void OnSceneGUI()
        {
            if (m_target.Source == null)
            {
                return;
            }

            // this to target line
            Handles.color = Color.yellow;
            Handles.DrawLine(m_target.Source.position, m_target.transform.position);

            TR.FromWorld(m_target.transform).Draw(0.2f);

            Handles.matrix = Matrix4x4.identity;
            EditorGUI.BeginChangeCheck();
            var pr = m_target.ParentRotation;
            var pos = m_target.transform.position;
            var rot = Handles.RotationHandle(pr * m_target.DestinationOffset, pos);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_target, "Rotated RotateAt Point");
                m_target.DestinationOffset = Quaternion.Inverse(pr) * rot;
            }

            // aim
            Handles.color = Color.yellow;
            var aim = pos + rot * Vector3.forward * 0.3f;
            Handles.DrawLine(pos, aim);
            Handles.Label(aim, "aim");
            // up
            var up = pos + rot * Vector3.up * 0.3f;
            Handles.DrawLine(pos, up);
            Handles.Label(up, "up");

            // var delta = Clamp180(m_target.Delta.eulerAngles);

            // // show source
            // {
            //     var sb = new StringBuilder();
            //     sb.AppendLine();
            //     sb.AppendLine();
            //     sb.AppendLine($"source: {m_target.SourceCoordinate}");
            //     sb.AppendLine($"{delta.x:0.}");
            //     sb.AppendLine($"{delta.y:0.}");
            //     sb.Append($"{delta.z:0.}");
            //     Handles.Label(m_target.Source.position, sb.ToString(), Style);
            // }

            // // show dst
            // {
            //     var sb = new StringBuilder();
            //     sb.AppendLine($"constraint: {m_target.DestinationCoordinate}");
            //     sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.X) ? $"freeze" : $"{delta.x:0.}");
            //     sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.Y) ? $"freeze" : $"{delta.y:0.}");
            //     sb.Append(m_target.FreezeAxes.HasFlag(AxisMask.Z) ? $"freeze" : $"{delta.z:0.}");
            //     Handles.Label(m_target.transform.position, sb.ToString(), Style);
            // }
        }
        // void OnDrawGizmos()
        // {
        //     if (Source == null)
        //     {
        //         return;
        //     }

        //     var localPosition = transform.worldToLocalMatrix.MultiplyPoint(Source.position);
        //     var (yaw, yaw_, pitch, pitch_) = CalcYawPitch(m_coords, localPosition);
        //     switch (yaw_)
        //     {
        //         case 0: break;
        //         case 1: yaw = 180 - yaw; break;
        //         case 2: yaw = 180 + yaw; break;
        //         case 3: yaw = 360 - yaw; break;
        //     }
        //     switch (pitch_)
        //     {
        //         case 0: pitch = -pitch; break;
        //         case 1: pitch = -pitch; break;
        //         case 2: break;
        //         case 3: break;
        //     }
        //     // Debug.Log($"{yaw}({yaw_}), {pitch}({pitch_})");
        //     // var rot = Quaternion.Euler(pitch, yaw, 0);
        //     var rot = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
        //     var p = rot * Vector3.forward;

        //     Gizmos.matrix = transform.localToWorldMatrix;
        //     Gizmos.DrawLine(Vector3.zero, p * 5);
        // }

    }
}
