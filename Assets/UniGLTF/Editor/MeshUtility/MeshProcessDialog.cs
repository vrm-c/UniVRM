using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UniGLTF;
using UniGLTF.M17N;

namespace UniGLTF.MeshUtility
{
    [CustomEditor(typeof(MeshProcessDialog), true)]
    public class BoneMeshEraserGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var skinnedMesh = serializedObject.FindProperty("_cSkinnedMesh");
            EditorGUILayout.PropertyField(skinnedMesh, new GUIContent("Skinned Mesh"), true);
            var animator = serializedObject.FindProperty("_cAnimator");
            EditorGUILayout.PropertyField(animator, new GUIContent("Animator"), false);
            var eraseRoot = serializedObject.FindProperty("_cEraseRoot");
            EditorGUILayout.PropertyField(eraseRoot, new GUIContent("Erase Root"), false);
            var list = serializedObject.FindProperty("_eraseBones");
            EditorGUILayout.PropertyField(list, new GUIContent("Erase Bones"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }

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
        private bool _isInvokeSuccess = false;

        GUIStyle _tabButtonStyle => "LargeButton";
        GUI.ToolbarButtonSize _tabButtonSize => GUI.ToolbarButtonSize.Fixed;

        private enum MeshProcessingMessages
        {
            [LangMsg(Languages.ja, "ターゲットオブジェクト")]
            [LangMsg(Languages.en, "TargetObject")]
            TARGET_OBJECT,

            [LangMsg(Languages.ja, "BlendShapeを含むメッシュは分割されます")]
            [LangMsg(Languages.en, "Meshes containing BlendShape will be split")]
            MESH_SEPARATOR,

            [LangMsg(Languages.ja, "メッシュを統合します。BlendShapeを含むメッシュは独立して統合されます")]
            [LangMsg(Languages.en, "Generate a single mesh. Meshes w/ BlendShape will be grouped into another one")]
            MESH_INTEGRATOR,

            [LangMsg(Languages.ja, "静的メッシュを一つに統合します")]
            [LangMsg(Languages.en, "Integrate static meshes into one")]
            STATIC_MESH_INTEGRATOR,

            [LangMsg(Languages.ja, "ボーン(Erase Rootのヒエラルキー)に関連するメッシュを削除します")]
            [LangMsg(Languages.en, "Eliminate meshes associated with the bones in EraseRoot hierarchy")]
            BONE_MESH_ERASER,

            [LangMsg(Languages.ja, "Skinned Meshを選んでください")]
            [LangMsg(Languages.en, "Select a skinned mesh")]
            SELECT_SKINNED_MESH,

            [LangMsg(Languages.ja, "Erase Rootを選んでください")]
            [LangMsg(Languages.en, "Select a erase root")]
            SELECT_ERASE_ROOT,

            [LangMsg(Languages.ja, "GameObjectを選んでください")]
            [LangMsg(Languages.en, "Select a GameObject first")]
            NO_GAMEOBJECT_SELECTED,

            [LangMsg(Languages.ja, "GameObjectにスキンメッシュが含まれていません")]
            [LangMsg(Languages.en, "No skinned mesh is contained")]
            NO_SKINNED_MESH,

            [LangMsg(Languages.ja, "GameObjectに静的メッシュが含まれていません")]
            [LangMsg(Languages.en, "No static mesh is contained")]
            NO_STATIC_MESH,

            [LangMsg(Languages.ja, "GameObjectにスキンメッシュ・静的メッシュが含まれていません")]
            [LangMsg(Languages.en, "Skinned/Static mesh is not contained")]
            NO_MESH,

            [LangMsg(Languages.ja, "ターゲットオブジェクトはVRMモデルです。`VRM0-> MeshIntegrator`を使ってください")]
            [LangMsg(Languages.en, "Target object is VRM model, use `VRM0 -> MeshIntegrator` instead")]
            VRM_DETECTED,
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

            switch (_tab)
            {
                case Tabs.MeshSeparator:
                    EditorGUILayout.LabelField(MeshProcessingMessages.MESH_SEPARATOR.Msg());
                    break;
                case Tabs.MeshIntegrator:
                    EditorGUILayout.LabelField(MeshProcessingMessages.MESH_INTEGRATOR.Msg());
                    break;
                case Tabs.StaticMeshIntegrator:
                    EditorGUILayout.LabelField(MeshProcessingMessages.STATIC_MESH_INTEGRATOR.Msg());
                    break;
                case Tabs.BoneMeshEraser:
                    EditorGUILayout.LabelField(MeshProcessingMessages.BONE_MESH_ERASER.Msg());
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MeshProcessingMessages.TARGET_OBJECT.Msg(), GUILayout.MaxWidth(146.0f));
            _exportTarget = (GameObject)EditorGUILayout.ObjectField(_exportTarget, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
            if (_exportTarget == null && MeshUtility.IsGameObjectSelected())
            {
                _exportTarget = Selection.activeObject as GameObject;
            }

            if (_tab == Tabs.BoneMeshEraser)
            {
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
            }

            // Create Other Buttons
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                    {
                        switch (_tab)
                        {
                            case Tabs.MeshSeparator:
                                _isInvokeSuccess = InvokeWizardUpdate("MeshSeparator");
                                break;
                            case Tabs.MeshIntegrator:
                                _isInvokeSuccess = InvokeWizardUpdate("MeshIntegrator");
                                break;
                            case Tabs.StaticMeshIntegrator:
                                _isInvokeSuccess = InvokeWizardUpdate("StaticMeshIntegrator");
                                break;
                            case Tabs.BoneMeshEraser:
                                _isInvokeSuccess = InvokeWizardUpdate("BoneMeshRemover");
                                break;
                        }
                        if (_isInvokeSuccess)
                        {
                            Close();
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private bool InvokeWizardUpdate(string processFuntion)
        {
            const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            _processFunction = GetType().GetMethod(processFuntion, kInstanceInvokeFlags);
            if (_processFunction != null)
            {
                return (Boolean)_processFunction.Invoke(this, null);
            }
            else
            {
                Debug.LogError("This function has not been implemented in script");
                return false;
            }
        }

        private bool GameObjectNull()
        {
            EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
            return false;
        }

        private bool MeshSeparator()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;

            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0)
            {
                MeshUtility.SeparationProcessing(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_SKINNED_MESH.Msg(), "ok");
                return false;
            }
        }

        private bool MeshIntegrator()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;

            Component[] allComponents = go.GetComponents(typeof(Component));
            var keyWord = "VRMMeta";

            foreach (var component in allComponents)
            {
                if (component == null) continue;
                var sourceString = component.ToString();
                if (sourceString.Contains(keyWord))
                {
                    EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.VRM_DETECTED.Msg(), "ok");
                    return false;
                }
            }

            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0 || go.GetComponentsInChildren<MeshFilter>().Length > 0)
            {
                MeshUtility.MeshIntegrator(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_MESH.Msg(), "ok");
                return false;
            }
        }

