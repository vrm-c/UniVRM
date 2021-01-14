using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace UniVRM10
{
    public class VRMSpringBoneWindow : EditorWindow
    {
        [MenuItem(VRMVersion.MENU + "/VRMSpringBoneWindow")]
        public static void ShowEditorWindow()
        {
            var wnd = GetWindow<VRMSpringBoneWindow>();
            wnd.titleContent = new GUIContent("VRMSpringBoneWindow");
        }

        void OnEnable()
        {
            minSize = new Vector2(100, 100);
            maxSize = new Vector2(4000, 4000);
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        VRM10Controller m_currentRoot;

        [SerializeField]
        TreeViewState m_treeViewState;
        VRMSpringBoneTreeView m_treeView;

        Rect m_treeRect;
        Rect m_inspectorRect;
        Vector2 m_inspectorScrollPos;

        void OnGUI()
        {
            if (m_treeView is null)
            {
                return;
            }

            // bone selector           
            Rect fullRect = GUILayoutUtility.GetRect(0, 100000, 0, 10000);
            var treeRect = new Rect(fullRect.x, fullRect.y, fullRect.width, EditorGUIUtility.singleLineHeight * 3);
            var inspectorRect = new Rect(fullRect.x, treeRect.y + treeRect.height, fullRect.width, fullRect.height - treeRect.height);
            if (Event.current.type == EventType.Repaint)
            {
                m_treeRect = treeRect;
                m_inspectorRect = inspectorRect;
                // Debug.Log($"{m_treeRect}, {m_inspectorRect}");
            }

            m_treeView.OnGUI(m_treeRect);

            GUILayout.BeginArea(m_inspectorRect);
            m_inspectorScrollPos = GUILayout.BeginScrollView(m_inspectorScrollPos, GUI.skin.box);
            if (m_treeViewState.selectedIDs.Any())
            {
                // selected な SpringBone の Inspector を描画する
                if (m_treeView.TryGetSpringBone(m_treeViewState.selectedIDs[0], out VRMSpringBone bone))
                {
                    // Debug.Log(bone);
                    using (var inspector = new CustomInspector(new SerializedObject(bone)))
                    {
                        inspector.OnInspectorGUI();
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void OnSelectionChanged()
        {
            var go = Selection.activeObject as GameObject;
            if (go is null)
            {
                return;
            }

            var meta = go.GetComponentInParent<VRM10Controller>();
            if (meta == m_currentRoot)
            {
                return;
            }

            m_currentRoot = meta;
            if (m_currentRoot is null)
            {
                m_treeViewState = null;
                m_treeView = null;
            }
            else
            {
                // update treeview
                Debug.Log(m_currentRoot);
                m_treeViewState = new TreeViewState();
                m_treeView = new VRMSpringBoneTreeView(m_treeViewState, m_currentRoot);
            }

            Repaint();
        }
    }
}
