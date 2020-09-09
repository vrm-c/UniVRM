using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using System;

namespace MeshUtility.Gizmo
{
    /// <summary>
    /// EditorTool のエントリポイント。
    /// Unityイベントとのつなぎこみ。
    /// 
    /// TODO:
    /// 
    /// * hover
    /// * cursor icon
    /// * bone rotation
    /// 
    /// </summary>
    [EditorTool("Bone Edit Tool")]
    public class BoneEditTool : EditorTool
    {
        [SerializeField]
        private Texture2D _toolIcon = null;

        private GUIContent _content = null;
        public override GUIContent toolbarIcon
        {
            get
            {
                if (_content == null)
                {
                    if (_toolIcon == null)
                    {
                        // TODO: load icon
                    }
                    _content = new GUIContent()
                    {
                        image = _toolIcon,     //アイコンの画像
                        text = "Bone Preview", //メニューの名前
                        tooltip = "Bone Preview Tool"   //メニューの説明
                    };
                }
                return _content;
            }
        }

        private void OnEnable()
        {
            EditorTools.activeToolChanged += OnActiveToolChange;

            // if (EditorApplication.isPlaying)
            {
                // OnActiveToolChange is not called
                // When call OnDisable, OnEnable when play or recompiled.
                EditorApplication.delayCall += OnActiveToolChange;
            }
        }

        void OnDisable()
        {
            EditorTools.activeToolChanged -= OnActiveToolChange;
        }

        BoneSelector _impl;

        Action _onSelectionChanged;
        // Action<SceneView> _onSceneViewEvent;

        private void OnActiveToolChange()
        {
            if (EditorTools.IsActiveTool(this))
            {
                _impl = new BoneSelector(SceneView.lastActiveSceneView.camera);
                _onSelectionChanged = () =>
                {
                    _impl.OnSelectionChanged(Selection.activeGameObject);
                };
                Selection.selectionChanged += _onSelectionChanged;
                _onSelectionChanged();
            }
            else
            {
                if (_impl != null)
                {
                    // SceneView.duringSceneGui -= _onSceneViewEvent;
                    Selection.selectionChanged -= _onSelectionChanged;
                    _impl.Dispose();
                    _impl = null;
                }
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (_impl != null)
            {
                _impl.Update();

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
                        // apply
                        selected.HeadObject.transform.rotation = rot;
                        selector = false;
                    }
                }

                if (selector)
                {
                    // 回転ギズモがなんもしなかった
                    // selector
                    Vector2 mousePosition = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                    Event e = Event.current;
                    if (e.isMouse && e.button == 0 && e.type == EventType.MouseUp)
                    {
                        var hit = _impl.IntersectBone(ray);
                        if (hit != null)
                        {
                            // select
                            Selection.activeGameObject = hit;
                        }
                    }
                }
            }

            // disable sceneView select
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}
