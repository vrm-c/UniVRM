using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMLookAtHead))]
    public class VRMLookAtHeadEditor : Editor
    {
        VRMLookAtHead m_target;
        PreviewRenderUtility m_previewRenderUtility;

        void OnEnable()
        {
            m_target = (VRMLookAtHead)target;
            m_previewRenderUtility = new PreviewRenderUtility(true);
        }

        private void OnDisable()
        {
            m_previewRenderUtility.Cleanup();
            m_previewRenderUtility = null;
        }

        static void SetPreviewCamera(Camera camera, Vector3 target, Vector3 forward)
        {
            camera.fieldOfView = 30f;
            camera.farClipPlane = 100;
            camera.nearClipPlane = 0.1f;

            camera.transform.position = target + forward * 0.8f;
            camera.transform.LookAt(target);
            camera.Render();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            m_previewRenderUtility.BeginPreview(r, background);
            var target = m_target.Head.Transform;
            if (target != null)
            {
                SetPreviewCamera(m_previewRenderUtility.m_Camera,
                    target.position + new Vector3(0, 0.1f, 0),
                    target.forward
                    );
            }
            m_previewRenderUtility.EndAndDrawPreview(r);
        }

        const float RADIUS = 0.5f;

        void OnSceneGUI()
        {
            //if (!Application.isPlaying) return;
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
