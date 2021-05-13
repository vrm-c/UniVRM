using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10PositionConstraint))]
    public class VRM10PositionConstraintEditor : Editor
    {
        VRM10PositionConstraint m_target;

        void OnEnable()
        {
            m_target = (VRM10PositionConstraint)target;
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

            var delta = m_target.Delta;

            // show source
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"source: {m_target.SourceCoordinate}");
                sb.AppendLine($"{delta.x:0.00}");
                sb.AppendLine($"{delta.y:0.00}");
                sb.Append($"{delta.z:0.00}");
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
