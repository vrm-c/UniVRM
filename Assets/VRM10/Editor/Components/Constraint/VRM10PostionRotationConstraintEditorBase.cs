using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public abstract class VRM10PositionRotationConstraintEditorBase : Editor
    {
        VRM10RotationPositionConstraintBase m_target;

        void OnEnable()
        {
            m_target = (VRM10RotationPositionConstraintBase)target;
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
                Quaternion offset = Handles.RotationHandle(m_target.SourceOffset, m_target.Source.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target.GetComponent(), "source offset");
                    m_target.SourceOffset = offset;
                }
            }

            // dest offset
            if (!Application.isPlaying)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion offset = Handles.RotationHandle(m_target.DestinationOffset, m_target.GetComponent().transform.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target.GetComponent(), "dest offset");
                    m_target.DestinationOffset = offset;
                }
            }

            // this to target line
            Handles.color = Color.yellow;
            Handles.DrawLine(m_target.Source.position, m_target.GetComponent().transform.position);

            var delta = Clamp180(m_target.Delta);

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
                sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.X) ? $"freeze" : $"{delta.x:0.00}");
                sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.Y) ? $"freeze" : $"{delta.y:0.00}");
                sb.Append(m_target.FreezeAxes.HasFlag(AxisMask.Z) ? $"freeze" : $"{delta.z:0.00}");
                Handles.Label(m_target.GetComponent().transform.position, sb.ToString(), Style);
            }

            m_target.GetSourceCoords().Draw(0.2f);
            if (Application.isPlaying)
            {
                Handles.matrix = m_target.GetSourceCurrent().TRS(0.05f);
                Handles.color = Color.yellow;
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }

            m_target.GetDstCoords().Draw(0.2f);
            if (Application.isPlaying)
            {
                Handles.matrix = m_target.GetDstCurrent().TRS(0.05f);
                Handles.color = Color.yellow;
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
