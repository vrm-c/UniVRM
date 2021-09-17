using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UniGLTF;

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

        BoneSelector _impl;
        void OnEnable()
        {
            EditorTools.activeToolChanged += ActiveToolDidChange;
            if (SceneView.lastActiveSceneView?.camera)
            {
            }
        }

        void OnDisable()
        {
            EditorTools.activeToolChanged -= ActiveToolDidChange;
            if (_impl != null)
            {
                _impl.Dispose();
                _impl = null;
            }
        }

        void ActiveToolDidChange()
        {
            if (EditorTools.IsActiveTool(this))
            {
            }
            else
            {
                if (_impl != null)
                {
                    _impl.Dispose();
                    _impl = null;
                }
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (_impl == null)
            {
                _impl = new BoneSelector(SceneView.lastActiveSceneView.camera);
            }

            var root = Selection.activeGameObject?.GetComponent<VRM10Controller>();
            if (root == null)
            {
                return;
            }
            _impl.SetTarget(root.gameObject);
            if (Event.current.type == EventType.Repaint)
            {
                _impl.Draw();
            }

            // bone manipulator
            var selected = _impl.SelectedBoneInfo;
            bool selector = true;
            if (selected != null)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion rot = Handles.RotationHandle(selected.HeadObject.transform.rotation, selected.HeadObject.transform.position);
                // Debug.Log($"{selected}");
                if (EditorGUI.EndChangeCheck())
                {
                    // UNDO
                    Undo.RecordObject(selected.HeadObject.transform, "bone rotation");

                    // apply
                    selected.HeadObject.transform.rotation = rot;
                    selector = false;
                }
            }

            if (selector)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    // 回転ギズモがなんもしなかった
                    // selector
                    Vector2 mousePosition = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                    Event e = Event.current;
                    _impl.IntersectBone(ray);
                }
                else if (Event.current.type == EventType.MouseMove)
                {
                    // hover
                    Vector2 mousePosition = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                    Event e = Event.current;
                    _impl.IntersectBone(ray, true);
                }
            }

            // disable sceneView select
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}
