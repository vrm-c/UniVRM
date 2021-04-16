using System;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// ヒエラルキーから VRM10SpringBone と VRM10ColliderGroup を集めてリスト表示する
    /// </summary>
    public class VRM10SpringSelectorWindow : EditorWindow
    {
        const string MENU_KEY = VRMVersion.MENU + "/SpringBone Window";

        [MenuItem(MENU_KEY, false, 0)]
        private static void ExportFromMenu()
        {
            var window = (VRM10SpringSelectorWindow)GetWindow(typeof(VRM10SpringSelectorWindow));
            window.titleContent = new GUIContent("SpringBone selector");
            window.Show();
            window.Root = Selection.activeTransform;
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            Selection.selectionChanged += Repaint;
        }

        void OnDisable()
        {
            // Debug.Log("OnDisable");
            Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;
        }

        Transform m_root;
        Transform Root
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
                if (m_root != null)
                {
                    m_springs = m_root.GetComponentsInChildren<VRM10SpringBone>();
                    m_colliderGroups = m_root.GetComponentsInChildren<VRM10SpringBoneColliderGroup>();
                }
            }
        }

        static bool s_foldSprings = true;
        public VRM10SpringBone[] m_springs;

        static bool s_foldColliders = true;
        public VRM10SpringBoneColliderGroup[] m_colliderGroups;

        void Reload()
        {
            var backup = Root;
            Root = null;
            Root = backup;
        }

        Vector2 m_scrollPosition;

        private void OnGUI()
        {
            Root = (Transform)EditorGUILayout.ObjectField("vrm1 root", m_root, typeof(Transform), true);

            if (m_springs != null && m_springs.Length > 0 && m_springs[0] == null)
            {
                Reload();
            }

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            DrawContent();
            EditorGUILayout.EndScrollView();
        }

        void DrawContent()
        {
            GUI.enabled = false;
            s_foldSprings = EditorGUILayout.Foldout(s_foldSprings, "springs");
            if (s_foldSprings)
            {
                if (m_springs != null)
                {
                    foreach (var s in m_springs)
                    {
                        EditorGUILayout.ObjectField(s, typeof(VRM10SpringBone), true);
                    }
                }
            }

            s_foldColliders = EditorGUILayout.Foldout(s_foldColliders, "colliders");
            if (s_foldColliders)
            {
                if (m_colliderGroups != null)
                {
                    foreach (var c in m_colliderGroups)
                    {
                        EditorGUILayout.ObjectField(c, typeof(VRM10SpringBoneColliderGroup), true);
                    }
                }
            }
        }
    }
}
