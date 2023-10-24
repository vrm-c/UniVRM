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
            MeshFreeze,
            MeshIntegration,
        }
        Tabs _tab;

        public static void OpenWindow()
        {
            var window =
                (Vrm10MeshUtilityDialog)EditorWindow.GetWindow(typeof(Vrm10MeshUtilityDialog));
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

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
        List<Validation> _validations = new List<Validation>();
        GameObject _exportTarget;

        MeshIntegrationAndSplit _meshIntegration;

        void OnEnable()
        {
            _meshIntegration = new MeshIntegrationAndSplit(this);
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
                case Tabs.MeshFreeze:
                    {
                        if (MeshBakeGui())
                        {
                            modified = true;
                        }
                        break;
                    }

                case Tabs.MeshIntegration:
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
                // Show Result ?
                // Close();
                // GUIUtility.ExitGUI();
                Debug.Log("Process !");
            }
        }

        bool MeshBakeGui()
        {
            EditorGUILayout.Toggle("BlendShape", false);
            EditorGUILayout.Toggle("Scale", false);
            EditorGUILayout.Toggle("Rotation", false);
            return default;
        }

        bool MeshIntegrateGui()
        {
            EditorGUILayout.Toggle("FirstPerson == AUTO の生成", false);
            EditorGUILayout.Toggle("Separate by BlendShape", false);
            var p = position;
            var last = GUILayoutUtility.GetLastRect();
            var y = last.y + last.height;
            var rect = new Rect
            {
                x = last.x,
                y = y,
                width = p.width,
                height = p.height - y - 30
            };
            _meshIntegration.OnGui(rect);
            return default;
        }
    }
}