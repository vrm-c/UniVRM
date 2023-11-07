using UnityEngine;
using UnityEditor;
using UniGLTF.M17N;
using System.Collections.Generic;
using UniGLTF.MeshUtility;
using UniGLTF;
using System.Linq;

namespace UniVRM10
{
    public class Vrm10MeshUtilityDialog : EditorWindow
    {
        const string TITLE = "Vrm10 Mesh Utility Window";
        enum Tabs
        {
            Freeze,
            IntegrateSplit,
        }
        Tabs _tab;

        public static void OpenWindow()
        {
            var window =
                (Vrm10MeshUtilityDialog)EditorWindow.GetWindow(typeof(Vrm10MeshUtilityDialog));
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

        Vrm10MeshUtility _meshUtility = new Vrm10MeshUtility();

        List<Validation> _validations = new List<Validation>();
        private void Validate()
        {
            _validations.Clear();
            if (_exportTarget == null)
            {
                _validations.Add(Validation.Error("set vrm1"));
                return;
            }
            if (_exportTarget.GetComponent<Vrm10Instance>() == null)
            {
                _validations.Add(Validation.Error("target is not vrm1"));
                return;
            }
        }
        bool IsValid => !_validations.Any(v => !v.CanExport);

        Vector2 _scrollPos;
        GameObject _exportTarget;
        MeshIntegrationTab _meshIntegration;
        void OnEnable()
        {
            _meshIntegration = new MeshIntegrationTab(this, _meshUtility);
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
                _meshIntegration.UpdateMeshIntegrationList(_exportTarget);
                modified = true;
            }
            _tab = TabBar.OnGUI(_tab, "LargeButton", GUI.ToolbarButtonSize.Fixed);

            foreach (var validation in _validations)
            {
                validation.DrawGUI();
            }

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
                _meshUtility.Process(exportTarget);

                // Show Result ?
                // Close();
                // GUIUtility.ExitGUI();
            }
        }

        bool ToggleIsModified(string label, ref bool value)
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
            var forceUniqueName = ToggleIsModified("ForceUniqueName", ref _meshUtility.ForceUniqueName);
            var blendShape = ToggleIsModified("BlendShape", ref _meshUtility.FreezeBlendShape);
            var scale = ToggleIsModified("Scale", ref _meshUtility.FreezeScaling);
            var rotation = ToggleIsModified("Rotation", ref _meshUtility.FreezeRotation);
            return forceUniqueName || blendShape || scale || rotation;
        }

        bool MeshIntegrateGui()
        {
            var firstPerson = ToggleIsModified("FirstPerson == AUTO の生成", ref _meshUtility.GenerateMeshForFirstPersonAuto);
            var split = ToggleIsModified("Separate by BlendShape", ref _meshUtility.SplitByBlendShape);
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
            var mod = _meshIntegration.OnGui(rect);
            return firstPerson || split || mod;
        }
    }
}