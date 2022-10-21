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
        public const string WINDOW_TITLE = "VRM1.0 Model Editor";

        public static VRM10Window Open()
        {
            var window = (VRM10Window)GetWindow(typeof(VRM10Window));
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.Show();
            window.Root = UnityEditor.Selection.activeTransform?.GetComponent<Vrm10Instance>();
            return window;
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
                // m_constraints = null;

                if (Root != null)
                {
                    var animator = Root.GetComponent<Animator>();
                    m_head = animator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        Transform m_head;

        ScrollView m_scrollView = new ScrollView();

        /// <summary>
        /// public entry point
        /// </summary>
        /// <param name="target"></param>
        void OnSceneGUI(SceneView sceneView)
        {
            Tools.hidden = true;
            SpringBoneEditor.Draw3D(Root, m_so);
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

            // Root
            Root = (Vrm10Instance)EditorGUILayout.ObjectField("vrm1", Root, typeof(Vrm10Instance), true);
            if (Root == null)
            {
                return;
            }

            // active
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("joint or collider", SpringBoneEditor.Active, typeof(MonoBehaviour), true);
            EditorGUI.EndDisabledGroup();

            if (m_so == null)
            {
                m_so = new SerializedObject(Root);
            }
            if (m_so == null)
            {
                return;
            }

            m_so.Update();
            SpringBoneEditor.Draw2D(Root, m_so);

            m_so.ApplyModifiedProperties();
        }
    }
}
