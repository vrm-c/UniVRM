using UnityEngine;
using UnityEditor;
using UniGLTF.M17N;
using System.Collections.Generic;
using System.Linq;

namespace UniGLTF.MeshUtility
{
    public class MeshUtilityDialog : EditorWindow
    {
        public const string MENU_NAME = "glTF MeshUtility";
        public static void OpenWindow()
        {
            var window =
                (MeshUtilityDialog)EditorWindow.GetWindow(typeof(MeshUtilityDialog));
            window.titleContent = new GUIContent(MENU_NAME);
            window.Show();
        }

        protected enum Tabs
        {
            Freeze,
            IntegrateSplit,
            BoneMeshEraser,
        }
        protected Tabs _tab;
        protected GameObject _exportTarget;

        GltfMeshUtility _meshUtil;
        protected virtual GltfMeshUtility MeshUtility
        {
            get
            {
                if (_meshUtil == null)
                {
                    _meshUtil = new GltfMeshUtility();
                }
                return _meshUtil;
            }
        }
        MeshIntegrationTab _integrationTab;
        protected virtual MeshIntegrationTab MeshIntegration
        {
            get
            {
                if (_integrationTab == null)
                {
                    _integrationTab = new MeshIntegrationTab(this, MeshUtility);
                }
                return _integrationTab;
            }
        }

        protected List<Validation> _validations = new List<Validation>();
        protected virtual void Validate()
        {
            _validations.Clear();
            if (_exportTarget == null)
            {
                _validations.Add(Validation.Error("set target GameObject"));
                return;
            }
        }
        bool IsValid => !_validations.Any(v => !v.CanExport);
        Vector2 _scrollPos;

        void OnEnable()
        {
        }

        private void OnGUI()
        {
            var modified = false;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUIUtility.labelWidth = 200;
            LanguageGetter.OnGuiSelectLang();
            var exportTarget = (GameObject)EditorGUILayout.ObjectField(
                MeshUtilityMessages.TARGET_OBJECT.Msg(),
                _exportTarget, typeof(GameObject), true);
            if (exportTarget != _exportTarget)
            {
                _exportTarget = exportTarget;
                MeshIntegration.UpdateMeshIntegrationList(_exportTarget);
                modified = true;
            }
            _tab = TabBar.OnGUI(_tab, "LargeButton", GUI.ToolbarButtonSize.Fixed);

            foreach (var validation in _validations)
            {
                validation.DrawGUI();
            }

            EditorGUILayout.HelpBox(MeshUtilityMessages.MESH_SEPARATOR.Msg(), MessageType.Info);
            EditorGUILayout.HelpBox(MeshUtilityMessages.MESH_INTEGRATOR.Msg(), MessageType.Info);

            switch (_tab)
            {
                case Tabs.Freeze:
                    {
                        if (MeshFreezeGui())
                        {
                            modified = true;
                        }
                        break;
                    }

                case Tabs.IntegrateSplit:
                    {
                        if (MeshIntegrateGui())
                        {
                            modified = true;
                        }
                        break;
                    }

                case Tabs.BoneMeshEraser:
                    {
                        // TODO: FirstPerson 処理と統合する
                        EditorGUILayout.HelpBox(MeshUtilityMessages.BONE_MESH_ERASER.Msg(), MessageType.Info);
                        // if (_boneMeshEraserEditor)
                        // {
                        //     _boneMeshEraserEditor.OnInspectorGUI();
                        // }
                        // if (TabBoneMeshRemover.TryExecutable(_exportTarget, _skinnedMeshRenderer, out string msg))
                        // {
                        //     processed = TabBoneMeshRemover.OnGUI(_exportTarget, _skinnedMeshRenderer, _eraseBones);
                        // }
                        // else
                        // {
                        //     EditorGUILayout.HelpBox(msg, MessageType.Error);
                        // }
                        break;
                    }
            }
            EditorGUILayout.EndScrollView();

            if (modified)
            {
                Validate();
            }

            GUI.enabled = IsValid;
            var pressed = GUILayout.Button("Process", GUILayout.MinWidth(100));
            GUI.enabled = true;
            if (pressed)
            {
                Undo.RegisterFullObjectHierarchyUndo(exportTarget, "MeshUtility");
                foreach (var go in MeshUtility.Process(exportTarget))
                {
                    Undo.RegisterCreatedObjectUndo(go, "MeshUtility");
                }
                _exportTarget = null;
                // Show Result ?
                // Close();
                // GUIUtility.ExitGUI();
            }
        }

        protected bool ToggleIsModified(string label, ref bool value)
        {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue == value)
            {
                return false;
            }
            value = newValue;
            return true;
        }

        bool MeshFreezeGui()
        {
            var forceUniqueName = ToggleIsModified("ForceUniqueName", ref MeshUtility.ForceUniqueName);
            var blendShape = ToggleIsModified("BlendShape", ref MeshUtility.FreezeBlendShape);
            var scale = ToggleIsModified("Scale", ref MeshUtility.FreezeScaling);
            var rotation = ToggleIsModified("Rotation", ref MeshUtility.FreezeRotation);
            return forceUniqueName || blendShape || scale || rotation;
        }

        protected virtual bool MeshIntegrateGui()
        {
            var split = ToggleIsModified("Separate by BlendShape", ref MeshUtility.SplitByBlendShape);
            var p = position;
            var last = GUILayoutUtility.GetLastRect();
            var y = last.y + last.height;
            var rect = new Rect
            {
                x = last.x,
                y = y,
                width = p.width,
                height = p.height - y
                // process button の高さ
                - 30
            };
            var mod = MeshIntegration.OnGui(rect);
            return split || mod;
        }
    }
}