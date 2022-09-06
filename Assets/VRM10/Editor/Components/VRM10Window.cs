using System;
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
        const string WINDOW_TITLE = "VRM1 Window";
        public static void Open()
        {
            var window = (VRM10Window)GetWindow(typeof(VRM10Window));
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.Show();
            window.Root = UnityEditor.Selection.activeTransform?.GetComponent<Vrm10Instance>();
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            UnityEditor.Selection.selectionChanged += Repaint;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SpringBoneEditor.Disable();

            SceneView.duringSceneGui -= OnSceneGUI;
            // Debug.Log("OnDisable");
            UnityEditor.Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;

            Tools.hidden = false;
        }

        SerializedObject m_so;
        int? m_root;
        Vrm10Instance Root
        {
            get => m_root.HasValue ? (EditorUtility.InstanceIDToObject(m_root.Value) as Vrm10Instance) : null;
            set
            {
                int? id = value != null ? value.GetInstanceID() : default;
                if (m_root == id)
                {
                    return;
                }
                if (value != null && !value.gameObject.scene.IsValid())
                {
                    // skip prefab
                    return;
                }
                m_root = id;
                m_so = value != null ? new SerializedObject(value) : null;
                m_constraints = null;

                if (Root != null)
                {
                    var animator = Root.GetComponent<Animator>();
                    m_head = animator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        Transform m_head;

        public IVrm10Constraint[] m_constraints;

        ScrollView m_scrollView = new ScrollView();

        enum VRMSceneUI
        {
            Constraints,
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

        /// <summary>
        /// public entry point
        /// </summary>
        /// <param name="target"></param>
        void OnSceneGUI(SceneView sceneView)
        {
            switch (s_ui)
            {
                case VRMSceneUI.Constraints:
                    Tools.hidden = false;
                    break;

                case VRMSceneUI.SpringBone:
                    Tools.hidden = true;
                    SpringBoneEditor.Draw3D(Root, m_so);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        //
        private void OnGUI()
        {
            if (Root == null)
            {
                if (UnityEditor.Selection.activeTransform != null)
                {
                    var root = UnityEditor.Selection.activeTransform.Ancestors().Select(x => x.GetComponent<Vrm10Instance>()).FirstOrDefault(x => x != null);
                    if (root != null)
                    {
                        Root = root;
                    }
                }
            }

            Root = (Vrm10Instance)EditorGUILayout.ObjectField("vrm1", Root, typeof(Vrm10Instance), true);
            if (Root == null)
            {
                return;
            }

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
                case VRMSceneUI.Constraints:
                    m_scrollView.Draw(this.position.y, DrawConstraints, Repaint);
                    break;

                case VRMSceneUI.SpringBone:
                    SpringBoneEditor.Draw2D(Root, m_so);
                    break;

                default:
                    throw new NotImplementedException();
            }

            m_so.ApplyModifiedProperties();
        }

        void DrawConstraints()
        {
            if (Root != null)
            {
                if (m_constraints == null)
                {
                    m_constraints = Root.GetComponentsInChildren<IVrm10Constraint>();
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                if (m_constraints != null)
                {
                    foreach (var c in m_constraints)
                    {
                        EditorGUILayout.ObjectField(c.ConstraintTarget, typeof(MonoBehaviour), true);
                    }
                }
            }
        }
    }
}
