using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UniGLTF;
using UniGLTF.M17N;

namespace UniGLTF.MeshUtility
{
    public class MeshProcessDialog : EditorWindow
    {
        const string TITLE = "Mesh Processing Window";
        public MeshProcessDialogTabs Tab;

        private GameObject _exportTarget;

        private MeshProcessDialogEditor _boneMeshEraserEditor;

        private SkinnedMeshRenderer _pSkinnedMesh;
        private Animator _pAnimator;
        private Transform _pEraseRoot;
        private Vector2 _scrollPos = new Vector2(0, 0);

        [SerializeField]
        public SkinnedMeshRenderer _skinnedMesh = null;

        [SerializeField]
        public bool _separateByBlendShape = true;

        private Animator _cAnimator = null;
        private Transform _cEraseRoot = null;

        [SerializeField]
        public BoneMeshEraser.EraseBone[] _eraseBones;

        private MethodInfo _processFunction;

        GUIStyle _tabButtonStyle => "LargeButton";
        GUI.ToolbarButtonSize _tabButtonSize => GUI.ToolbarButtonSize.Fixed;


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
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(MeshProcessingMessages.TARGET_OBJECT.Msg(), GUILayout.MaxWidth(146.0f));
                _exportTarget = (GameObject)EditorGUILayout.ObjectField(_exportTarget, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
                if (_exportTarget == null && IsGameObjectSelected())
                {
                    _exportTarget = Selection.activeObject as GameObject;
                }
            }

            // tab
            Tab = TabBar.OnGUI(Tab, _tabButtonStyle, _tabButtonSize);
            var processed = false;
            switch (Tab)
            {
                case MeshProcessDialogTabs.MeshSeparator:
                    EditorGUILayout.HelpBox(MeshProcessingMessages.MESH_SEPARATOR.Msg(), MessageType.Info);
                    processed = TabMeshSeparator.OnGUI(_exportTarget);
                    break;

                case MeshProcessDialogTabs.MeshIntegrator:
                    EditorGUILayout.HelpBox(MeshProcessingMessages.MESH_INTEGRATOR.Msg(), MessageType.Info);
                    if (_boneMeshEraserEditor)
                    {
                        _boneMeshEraserEditor.OnInspectorGUI();
                    }
                    processed = TabMeshIntegrator.OnGUI(_exportTarget, _separateByBlendShape);
                    break;

                case MeshProcessDialogTabs.BoneMeshEraser:
                    EditorGUILayout.HelpBox(MeshProcessingMessages.BONE_MESH_ERASER.Msg(), MessageType.Info);
                    if (_boneMeshEraserEditor)
                    {
                        _boneMeshEraserEditor.OnInspectorGUI();
                    }
                    // any better way we can detect component change?
                    if (_skinnedMesh != _pSkinnedMesh || _cAnimator != _pAnimator || _cEraseRoot != _pEraseRoot)
                    {
                        BoneMeshEraserValidate();
                    }
                    _pSkinnedMesh = _skinnedMesh;
                    _pAnimator = _cAnimator;
                    _pEraseRoot = _cEraseRoot;
                    processed = TabBoneMeshRemover.OnGUI(_exportTarget, _skinnedMesh, _eraseBones);
                    break;
            }
            EditorGUILayout.EndScrollView();

            if (processed)
            {
                Close();
                GUIUtility.ExitGUI();
            }
        }


        private void BoneMeshEraserValidate()
        {
            if (_skinnedMesh == null)
            {
                _eraseBones = new BoneMeshEraser.EraseBone[] { };
                return;
            }

            if (_cEraseRoot == null)
            {
                if (_cAnimator != null)
                {
                    _cEraseRoot = _cAnimator.GetBoneTransform(HumanBodyBones.Head);
                    //Debug.LogFormat("head: {0}", EraseRoot);
                }
            }

            _eraseBones = _skinnedMesh.bones.Select(x =>
            {
                var eb = new BoneMeshEraser.EraseBone
                {
                    Bone = x,
                };

                if (_cEraseRoot != null)
                {
                    // 首の子孫を消去
                    if (eb.Bone.Ancestor().Any(y => y == _cEraseRoot))
                    {
                        //Debug.LogFormat("erase {0}", x);
                        eb.Erase = true;
                    }
                }

                return eb;
            })
            .ToArray();
        }
    }
}