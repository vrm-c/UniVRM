using System;
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

        #region SRC
        void DrawSourceCurrent()
        {
            var s = m_target.transform.lossyScale.x;
            Handles.matrix = m_target.GetSourceCurrent().TRS(0.05f * s);
            Handles.color = Color.yellow;
            Handles.DrawWireCube(Vector3.zero, Vector3.one);
        }

        void DrawSourceCoords()
        {
            var s = m_target.transform.lossyScale.x;
            m_target.GetSourceCoords().Draw(0.2f / s);
        }
        #endregion

        #region Dst
        void DrawDstCurrent()
        {
            var s = m_target.transform.lossyScale.x;
            Handles.matrix = m_target.GetDstCurrent().TRS(0.05f * s);
            Handles.color = Color.yellow;
            Handles.DrawWireCube(Vector3.zero, Vector3.one);
        }

        void DrawDstCoords()
        {
            var s = m_target.transform.lossyScale.x;
            m_target.GetDstCoords().Draw(0.2f / s);
        }
        #endregion

        static GUIStyle s_style;

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
            if (s_style == null)
            {
                s_style = new GUIStyle("box");
            }

            // this to target line
            Handles.color = Color.yellow;
            Handles.DrawLine(m_target.Source.position, m_target.transform.position);

            var euler = Clamp180(m_target.Delta.eulerAngles);

            // show source
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"source: {m_target.SourceCoordinate}");
                sb.AppendLine($"{euler.x:0.}");
                sb.AppendLine($"{euler.y:0.}");
                sb.Append($"{euler.z:0.}");
                Handles.Label(m_target.Source.position, sb.ToString(), s_style);
            }

            // show dst
            {
                var sb = new StringBuilder();
                sb.AppendLine($"constraint: {m_target.DestinationCoordinate}");
                sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.X) ? $"freeze" : $"{euler.x:0.}");
                sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.Y) ? $"freeze" : $"{euler.y:0.}");
                sb.Append(m_target.FreezeAxes.HasFlag(AxisMask.Z) ? $"freeze" : $"{euler.z:0.}");
                Handles.Label(m_target.transform.position, sb.ToString(), s_style);
            }

            DrawSourceCoords();
            DrawSourceCurrent();
            DrawDstCoords();
            DrawDstCurrent();
        }
    }
}
