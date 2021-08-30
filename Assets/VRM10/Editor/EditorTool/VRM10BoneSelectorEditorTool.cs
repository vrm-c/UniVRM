using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace UniVRM10
{
    /// <summary>
    /// Bone Selector
    /// </summary>
    [EditorTool("vrm-1.0/Humanoid", typeof(UniVRM10.VRM10Controller))]
    public class VRM10BoneSelectorEditorTool : EditorTool
    {
        static GUIContent s_cachedIcon;
        public override GUIContent toolbarIcon
        {
            get
            {
                if (s_cachedIcon == null)
                {
                    s_cachedIcon = EditorGUIUtility.IconContent("AvatarSelector@2x", "|vrm-1.0 Humanoid");
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

        UniHumanoid.Humanoid _target;

        UniHumanoid.Humanoid Target
        {
            get
            {
                return _target;
            }
            set
            {
                if (_target == value)
                {
                    return;
                }
                _target = value;
                if (_target != null)
                {
                    // initialize
                }
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            var root = Selection.activeTransform.GetComponent<VRM10Controller>();
            if (root == null)
            {
                return;
            }

            Target = root.GetComponent<UniHumanoid.Humanoid>();


            // if (_impl != null)
            // {
            //     _impl.Update();

            //     // bone manipulator
            //     var selected = _impl.SelectedBoneInfo;
            //     bool selector = true;
            //     if (selected != null)
            //     {
            //         EditorGUI.BeginChangeCheck();
            //         Quaternion rot = Handles.RotationHandle(selected.HeadObject.transform.rotation, selected.HeadObject.transform.position);
            //         // Debug.Log($"{selected}");
            //         if (EditorGUI.EndChangeCheck())
            //         {
            //             // apply
            //             selected.HeadObject.transform.rotation = rot;
            //             selector = false;
            //         }
            //     }

            //     if (selector)
            //     {
            //         // 回転ギズモがなんもしなかった
            //         // selector
            //         Vector2 mousePosition = Event.current.mousePosition;
            //         Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            //         Event e = Event.current;
            //         if (e.isMouse && e.button == 0 && e.type == EventType.MouseUp)
            //         {
            //             var hit = _impl.IntersectBone(ray);
            //             if (hit != null)
            //             {
            //                 // select
            //                 Selection.activeGameObject = hit;
            //             }
            //         }
            //     }
            // }

            // // disable sceneView select
            // HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}
