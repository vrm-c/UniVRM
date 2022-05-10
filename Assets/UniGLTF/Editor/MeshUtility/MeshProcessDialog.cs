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
        enum Tabs
        {
            MeshSeparator,
            MeshIntegrator,
            StaticMeshIntegrator,
            BoneMeshEraser,
        }
        private Tabs _tab;

        private GameObject _exportTarget;
        private Editor _boneMeshEraserEditor;
        private SkinnedMeshRenderer _pSkinnedMesh;
        private Animator _pAnimator;
        private Transform _pEraseRoot;
        private Vector2 _scrollPos = new Vector2(0, 0);

        [SerializeField]
        private SkinnedMeshRenderer _cSkinnedMesh = null;
        [SerializeField]
        private Animator _cAnimator = null;
        [SerializeField]
        private Transform _cEraseRoot = null;
        [SerializeField]
        private BoneMeshEraser.EraseBone[] _eraseBones;

        private MethodInfo _processFunction;

        GUIStyle _tabButtonStyle => "LargeButton";
        GUI.ToolbarButtonSize _tabButtonSize => GUI.ToolbarButtonSize.Fixed;


        public static void OpenWindow()
        {
            var window =
                (MeshProcessDialog)EditorWindow.GetWindowWithRect(typeof(MeshProcessDialog),
                    new Rect(0, 0, 650, 500));
            window.titleContent = new GUIContent("Mesh Processing Window");
            window.Show();
        }

        private void OnEnable()
        {
            if (!_boneMeshEraserEditor)
            {
                _boneMeshEraserEditor = Editor.CreateEditor(this);
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUIUtility.labelWidth = 150;
            // lang
            LanguageGetter.OnGuiSelectLang();

            _tab = TabBar.OnGUI(_tab, _tabButtonStyle, _tabButtonSize);
            var processed = false;
            switch (_tab)
            {
                case Tabs.MeshSeparator:
                    EditorGUILayout.LabelField(MeshProcessingMessages.MESH_SEPARATOR.Msg());
                    OnGUI_ExportTarget();
                    processed = TabMeshSeparator.OnGUI(_exportTarget);
                    break;
                case Tabs.MeshIntegrator:
                    EditorGUILayout.LabelField(MeshProcessingMessages.MESH_INTEGRATOR.Msg());
                    OnGUI_ExportTarget();
                    processed = TabMeshIntegrator.OnGUI(_exportTarget);
                    break;
                case Tabs.StaticMeshIntegrator:
                    EditorGUILayout.LabelField(MeshProcessingMessages.STATIC_MESH_INTEGRATOR.Msg());
                    OnGUI_ExportTarget();
                    processed = TabStaticMeshIntegrator.OnGUI(_exportTarget);
                    break;
                case Tabs.BoneMeshEraser:
                    EditorGUILayout.LabelField(MeshProcessingMessages.BONE_MESH_ERASER.Msg());
                    OnGUI_ExportTarget();
                    if (_boneMeshEraserEditor)
                    {
                        _boneMeshEraserEditor.OnInspectorGUI();
                    }
                    // any better way we can detect component change?
                    if (_cSkinnedMesh != _pSkinnedMesh || _cAnimator != _pAnimator || _cEraseRoot != _pEraseRoot)
                    {
                        BoneMeshEraserValidate();
                    }
                    _pSkinnedMesh = _cSkinnedMesh;
                    _pAnimator = _cAnimator;
                    _pEraseRoot = _cEraseRoot;
                    processed = TabBoneMeshRemover.OnGUI(_exportTarget, _cSkinnedMesh, _cEraseRoot, _eraseBones);
                    break;
            }
            EditorGUILayout.EndScrollView();

            if (processed)
            {
                Close();
                GUIUtility.ExitGUI();
            }
        }

        private void OnGUI_ExportTarget()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MeshProcessingMessages.TARGET_OBJECT.Msg(), GUILayout.MaxWidth(146.0f));
            _exportTarget = (GameObject)EditorGUILayout.ObjectField(_exportTarget, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
            if (_exportTarget == null && MeshUtility.IsGameObjectSelected())
            {
                _exportTarget = Selection.activeObject as GameObject;
            }
        }

        private void BoneMeshEraserValidate()
        {
            if (_cSkinnedMesh == null)
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

            _eraseBones = _cSkinnedMesh.bones.Select(x =>
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