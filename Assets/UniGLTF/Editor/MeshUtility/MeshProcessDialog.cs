using UnityEngine;
using UnityEditor;
using UniGLTF.M17N;
using System.Collections.Generic;

namespace UniGLTF.MeshUtility
{
    public class MeshProcessDialog : EditorWindow
    {
        const string TITLE = "Mesh Processing Window";
        public MeshProcessDialogTabs Tab;

        private GameObject _exportTarget;

        public BoneMeshRemoverValidator _boneMeshRemoverValidator = new BoneMeshRemoverValidator();

        [SerializeField]
        public bool _separateByBlendShape = true;

        [SerializeField]
        public SkinnedMeshRenderer _skinnedMeshRenderer = null;

        [SerializeField]
        public List<BoneMeshEraser.EraseBone> _eraseBones;

        private MeshProcessDialogEditor _boneMeshEraserEditor;
        private Vector2 _scrollPos = new Vector2(0, 0);

        public static void OpenWindow()
        {
            var window =
                (MeshProcessDialog)EditorWindow.GetWindow(typeof(MeshProcessDialog));
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

        private void OnEnable()
        {
            if (!_boneMeshEraserEditor)
            {
                _boneMeshEraserEditor = (MeshProcessDialogEditor)Editor.CreateEditor(this);
            }
        }

        static bool IsGameObjectSelected()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUIUtility.labelWidth = 300;
            // lang
            LanguageGetter.OnGuiSelectLang();

            {
                // _exportTarget
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(MeshProcessingMessages.TARGET_OBJECT.Msg(), GUILayout.MaxWidth(146.0f));
                _exportTarget = (GameObject)EditorGUILayout.ObjectField(_exportTarget, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
                // auto select ?
                if (_exportTarget == null && IsGameObjectSelected())
                {
                    _exportTarget = Selection.activeObject as GameObject;
                }
            }

            // tab
            Tab = TabBar.OnGUI(Tab, "LargeButton", GUI.ToolbarButtonSize.Fixed);
            var processed = false;
            switch (Tab)
            {
                case MeshProcessDialogTabs.MeshSeparator:
                    {
                        EditorGUILayout.HelpBox(MeshProcessingMessages.MESH_SEPARATOR.Msg(), MessageType.Info);
                        processed = TabMeshSeparator.OnGUI(_exportTarget);
                        break;
                    }

                case MeshProcessDialogTabs.MeshIntegrator:
                    {
                        EditorGUILayout.HelpBox(MeshProcessingMessages.MESH_INTEGRATOR.Msg(), MessageType.Info);
                        _separateByBlendShape = EditorGUILayout.Toggle(MeshProcessingMessages.MESH_SEPARATOR_BY_BLENDSHAPE.Msg(), _separateByBlendShape);
                        processed = TabMeshIntegrator.OnGUI(_exportTarget, _separateByBlendShape);
                        break;
                    }

                case MeshProcessDialogTabs.BoneMeshEraser:
                    {
                        EditorGUILayout.HelpBox(MeshProcessingMessages.BONE_MESH_ERASER.Msg(), MessageType.Info);
                        if (_boneMeshEraserEditor)
                        {
                            _boneMeshEraserEditor.OnInspectorGUI();
                        }
                        _boneMeshRemoverValidator.Validate(_skinnedMeshRenderer, _eraseBones);
                        processed = TabBoneMeshRemover.OnGUI(_exportTarget, _skinnedMeshRenderer, _eraseBones);
                        break;
                    }
            }
            EditorGUILayout.EndScrollView();

            if (processed)
            {
                Close();
                GUIUtility.ExitGUI();
            }
        }
    }
}