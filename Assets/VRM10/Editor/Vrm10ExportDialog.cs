using System;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using VrmLib;
using VRMShaders;

namespace UniVRM10
{
    public class VRM10ExportDialog : ExportDialogBase
    {
        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export VRM-1.0";

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var window = (VRM10ExportDialog)GetWindow(typeof(VRM10ExportDialog));
            window.titleContent = new GUIContent("VRM-1.0 Exporter");
            window.Show();
        }

        enum Tabs
        {
            Meta,
            Mesh,
            ExportSettings,
        }
        Tabs _tab;

        VRM10MetaObject m_meta;
        VRM10MetaObject Meta
        {
            get { return m_meta; }
            set
            {
                if (value != null && AssetDatabase.IsSubAsset(value))
                {
                    Debug.Log("copy VRM10MetaObject");
                    value.CopyTo(m_tmpMeta);
                    return;
                }

                if (m_meta == value)
                {
                    return;
                }
                if (m_metaEditor != null)
                {
                    UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                    m_metaEditor = null;
                }
                m_meta = value;
            }
        }

        protected override string SaveTitle => "Vrm1";

        protected override string SaveName => $"{State.ExportRoot.name}.vrm";
        protected override string[] SaveExtensions => new string[] { "vrm" };


        VRM10MetaObject m_tmpMeta;

        Editor m_metaEditor;

        protected override void Initialize()
        {
            m_tmpMeta = ScriptableObject.CreateInstance<VRM10MetaObject>();
            m_tmpMeta.Authors = new List<string> { "" };

            State.ExportRootChanged += (root) =>
            {
                // update meta
                if (root == null)
                {
                    Meta = null;
                }
                else
                {
                    var controller = root.GetComponent<VRM10Controller>();
                    if (controller != null)
                    {
                        Meta = controller.Meta;
                    }
                    else
                    {
                        Meta = null;
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
            ScriptableObject.DestroyImmediate(m_tmpMeta);
            m_tmpMeta = null;
        }

        protected override IEnumerable<Validator> ValidatorFactory()
        {
            yield return HierarchyValidator.Validate;
            if (!State.ExportRoot)
            {
                yield break;
            }

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

            var meta = Meta ? Meta : m_tmpMeta;
            yield return meta.Validate;
        }

        // private void OnGUI()
        // {
        //             {
        //                 var path = SaveFileDialog.GetPath("Save vrm1", $"{State.ExportRoot.name}.vrm", "vrm");
        //                 if (!string.IsNullOrEmpty(path))
        //                 {
        //                     // export
        //                     Export(State.ExportRoot, path);
        //                     // close
        //                     Close();
        //                     GUIUtility.ExitGUI();
        //                 }
        //             }
        //             GUI.enabled = true;

        //             GUILayout.EndHorizontal();
        //         }
        //         GUILayout.EndVertical();
        //     }

        //     GUILayout.Space(8);

        //     if (modified)
        //     {
        //         State.Invalidate();
        //     }
        // }

        protected override bool DoGUI()
        {
            if (m_tmpMeta == null)
            {
                // disabled
                return false;
            }

            // tabbar
            _tab = MeshUtility.TabBar.OnGUI(_tab);
            switch (_tab)
            {
                case Tabs.Meta:
                    if (m_metaEditor == null)
                    {
                        if (m_meta != null)
                        {
                            m_metaEditor = Editor.CreateEditor(Meta);
                        }
                        else
                        {
                            m_metaEditor = Editor.CreateEditor(m_tmpMeta);
                        }
                    }
                    m_metaEditor.OnInspectorGUI();
                    break;

                case Tabs.ExportSettings:
                    // m_settingsInspector.OnInspectorGUI();
                    break;

                case Tabs.Mesh:
                    // m_meshesInspector.OnInspectorGUI();
                    break;
            }

            return true;
        }

        string m_logLabel;

        protected override void ExportPath(string path)
        {
            m_logLabel = "";

            m_logLabel += $"export...\n";

            var root = State.ExportRoot;

            try
            {
                var converter = new UniVRM10.RuntimeVrmConverter();
                var model = converter.ToModelFrom10(root);

                // if (MeshUtility.Validators.HumanoidValidator.HasRotationOrScale(root))
                // {
                //     // 正規化
                //     m_logLabel += $"normalize...\n";
                //     var modifier = new ModelModifier(model);
                //     modifier.SkinningBake();
                // }

                // 右手系に変換
                m_logLabel += $"convert to right handed coordinate...\n";
                model.ConvertCoordinate(VrmLib.Coordinates.Vrm1, ignoreVrm: false);

                // export vrm-1.0
                var exporter = new UniVRM10.Vrm10Exporter(AssetTextureUtil.IsTextureEditorAsset);
                var option = new VrmLib.ExportArgs();
                exporter.Export(root, model, converter, option, AssetTextureUtil.GetTextureBytesWithMime, Meta ? Meta : m_tmpMeta);

                var exportedBytes = exporter.Storage.ToBytes();

                m_logLabel += $"write to {path}...\n";
                File.WriteAllBytes(path, exportedBytes);
                Debug.Log("exportedBytes: " + exportedBytes.Length);

                var assetPath = ToAssetPath(path);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.ImportAsset(assetPath);
                }
            }
            catch (Exception ex)
            {
                m_logLabel += ex.ToString();
                // rethrow
                throw;
            }
        }

        static string ToAssetPath(string path)
        {
            var assetPath = UniGLTF.UnityPath.FromFullpath(path);
            return assetPath.Value;
        }
    }
}
