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

        const int WINDOW_ID = 1234;
        const string WINDOW_TITLE = "VRM1";
        static Rect s_windowRect = new Rect(20, 20, 400, 50);

        /// <summary>
        /// public entry point
        /// </summary>
        /// <param name="target"></param>
        public static void Draw(VRM10Controller target, SerializedObject so)
        {
            //
            // 2d window
            //
            Handles.BeginGUI();
            s_windowRect = GUILayout.Window(WINDOW_ID, s_windowRect, id =>
            {
                s_ui = (VRMSceneUI)GUILayout.SelectionGrid((int)s_ui, Selection, 3);

                switch (s_ui)
                {
                    case VRMSceneUI.None:
                        break;

                    case VRMSceneUI.LookAt:
                        LookAtEditor.Draw2D(target);
                        break;

                    case VRMSceneUI.SpringBone:
                        SpringBoneEditor.Draw2D(target, so);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                GUI.DragWindow();
            }, WINDOW_TITLE);
            Handles.EndGUI();

            //
            // 3d handle
            //
            switch (s_ui)
            {
                case VRMSceneUI.None:
                    Tools.hidden = false;
                    break;

                case VRMSceneUI.LookAt:
                    Tools.hidden = true;
                    LookAtEditor.Draw3D(target);
                    break;

                case VRMSceneUI.SpringBone:
                    Tools.hidden = true;
                    SpringBoneEditor.Draw3D(target);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static void Disable()
        {
            SpringBoneEditor.Disable();
        }
    }
}
