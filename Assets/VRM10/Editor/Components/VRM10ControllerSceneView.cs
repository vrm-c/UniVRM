using System;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// SceneView
    /// </summary>
    public class VRM10ControllerSceneView
    {
        enum VRMSceneUI
        {
            None,
            LookAt,
            SpringBone,
        }
        static VRMSceneUI s_ui = default;
        static string[] s_selection;
        static string[] Selection
        {
            get
            {
                if (s_selection == null)
                {
                    s_selection = Enum.GetNames(typeof(VRMSceneUI));
                }
                return s_selection;
            }
        }

        static VRMSceneUI SelectUI(VRMSceneUI ui)
        {
            var size = SceneView.currentDrawingSceneView.position.size;

            var rect = new Rect(0, 0, size.x, EditorGUIUtility.singleLineHeight);
            return (VRMSceneUI)GUI.SelectionGrid(rect, (int)ui, Selection, 3);
        }

        public static void Draw(VRM10Controller m_target)
        {
            Handles.BeginGUI();
            s_ui = SelectUI(s_ui);
            Handles.EndGUI();

            switch (s_ui)
            {
                case VRMSceneUI.None:
                    Tools.hidden = false;
                    break;

                case VRMSceneUI.LookAt:
                    Tools.hidden = true;
                    OnSceneGUIOffset(m_target);
                    if (!Application.isPlaying)
                    {
                        // offset
                        var p = m_target.LookAt.OffsetFromHead;
                        Handles.Label(m_target.Head.position, $"fromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]");
                    }
                    else
                    {
                        m_target.LookAt.OnSceneGUILookAt(m_target.Head);
                    }
                    break;

                case VRMSceneUI.SpringBone:
                    Tools.hidden = true;
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        const int windowID = 1234;
        static Rect windowHandle;
        static Rect windowRect = new Rect(20, 20, 120, 50);
        static void DoMyWindow(int windowID)
        {
            // Make a very long rect that is 20 pixels tall.
            // This will make the window be resizable by the top
            // title bar - no matter how wide it gets.
            GUILayout.Button("hello");
            // GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        static void OnSceneGUIOffset(VRM10Controller m_target)
        {
            if (!m_target.LookAt.DrawGizmo)
            {
                return;
            }

            var head = m_target.Head;
            if (head == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var worldOffset = head.localToWorldMatrix.MultiplyPoint(m_target.LookAt.OffsetFromHead);
            worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

            Handles.DrawDottedLine(head.position, worldOffset, 5);
            Handles.SphereHandleCap(0, head.position, Quaternion.identity, 0.02f, Event.current.type);
            Handles.SphereHandleCap(0, worldOffset, Quaternion.identity, 0.02f, Event.current.type);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_target, "Changed FirstPerson");

                m_target.LookAt.OffsetFromHead = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
            }
        }
    }
}