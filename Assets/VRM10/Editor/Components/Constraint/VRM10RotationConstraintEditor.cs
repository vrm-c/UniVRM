using System;
using System.Text;
using UniGLTF.Extensions.VRMC_node_constraint;
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

        void DrawSrcModel()
        {
            var model = m_target.ModelRoot;
            var src = m_target.Source;
            if (model == null)
            {
                Handles.Label(src.position, "ModelRoot required");
                return;
            }

            const float size = 0.05f;
            Handles.color = Color.red;
            Handles.DrawLine(src.position, src.position + model.right * size);
            Handles.color = Color.green;
            Handles.DrawLine(src.position, src.position + model.up * size);
            Handles.color = Color.black;
            Handles.DrawLine(src.position, src.position + model.forward * size);
        }

        void DrawSrcLocal()
        {
            var s = m_target.transform.lossyScale.x;

            // init 
            Coords.Write(m_target.GetSourceLocalInit(), 0.2f / s);

            // current
            Handles.matrix = m_target.Source.localToWorldMatrix;
            Handles.color = Color.yellow;
            var size = 0.05f / s;
            Handles.DrawWireCube(Vector3.zero, new Vector3(size, size, size));
        }

        void DrawDstLocal()
        {
            var s = m_target.transform.lossyScale.x;

            // init
            Coords.Write(m_target.GetDstLocalInit(), 0.2f / s);

            // current
            Handles.matrix = m_target.transform.localToWorldMatrix;
            Handles.color = Color.yellow;
            var size = 0.05f / s;
            Handles.DrawWireCube(Vector3.zero, new Vector3(size, size, size));
        }

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

            switch (m_target.SourceCoordinate)
            {
                case ObjectSpace.model:
                    DrawSrcModel();
                    break;

                case ObjectSpace.local:
                    DrawSrcLocal();
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (m_target.DestinationCoordinate)
            {
                case ObjectSpace.model:
                    break;

                case ObjectSpace.local:
                    DrawDstLocal();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
