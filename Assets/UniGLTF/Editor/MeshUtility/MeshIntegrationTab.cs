using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrationTab
    {
        bool _modified = false;
        protected GltfMeshUtility _meshUtil;

        Splitter _splitter;
        ReorderableList _groupList;
        ReorderableList _rendererList;
        public List<Renderer> _renderers = new List<Renderer>();
        protected int _selected = -1;
        protected int Selected
        {
            set
            {
                if (_selected == value)
                {
                    return;
                }
                if (value < 0 || value >= _meshUtil.MeshIntegrationGroups.Count)
                {
                    return;
                }
                _selected = value;
                _renderers.Clear();
                _renderers.AddRange(_meshUtil.MeshIntegrationGroups[_selected].Renderers);
            }
        }

        public MeshIntegrationTab(EditorWindow editor, GltfMeshUtility meshUtility)
        {
            _meshUtil = meshUtility;
            _splitter = new VerticalSplitter(editor, 200, 50);

            _groupList = new ReorderableList(_meshUtil.MeshIntegrationGroups, typeof(MeshIntegrationGroup));
            _groupList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "Integration group");
            };
            _groupList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                // Flag / Name
                var group = _meshUtil.MeshIntegrationGroups[index];

                const float LEFT_WIDTH = 92.0f;
                var left = rect;
                left.width = LEFT_WIDTH;
                var right = rect;
                right.width -= LEFT_WIDTH;
                right.x += LEFT_WIDTH;

                group.IntegrationType = (MeshIntegrationGroup.MeshIntegrationTypes)EditorGUI.EnumPopup(left, group.IntegrationType);
                group.Name = EditorGUI.TextField(right, group.Name);
            };
            _groupList.onSelectCallback = rl =>
            {
                Selected = (rl.selectedIndices.Count > 0) ? rl.selectedIndices[0] : -1;
            };

            _rendererList = new ReorderableList(_renderers, typeof(Renderer));
            _rendererList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "Renderer");
            };
            _rendererList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var r = _renderers[index];
                EditorGUI.ObjectField(rect, r, typeof(Renderer), true);
            };

            // +ボタンが押された時のコールバック
            _rendererList.onAddCallback = list => Debug.Log("+ clicked.");

            // -ボタンが押された時のコールバック
            _rendererList.onRemoveCallback = list => Debug.Log("- clicked : " + list.index + ".");
        }

        public void UpdateMeshIntegrationList(GameObject root)
        {
            _selected = -1;
            _meshUtil.UpdateMeshIntegrationGroups(root);
            Selected = 0;
        }

        private void ShowGroup(Rect rect)
        {
            _groupList.DoList(rect);
        }

        private void ShowSelected(Rect rect)
        {
            _rendererList.DoList(rect);
        }

        public bool OnGui(Rect rect)
        {
            _modified = false;
            _splitter.OnGUI(
               rect,
               ShowGroup,
               ShowSelected);
            return _modified;
        }
    }
}