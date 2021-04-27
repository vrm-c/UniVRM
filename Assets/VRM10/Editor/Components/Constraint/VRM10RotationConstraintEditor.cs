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
            // init 

            // current

            // delta
        }

        private GUIStyle _style;

        public void OnSceneGUI()
        {
            if (m_target.Source == null)
            {
                return;
            }

            // this to target line
            Handles.color = Color.yellow;
            var rot = Quaternion.LookRotation(m_target.Source.position - m_target.transform.position, Vector3.up);
            var len = (m_target.Source.position - m_target.transform.position).magnitude;
            Handles.ArrowHandleCap(0, m_target.transform.position, rot, len, EventType.Repaint);

            // show delta
            if (_style == null)
            {
                _style = new GUIStyle("box");
            }
            var euler = m_target.Delta.eulerAngles;
            var sb = new StringBuilder();
            sb.AppendLine(m_target.SourceCoordinate.ToString());
            sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.X) ? $"{euler.x:0.} => 0" : $"{euler.x:0.}");
            sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.Y) ? $"{euler.y:0.} => 0" : $"{euler.y:0.}");
            sb.AppendLine(m_target.FreezeAxes.HasFlag(AxisMask.Z) ? $"{euler.z:0.} => 0" : $"{euler.z:0.}");
            Handles.Label(m_target.Source.position, sb.ToString(), _style);

            switch (m_target.SourceCoordinate)
            {
                case ObjectSpace.model:
                    DrawSrcModel();
                    break;

                case ObjectSpace.local:
                    DrawSrcLocal();
                    break;

                default:
                    throw new System.NotImplementedException();
            }
        }

    }
}