        private bool StaticMeshIntegrator()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;
            if (go.GetComponentsInChildren<MeshFilter>().Length > 0)
            {
                MeshUtility.IntegrateSelected(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_STATIC_MESH.Msg(), "ok");
                return false;
            }
        }

        private bool BoneMeshRemover()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;

            if (_cSkinnedMesh == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.SELECT_SKINNED_MESH.Msg(), "ok");
                return false;
            }
            else if (_cEraseRoot == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.SELECT_ERASE_ROOT.Msg(), "ok");
                return false;
            }
            BoneMeshRemove(go);

            return true;
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

        private void BoneMeshRemove(GameObject go)
        {
            var renderer = Remove(go);
            var outputObject = GameObject.Instantiate(go);
            outputObject.name = outputObject.name + "_bone_mesh_erase";
            if (renderer == null)
            {
                return;
            }

            // save mesh to Assets
            var assetPath = string.Format("{0}{1}", go.name, MeshUtility.ASSET_SUFFIX);
            var prefab = MeshUtility.GetPrefab(go);
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}{2}",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    MeshUtility.ASSET_SUFFIX
                    );
            }

            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(renderer.sharedMesh, assetPath);

            // destroy BoneMeshEraser in the source
            foreach (var skinnedMesh in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.gameObject.name == BoneMeshEraserWizard.BONE_MESH_ERASER_NAME)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
            }
            // destroy the original mesh in the copied GameObject
            foreach (var skinnedMesh in outputObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.sharedMesh == _cSkinnedMesh.sharedMesh)
                {
                    GameObject.DestroyImmediate(skinnedMesh);
                }
            }
        }

        private SkinnedMeshRenderer Remove(GameObject go)
        {
            var bones = _cSkinnedMesh.bones;
            var eraseBones = _eraseBones
                .Where(x => x.Erase)
                .Select(x => Array.IndexOf(bones, x.Bone))
                .ToArray();

            var meshNode = new GameObject(BoneMeshEraserWizard.BONE_MESH_ERASER_NAME);
            meshNode.transform.SetParent(go.transform, false);

            var erased = meshNode.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = BoneMeshEraser.CreateErasedMesh(_cSkinnedMesh.sharedMesh, eraseBones);
            erased.sharedMaterials = _cSkinnedMesh.sharedMaterials;
            erased.bones = _cSkinnedMesh.bones;

            return erased;
        }
    }
}