using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10RotationConstraint))]
    public class VRM10RotationConstraintEditor : Editor
    {
        VRM10RotationConstraint m_target;

        void OnEnable()
        {
            m_target = (VRM10RotationConstraint)target;
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

        /// <summary>
        /// Euler各を +- 180 にクランプする
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        static Vector3 Clamp180(Vector3 v)
        {
            var x = v.x;
            while (x < -180) x += 360;
            while (x > 180) x -= 360;
            var y = v.y;
            while (y < -180) y += 360;
            while (y > 180) y -= 360;
            var z = v.z;
            while (z < -180) z += 360;
            while (z > 180) z -= 360;
            return new Vector3(x, y, z);
        }

        public void OnSceneGUI()
        {
            if (m_target.Source == null)
            {
                return;
            }

            // source offset
            if (!Application.isPlaying)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion sourceOffset = Handles.RotationHandle(m_target.SourceOffset.Rotation, m_target.Source.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target, "source offset");
                    m_target.SourceOffset.Rotation = sourceOffset;
                }
            }

            // this to target line
            Handles.color = Color.yellow;
            Handles.DrawLine(m_target.Source.position, m_target.transform.position);

            var delta = Clamp180(m_target.Delta.eulerAngles);

            // show source
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"source: {m_target.SourceCoordinate}");
                sb.AppendLine($"{delta.x:0.}");
                sb.AppendLine($"{delta.y:0.}");
                sb.Append($"{delta.z:0.}");
                Handles.Label(m_target.Source.position, sb.ToString(), Style);
            }

            // show dst
            {
                var sb = new StringBuilder();
                sb.AppendLine($"constraint: {m_target.DestinationCoordinate}");
                sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.X) ? $"freeze" : $"{delta.x:0.}");
                sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.Y) ? $"freeze" : $"{delta.y:0.}");
                sb.Append(m_target.FreezeAxes.HasFlag(AxisMask.Z) ? $"freeze" : $"{delta.z:0.}");
                Handles.Label(m_target.transform.position, sb.ToString(), Style);
            }

            m_target.DrawSourceCoords();
            m_target.DrawSourceCurrent();
            m_target.DrawDstCoords();
            m_target.DrawDstCurrent();
        }
    }
}
