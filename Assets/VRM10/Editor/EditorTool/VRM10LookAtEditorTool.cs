using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

#if UNITY_2021_OR_NEWER
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif


namespace UniVRM10
{
    [EditorTool("vrm-1.0/LookAt", typeof(UniVRM10.Vrm10Instance))]
    class VRM10LookAtEditorTool : EditorTool
    {
        static GUIContent s_cachedIcon;
        public override GUIContent toolbarIcon
        {
            get
            {
                if (s_cachedIcon == null)
                {
                    s_cachedIcon = EditorGUIUtility.IconContent("d_BillboardRenderer Icon", "|vrm-1.0 LookAt");
                }
                return s_cachedIcon;
            }
        }

        void OnEnable()
        {
            ToolManager.activeToolChanged += ActiveToolDidChange;
        }

        void OnDisable()
        {
            ToolManager.activeToolChanged -= ActiveToolDidChange;
        }

        void ActiveToolDidChange()
        {
            if (!ToolManager.IsActiveTool(this))
            {
                return;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if(Selection.activeTransform==null)
            {
                return;
            }
            var root = Selection.activeTransform.GetComponent<Vrm10Instance>();
            if (root == null)
            {
                return;
            }
            if (!root.DrawLookAtGizmo)
            {
                return;
            }
            var humanoid = root.GetComponent<UniHumanoid.Humanoid>();
            var head = humanoid.Head;
            if (head == null)
            {
                return;
            }

            {
                EditorGUI.BeginChangeCheck();

                var worldOffset = head.localToWorldMatrix.MultiplyPoint(root.Vrm.LookAt.OffsetFromHead);
                worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

                Handles.DrawDottedLine(head.position, worldOffset, 5);
                Handles.SphereHandleCap(0, head.position, Quaternion.identity, 0.02f, Event.current.type);
                Handles.SphereHandleCap(0, worldOffset, Quaternion.identity, 0.02f, Event.current.type);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(root.Vrm, "LookAt.OffsetFromHead");

                    root.Vrm.LookAt.OffsetFromHead = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
                }
            }

            if (Application.isPlaying)
            {
                OnSceneGUILookAt(root.Vrm.LookAt, root.Runtime.LookAt, head, root.LookAtTargetType, root.Gaze);
            }
            else
            {
                // offset
                var p = root.Vrm.LookAt.OffsetFromHead;
                Handles.Label(head.position, $"fromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]");
            }
        }

        #region Gizmo
        static void DrawMatrix(Matrix4x4 m, float size)
        {
            Gizmos.matrix = m;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.right * size);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * size);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * size);
        }

        const float LOOKAT_GIZMO_SIZE = 0.5f;

        // private void OnDrawGizmos()
        // {
        //     if (DrawGizmo)
        //     {
        //         if (m_leftEye != null & m_rightEye != null)
        //         {
        //             DrawMatrix(m_leftEye.localToWorldMatrix, LOOKAT_GIZMO_SIZE);
        //             DrawMatrix(m_rightEye.localToWorldMatrix, LOOKAT_GIZMO_SIZE);
        //         }
        //     }
        // }
        #endregion

        const float RADIUS = 0.5f;

        static void OnSceneGUILookAt(VRM10ObjectLookAt lookAt, Vrm10RuntimeLookAt runtime, Transform head, VRM10ObjectLookAt.LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            if (head == null) return;

            if (gaze != null)
            {
                {
                    EditorGUI.BeginChangeCheck();
                    var newTargetPosition = Handles.PositionHandle(gaze.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(gaze, "Change Look At Target Position");
                        gaze.position = newTargetPosition;
                    }
                }

                Handles.color = new Color(1, 1, 1, 0.6f);
                Handles.DrawDottedLine(runtime.GetLookAtOrigin(head).position, gaze.position, 4.0f);
            }

            var (yaw, pitch) = runtime.GetLookAtYawPitch(head, lookAtTargetType, gaze);
            var lookAtOriginMatrix = runtime.GetLookAtOrigin(head).localToWorldMatrix;
            Handles.matrix = lookAtOriginMatrix;
            var p = lookAt.OffsetFromHead;
            Handles.Label(Vector3.zero,
            $"FromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]\nYaw: {yaw:0.}degree\nPitch: {pitch:0.}degree");

            Handles.color = new Color(0, 1, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    Matrix4x4.identity.GetColumn(1),
                    Matrix4x4.identity.GetColumn(2),
                    yaw,
                    RADIUS);


            var yawQ = Quaternion.AngleAxis(yaw, Vector3.up);
            var yawMatrix = default(Matrix4x4);
            yawMatrix.SetTRS(Vector3.zero, yawQ, Vector3.one);

            Handles.matrix = lookAtOriginMatrix * yawMatrix;
            Handles.color = new Color(1, 0, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    Matrix4x4.identity.GetColumn(0),
                    Matrix4x4.identity.GetColumn(2),
                    -pitch,
                    RADIUS);
        }
    }
}
