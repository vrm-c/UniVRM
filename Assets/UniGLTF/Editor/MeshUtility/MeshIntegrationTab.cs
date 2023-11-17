using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrationTab
    {
        bool _modified = false;
        protected GltfMeshUtility _meshUti;

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
                if (value < 0 || value >= _meshUti.MeshIntegrationGroups.Count)
                {
                    return;
                }
                _selected = value;
                _renderers.Clear();
                _renderers.AddRange(_meshUti.MeshIntegrationGroups[_selected].Renderers);
            }
        }

        public MeshIntegrationTab(EditorWindow editor, GltfMeshUtility meshUtility)
        {
            _meshUti = meshUtility;
            _splitter = new VerticalSplitter(editor, 200, 50);

            _groupList = new ReorderableList(_meshUti.MeshIntegrationGroups, typeof(MeshIntegrationGroup));
            _groupList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "Integration group");
            };
            _groupList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var group = _meshUti.MeshIntegrationGroups[index];
                EditorGUI.TextField(rect, group.Name);
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
        }

        public virtual void UpdateMeshIntegrationList(GameObject root)
        {
            _selected = -1;
            _meshUti.MeshIntegrationGroups.Clear();
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