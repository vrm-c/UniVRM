using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;


namespace UniVRM10
{
    [EditorTool("vrm-1.0/LookAt", typeof(UniVRM10.VRM10Controller))]
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
            EditorTools.activeToolChanged += ActiveToolDidChange;
        }

        void OnDisable()
        {
            EditorTools.activeToolChanged -= ActiveToolDidChange;
        }

        void ActiveToolDidChange()
        {
            if (!EditorTools.IsActiveTool(this))
            {
                return;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            var root = Selection.activeTransform.GetComponent<VRM10Controller>();
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

            if (!Application.isPlaying)
            {
                // offset
                var p = root.Vrm.LookAt.OffsetFromHead;
                Handles.Label(head.position, $"fromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]");
            }
            else
            {
                root.Vrm.LookAt.OnSceneGUILookAt(head, root.LookAtTargetType, root.Gaze);
            }
        }
    }
}
