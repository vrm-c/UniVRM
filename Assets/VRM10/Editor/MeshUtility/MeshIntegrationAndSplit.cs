using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniVRM10
{
    class MeshIntegrationAndSplit
    {
        class MeshIntegrationGroup
        {
            public string Name;
            public List<Renderer> Renderers = new List<Renderer>();
        }
        List<MeshIntegrationGroup> _meshIntegrationList = new List<MeshIntegrationGroup>();
        List<Renderer> _renderers = new List<Renderer>();

        Splitter _splitter;
        ReorderableList _groupList;
        ReorderableList _rendererList;
        int _selected = -1;
        int Selected
        {
            set
            {
                if (_selected == value)
                {
                    return;
                }
                if (value < 0 || value >= _meshIntegrationList.Count)
                {
                    return;
                }
                _selected = value;
                _renderers.Clear();
                _renderers.AddRange(_meshIntegrationList[_selected].Renderers);
            }
        }

        public MeshIntegrationAndSplit(EditorWindow editor)
        {
            _splitter = new VerticalSplitter(editor, 50, 50);

            _groupList = new ReorderableList(_meshIntegrationList, typeof(MeshIntegrationGroup));
            _groupList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "Integration group");
            };
            _groupList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var group = _meshIntegrationList[index];
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

        public void UpdateMeshIntegrationList(GameObject root)
        {
            _selected = -1;
            _meshIntegrationList.Clear();

            IntegrateFirstPerson(root);
            Selected = 0;
        }

        void IntegrateAll(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            _meshIntegrationList.Add(new MeshIntegrationGroup
            {
                Name = "ALL",
                Renderers = root.GetComponentsInChildren<Renderer>().ToList(),
            });
        }

        MeshIntegrationGroup GetOrCreateGroup(string name)
        {
            foreach (var g in _meshIntegrationList)
            {
                if (g.Name == name)
                {
                    return g;
                }
            }
            _meshIntegrationList.Add(new MeshIntegrationGroup
            {
                Name = name,
            });
            return _meshIntegrationList.Last();
        }

        void IntegrateFirstPerson(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            var vrm1 = root.GetComponent<Vrm10Instance>();
            if (vrm1 == null)
            {
                return;
            }
            var vrmObject = vrm1.Vrm;
            if (vrmObject == null)
            {
                return;
            }
            var fp = vrmObject.FirstPerson;
            if (fp == null)
            {
                return;
            }
            foreach (var a in fp.Renderers)
            {
                var g = GetOrCreateGroup(a.FirstPersonFlag.ToString());
                g.Renderers.Add(a.GetRenderer(root.transform));
            }
        }

        private void ShowGroup(Rect rect)
        {
            _groupList.DoList(rect);
        }

        private void ShowSelected(Rect rect)
        {
            _rendererList.DoList(rect);
        }

        public void OnGui(Rect rect)
        {
            _splitter.OnGUI(
               rect,
               ShowGroup,
               ShowSelected);
        }
    }
}