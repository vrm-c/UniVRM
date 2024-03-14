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
        public const string MENU_NAME = "VRM 1.0 Window";

        public static VRM10Window Open()
        {
            var window = (VRM10Window)GetWindow(typeof(VRM10Window));
            window.titleContent = new GUIContent(MENU_NAME);
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
            s_treeView = null;

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
            }
        }

        /// <summary>
        /// Scene 上の 3D 表示
        /// 
        /// * Joint/Collider の Picker
        /// 
        /// </summary>
        void OnSceneGUI(SceneView sceneView)
        {
            Tools.hidden = true;
            Draw3D(Root, m_so);
        }

        /// <summary>
        /// Window 上の GUI
        /// 
        /// * 対象 VRM の保持
        /// * 選択 Joint/Collider の表示
        /// 
        /// </summary>
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
            Root = (Vrm10Instance)EditorGUILayout.ObjectField("Editing Model", Root, typeof(Vrm10Instance), true);
            if (Root == null)
            {
                return;
            }

            // active
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Selected Object", Active, typeof(MonoBehaviour), true);
            EditorGUI.EndDisabledGroup();

            // if (m_so == null)
            // {
            //     m_so = new SerializedObject(Root);
            // }
            // if (m_so == null)
            // {
            //     return;
            // }
            // m_so.Update();
            // SpringBoneEditor.Draw2D(Root, m_so);
            // m_so.ApplyModifiedProperties();
        }

        SpringBoneTreeView s_treeView;
        SpringBoneTreeView GetTree(Vrm10Instance target, SerializedObject so)
        {
            if (s_treeView == null || s_treeView.Target != target)
            {
                var state = new UnityEditor.IMGUI.Controls.TreeViewState();
                s_treeView = new SpringBoneTreeView(state, target, so);
                s_treeView.Reload();
            }
            return s_treeView;
        }

        public static MonoBehaviour Active;

        /// <summary>
        /// 3D の Handle 描画
        /// </summary>
        public void Draw3D(Vrm10Instance target, SerializedObject so)
        {
            var tree = GetTree(target, so);
            if (tree != null && target != null)
            {
                if (tree.Draw3D(target.SpringBone))
                {
                    Repaint();
                }
                Active = tree.Active;
            }
        }
    }
}
