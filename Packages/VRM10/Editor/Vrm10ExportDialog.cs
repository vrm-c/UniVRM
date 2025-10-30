using System;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UniGLTF.M17N;
using UniGLTF.MeshUtility;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public class VRM10ExportDialog : ExportDialogBase
    {
        public const string MENU_NAME = "Export VRM 1.0...";

        public static void Open()
        {
            var window = GetWindow<VRM10ExportDialog>(MENU_NAME);
            window.Show();
        }


        enum Tabs
        {
            Meta,
            Mesh,
            ExportSettings,
        }
        Tabs _tab;


        VRM10ExportSettings m_settings;
        Editor m_settingsInspector;


        MeshExportValidator m_meshes;
        Editor m_meshesInspector;

        VRM10Object m_meta;
        VRM10Object Vrm
        {
            get { return m_meta; }
            set
            {
                if (value != null && AssetDatabase.IsSubAsset(value))
                {
                    // SubAsset is readonly. copy
                    UniGLTFLogger.Log("copy VRM10ObjectMeta");
                    value.Meta.CopyTo(m_tmpObject.Meta);
                    return;
                }

                if (m_meta == value)
                {
                    return;
                }
                m_metaEditor = default;
                m_meta = value;
            }
        }
        VRM10Object m_tmpObject;
        VRM10MetaEditor m_metaEditor;

        protected override void Initialize()
        {
            m_tmpObject = ScriptableObject.CreateInstance<VRM10Object>();
            m_tmpObject.name = "_vrm1_";
            m_tmpObject.Meta.Authors = new List<string> { "" };

            m_settings = ScriptableObject.CreateInstance<VRM10ExportSettings>();
            m_settingsInspector = Editor.CreateEditor(m_settings);

            m_meshes = ScriptableObject.CreateInstance<MeshExportValidator>();
            m_meshesInspector = Editor.CreateEditor(m_meshes);

            State.ExportRootChanged += (root) =>
            {
                // update meta
                if (root == null)
                {
                    Vrm = null;
                }
                else
                {
                    if (root.TryGetComponent<Vrm10Instance>(out var controller))
                    {
                        Vrm = controller.Vrm;
                    }
                    else
                    {
                        Vrm = null;
                    }

                    // default setting
                    // m_settings.PoseFreeze =
                    // MeshUtility.Validators.HumanoidValidator.HasRotationOrScale(root)
                    // || m_meshes.Meshes.Any(x => x.ExportBlendShapeCount > 0 && !x.HasSkinning)
                    // ;
                }
            };
        }

        protected override void Clear()
        {
            // m_settingsInspector
            UnityEditor.Editor.DestroyImmediate(m_settingsInspector);
            m_settingsInspector = null;
            // m_meshesInspector
            UnityEditor.Editor.DestroyImmediate(m_meshesInspector);
            m_meshesInspector = null;
            // Meta
            Vrm = null;
            ScriptableObject.DestroyImmediate(m_tmpObject);
            m_tmpObject = null;
            // m_settings
            ScriptableObject.DestroyImmediate(m_settings);
            m_settings = null;
            // m_meshes
            ScriptableObject.DestroyImmediate(m_meshes);
            m_meshes = null;
        }

        protected override IEnumerable<Validator> ValidatorFactory()
        {
            HumanoidValidator.MeshInformations = m_meshes.Meshes;
            // HumanoidValidator.EnableFreeze = m_settings.PoseFreeze;

            yield return HierarchyValidator.Validate;
            if (!State.ExportRoot)
            {
                yield break;
            }

            // Mesh/Renderer のチェック
            m_meshes.MaterialValidator = new VRM10MaterialValidator();
            yield return m_meshes.Validate;

            yield return HumanoidValidator.Validate_TPose;

            // MeshUtility.Validators.HumanoidValidator.EnableFreeze = false;
            // yield return MeshUtility.Validators.HumanoidValidator.Validate;

            // yield return VRMExporterValidator.Validate;
            // yield return VRMSpringBoneValidator.Validate;

            // var firstPerson = State.ExportRoot.GetComponent<VRMFirstPerson>();
            // if (firstPerson != null)
            // {
            //     yield return firstPerson.Validate;
            // }

            // var proxy = State.ExportRoot.GetComponent<VRMBlendShapeProxy>();
            // if (proxy != null)
            // {
            //     yield return proxy.Validate;
            // }

            var vrm = Vrm ? Vrm : m_tmpObject;
            yield return vrm.Meta.Validate;
        }

        protected override void OnLayout()
        {
            // m_settings, m_meshes.Meshes
            m_meshes.SetRoot(State.ExportRoot, m_settings.MeshExportSettings, new DefualtBlendShapeExportFilter());
        }

        protected override bool DoGUI(bool isValid)
        {
            if (State.ExportRoot == null)
            {
                return false;
            }

            if (State.ExportRoot.TryGetComponent<Animator>(out var animator))
            {
                var backup = GUI.enabled;
                GUI.enabled = State.ExportRoot.scene.IsValid();
                if (GUI.enabled)
                {
                    EditorGUILayout.HelpBox(EnableTPose.ENALBE_TPOSE_BUTTON.Msg(), MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(EnableTPose.DISABLE_TPOSE_BUTTON.Msg(), MessageType.Warning);
                }

                if (GUILayout.Button("T-Pose" + "(unity internal)"))
                {
                    if (State.ExportRoot != null)
                    {
                        Undo.RecordObjects(State.ExportRoot.GetComponentsInChildren<Transform>(), "tpose.internal");
                        if (InternalTPose.TryMakePoseValid(State.ExportRoot))
                        {
                            // done
                            Repaint();
                        }
                        else
                        {
                            UniGLTFLogger.Warning("not found");
                        }
                    }
                }
                EditorGUILayout.Separator();
                GUI.enabled = backup;
            }

            if (!isValid)
            {
                return false;
            }

            if (m_tmpObject == null)
            {
                // disabled
                return false;
            }

            // tabbar
            _tab = TabBar.OnGUI(_tab);
            switch (_tab)
            {
                case Tabs.Meta:
                    if (m_metaEditor == null)
                    {
                        SerializedObject so;
                        if (m_meta != null)
                        {
                            so = new SerializedObject(Vrm);
                        }
                        else
                        {
                            so = new SerializedObject(m_tmpObject);
                        }
                        m_metaEditor = VRM10MetaEditor.Create(so);
                    }
                    m_metaEditor.OnInspectorGUI();
                    break;

                case Tabs.Mesh:
                    m_meshesInspector.OnInspectorGUI();
                    break;

                case Tabs.ExportSettings:
                    m_settingsInspector.OnInspectorGUI();
                    break;
            }

            return true;
        }

        protected override string SaveTitle => "Vrm1";
        protected override string SaveName => $"{State.ExportRoot.name}.vrm";
        protected override string[] SaveExtensions => new string[] { "vrm" };

        string m_logLabel;

        class TmpDisposer : IDisposable
        {
            List<UnityEngine.Object> _disposables = new();
            public void Push(UnityEngine.Object o)
            {
                _disposables.Add(o);
            }

            public void Dispose()
            {
                foreach (var o in _disposables)
                {
                    GameObject.DestroyImmediate(o);
                }
                _disposables.Clear();
            }
        }

        protected override void ExportPath(string path)
        {
            m_logLabel = "";

            m_logLabel += $"export...\n";

            var root = State.ExportRoot;

            try
            {
                using (var disposer = new TmpDisposer())
                using (var arrayManager = new NativeArrayManager())
                {
                    if (m_settings.FreezeMesh)
                    {
                        UniGLTFLogger.Log("vrm-1.0 FreezeMesh");
                        var copy = GameObject.Instantiate(root);
                        copy.GetComponent<Vrm10Instance>().UpdateType = Vrm10Instance.UpdateTypes.None;
                        disposer.Push(copy);
                        root = copy;

                        using (var backup = new Vrm10GeometryBackup(root))
                        {
                            // Transform の回転とスケールを Mesh に適用します。
                            // - BlendShape は現状がbakeされます
                            // - 回転とスケールが反映された新しい Mesh が作成されます
                            // - Transform の回転とスケールはクリアされます。world position を維持します
                            var newMeshMap = BoneNormalizer.NormalizeHierarchyFreezeMesh(root, m_settings.FreezeMeshUseCurrentBlendShapeWeight);

                            // SkinnedMeshRenderer.sharedMesh と MeshFilter.sharedMesh を新しいMeshで置き換える
                            BoneNormalizer.Replace(root, newMeshMap, m_settings.FreezeMeshKeepRotation);
                        }
                    }

                    var converter = new UniVRM10.ModelExporter();
                    var model = converter.Export(m_settings.MeshExportSettings, arrayManager, root);

                    // 右手系に変換
                    m_logLabel += $"convert to right handed coordinate...\n";
                    model.ConvertCoordinate(VrmLib.Coordinates.Vrm1, ignoreVrm: false);

                    // export vrm-1.0
                    var exporter = new Vrm10Exporter(
                        m_settings.MeshExportSettings,
                        textureSerializer: new EditorTextureSerializer()
                    );
                    var option = new VrmLib.ExportArgs
                    {
                        sparse = m_settings.MorphTargetUseSparse,
                    };
                    exporter.Export(root, model, converter, option, Vrm ? Vrm.Meta : m_tmpObject.Meta);

                    var exportedBytes = exporter.Storage.ToGlbBytes();

                    m_logLabel += $"write to {path}...\n";
                    File.WriteAllBytes(path, exportedBytes);
                    UniGLTFLogger.Log("exportedBytes: " + exportedBytes.Length);

                    var assetPath = UniGLTF.UnityPath.FromFullpath(path);
                    if (assetPath.IsUnderWritableFolder)
                    {
                        // asset folder 内。import を発動
                        assetPath.ImportAsset();
                    }
                }
            }
            catch (Exception ex)
            {
                m_logLabel += ex.ToString();
                // rethrow
                //throw;
                UniGLTFLogger.Exception(ex);
            }
        }
    }
}
