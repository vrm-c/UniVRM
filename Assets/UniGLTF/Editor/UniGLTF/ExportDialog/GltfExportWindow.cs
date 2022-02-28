using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class GltfExportWindow : ExportDialogBase
    {


        enum Tabs
        {
            Mesh,
            ExportSettings,
        }
        Tabs _tab;

        [SerializeField]
        public GltfExportSettings Settings = new GltfExportSettings();

        SerializedPropertyEditor m_editor;

        MeshExportValidator m_meshes;
        Editor m_meshesInspector;

        protected override void Initialize()
        {
            Settings.InverseAxis = UniGLTFPreference.GltfIOAxis;
            var so = new SerializedObject(this);
            m_editor = new SerializedPropertyEditor(so, so.FindProperty(nameof(Settings)));

            m_meshes = ScriptableObject.CreateInstance<MeshExportValidator>();
            m_meshesInspector = Editor.CreateEditor(m_meshes);
        }

        protected override void Clear()
        {
            // m_meshesInspector
            UnityEditor.Editor.DestroyImmediate(m_meshesInspector);
            m_meshesInspector = null;
        }

        protected override IEnumerable<Validator> ValidatorFactory()
        {
            yield return HierarchyValidator.ValidateRoot;
            yield return AnimationValidator.Validate;
            if (!State.ExportRoot)
            {
                yield break;
            }

            // Mesh/Renderer のチェック
            yield return m_meshes.Validate;
        }

        protected override void OnLayout()
        {
            m_meshes.SetRoot(State.ExportRoot, Settings, new DefualtBlendShapeExportFilter());
        }

        protected override bool DoGUI(bool isValid)
        {
            if (!isValid)
            {
                return false;
            }

            // tabbar
            _tab = MeshUtility.TabBar.OnGUI(_tab);
            switch (_tab)
            {
                case Tabs.Mesh:
                    m_meshesInspector.OnInspectorGUI();
                    break;

                case Tabs.ExportSettings:
                    m_editor.OnInspectorGUI();
                    break;
            }

            return true;
        }

        protected override string SaveTitle => "Save gltf";
        protected override string SaveName => $"{State.ExportRoot.name}.glb";
        protected override string[] SaveExtensions => new string[] { "glb", "gltf" };

        protected override void ExportPath(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            var isGlb = false;
            switch (ext)
            {
                case ".glb": isGlb = true; break;
                case ".gltf": isGlb = false; break;
                default: throw new System.Exception();
            }

            var progress = 0;
            EditorUtility.DisplayProgressBar("export gltf", path, progress);
            try
            {
                var data = new ExportingGltfData();
                using (var exporter = new gltfExporter(data, Settings,
                    progress: new EditorProgress(),
                    animationExporter: new EditorAnimationExporter()))
                {
                    exporter.Prepare(State.ExportRoot);
                    exporter.Export(new EditorTextureSerializer());
                }

                if (isGlb)
                {
                    var bytes = data.ToGlbBytes();
                    File.WriteAllBytes(path, bytes);
                }
                else
                {
                    var (json, buffer0) = data.ToGltf(path);

                    {
                        // write JSON without BOM
                        var encoding = new System.Text.UTF8Encoding(false);
                        File.WriteAllText(path, json, encoding);
                    }

                    {
                        // write to buffer0 local folder
                        var dir = Path.GetDirectoryName(path);
                        var bufferPath = Path.Combine(dir, buffer0.uri);
                        File.WriteAllBytes(bufferPath, data.BinBytes.ToArray());
                    }
                }

                if (path.StartsWithUnityAssetPath())
                {
                    AssetDatabase.ImportAsset(path.ToUnityRelativePath());
                    AssetDatabase.Refresh();
                }

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
