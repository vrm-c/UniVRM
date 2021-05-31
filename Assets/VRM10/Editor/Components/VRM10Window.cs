using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VRM10操作 Window
    /// </summary>
    public class VRM10Window : EditorWindow
    {
        const string MENU_KEY = VRMVersion.MENU + "/VRM1 Window";
        const string WINDOW_TITLE = "VRM1 Window";

        [MenuItem(MENU_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var window = (VRM10Window)GetWindow(typeof(VRM10Window));
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.Show();
            window.Root = UnityEditor.Selection.activeTransform.GetComponent<VRM10Controller>();
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            UnityEditor.Selection.selectionChanged += Repaint;

            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        void OnDisable()
        {
            SpringBoneEditor.Disable();

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            // Debug.Log("OnDisable");
            UnityEditor.Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;

            Tools.hidden = false;
        }

        SerializedObject m_so;
        VRM10Controller m_root;
        VRM10Controller Root
        {
            get => m_root;
            set
            {
                if (m_root == value)
                {
                    return;
                }
                if (value != null && !value.gameObject.scene.IsValid())
                {
                    // skip prefab
                    return;
                }
                m_root = value;
                m_so = m_root != null ? new SerializedObject(m_root) : null;

                m_boneMap = null;
                m_constraints = null;
            }
        }

        static bool s_foldHumanoidBones = true;
        static HumanBodyBones[] s_bones;
        public Dictionary<HumanBodyBones, Transform> m_boneMap;

        static bool s_foldConstraints = true;
        public VRM10Constraint[] m_constraints;

        void Reload()
        {
            var backup = Root;
            Root = null;
            Root = backup;
        }

        Vector2 m_scrollPosition;


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
        static Rect s_windowRect = new Rect(20, 20, 400, 50);

        /// <summary>
        /// public entry point
        /// </summary>
        /// <param name="target"></param>
        void OnSceneGUI(SceneView sceneView)
        {
            switch (s_ui)
            {
                case VRMSceneUI.None:
                    Tools.hidden = false;
                    break;

                case VRMSceneUI.LookAt:
                    Tools.hidden = true;
                    LookAtEditor.Draw3D(m_root);
                    break;

                case VRMSceneUI.SpringBone:
                    Tools.hidden = true;
                    SpringBoneEditor.Draw3D(m_root);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        //
        private void OnGUI()
        {
            Root = (VRM10Controller)EditorGUILayout.ObjectField("vrm1", m_root, typeof(VRM10Controller), true);

            var ui = (VRMSceneUI)GUILayout.SelectionGrid((int)s_ui, Selection, 3);
            if (s_ui != ui)
            {
                s_ui = ui;
                SceneView.RepaintAll();
            }

            if (m_so == null)
            {
                m_so = new SerializedObject(Root);
            }
            if (m_so == null)
            {
                return;
            }

            m_so.Update();
            switch (s_ui)
            {
                case VRMSceneUI.None:
                    {
                        m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

                        // mouse wheel scroll part 1
                        var isScroll = Event.current.isScrollWheel;
                        if (isScroll)
                        {
                            m_scrollPosition += Event.current.delta * EditorGUIUtility.singleLineHeight;
                            if (m_scrollPosition.y < 0)
                            {
                                m_scrollPosition = Vector2.zero;
                            }
                        }

                        DrawContent();

                        // mouse wheel scroll part 2
                        var bottom = EditorGUILayout.GetControlRect();
                        if (isScroll)
                        {
                            var maxScroll = bottom.y - (this.position.size.y - EditorGUIUtility.singleLineHeight * 2);
                            // Debug.Log($"{bottom.y}: {this.position.size.y}: {maxScroll}");
                            if (m_scrollPosition.y > maxScroll)
                            {
                                m_scrollPosition = new Vector2(0, maxScroll);
                            }
                            Repaint();
                        }

                        EditorGUILayout.EndScrollView();

                    }
                    break;

                case VRMSceneUI.LookAt:
                    LookAtEditor.Draw2D(m_root);
                    break;

                case VRMSceneUI.SpringBone:
                    SpringBoneEditor.Draw2D(m_root, m_so);
                    break;

                default:
                    throw new NotImplementedException();
            }

            m_so.ApplyModifiedProperties();
        }

        void DrawContent()
        {
            if (m_root != null)
            {
                if (s_bones == null)
                {
                    var values = Enum.GetValues(typeof(HumanBodyBones));
                    s_bones = new HumanBodyBones[values.Length - 1];
                    int j = 0;
                    foreach (HumanBodyBones bone in values)
                    {
                        if (bone != HumanBodyBones.LastBone)
                        {
                            s_bones[j++] = bone;
                        }
                    }
                }
                if (m_boneMap == null)
                {
                    var animator = m_root.GetComponent<Animator>();
                    if (animator != null)
                    {
                        m_boneMap = new Dictionary<HumanBodyBones, Transform>();
                        foreach (var bone in s_bones)
                        {
                            var t = animator.GetBoneTransform(bone);
                            if (t != null)
                            {
                                m_boneMap.Add(bone, t);
                            }
                        }
                    }
                }
                if (m_constraints == null)
                {
                    m_constraints = m_root.GetComponentsInChildren<VRM10Constraint>();
                }
            }

            GUI.enabled = false;
            s_foldHumanoidBones = EditorGUILayout.Foldout(s_foldHumanoidBones, "humanoid bones");
            if (s_foldHumanoidBones)
            {
                if (m_boneMap != null)
                {
                    foreach (var bone in s_bones)
                    {
                        if (m_boneMap.TryGetValue(bone, out Transform t))
                        {

                        }
                        EditorGUILayout.ObjectField(bone.ToString(), t, typeof(Transform), true);
                    }
                }
            }

            s_foldConstraints = EditorGUILayout.Foldout(s_foldConstraints, "constraints");
            if (s_foldConstraints)
            {
                if (m_constraints != null)
                {
                    foreach (var c in m_constraints)
                    {
                        EditorGUILayout.ObjectField(c, typeof(VRM10Constraint), true);
                    }
                }
            }
        }
    }
}
