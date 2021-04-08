using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MeshUtility;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using VrmLib;
using VRMShaders;

namespace UniVRM10
{
    public class VRM10ExportDialog : EditorWindow
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

        MeshUtility.ExporterDialogState m_state;

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
        VRM10MetaObject m_tmpMeta;

        Editor m_metaEditor;

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            Selection.selectionChanged += Repaint;

            m_tmpMeta = ScriptableObject.CreateInstance<VRM10MetaObject>();
            m_tmpMeta.Authors = new List<string> { "" };

            m_state = new MeshUtility.ExporterDialogState();
            m_state.ExportRootChanged += (root) =>
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

                Repaint();
            };
            m_state.ExportRoot = Selection.activeObject as GameObject;
        }

        void OnDisable()
        {
            m_state.Dispose();

            // Debug.Log("OnDisable");
            Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;
        }

        public delegate Vector2 BeginVerticalScrollViewFunc(Vector2 scrollPosition, bool alwaysShowVertical, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options);
        static BeginVerticalScrollViewFunc s_func;
        static BeginVerticalScrollViewFunc BeginVerticalScrollView
        {
            get
            {
                if (s_func == null)
                {
                    var methods = typeof(EditorGUILayout).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == "BeginVerticalScrollView").ToArray();
                    var method = methods.First(x => x.GetParameters()[1].ParameterType == typeof(bool));
                    s_func = (BeginVerticalScrollViewFunc)method.CreateDelegate(typeof(BeginVerticalScrollViewFunc));
                }
                return s_func;
            }
        }
        private Vector2 m_ScrollPosition;

        IEnumerable<MeshUtility.Validator> ValidatorFactory()
        {
            yield return MeshUtility.Validators.HierarchyValidator.Validate;
            if (!m_state.ExportRoot)
            {
                yield break;
            }

            // MeshUtility.Validators.HumanoidValidator.EnableFreeze = false;
            // yield return MeshUtility.Validators.HumanoidValidator.Validate;

            // yield return VRMExporterValidator.Validate;
            // yield return VRMSpringBoneValidator.Validate;

            // var firstPerson = m_state.ExportRoot.GetComponent<VRMFirstPerson>();
            // if (firstPerson != null)
            // {
            //     yield return firstPerson.Validate;
            // }

            // var proxy = m_state.ExportRoot.GetComponent<VRMBlendShapeProxy>();
            // if (proxy != null)
            // {
            //     yield return proxy.Validate;
            // }

            var meta = Meta ? Meta : m_tmpMeta;
            yield return meta.Validate;
        }

        private void OnGUI()
        {
            // ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint Aborting
            // Validation により GUI の表示項目が変わる場合があるので、
            // EventType.Layout と EventType.Repaint 間で内容が変わらないようしている。
            if (Event.current.type == EventType.Layout)
            {
                m_state.Validate(ValidatorFactory());
            }

            EditorGUIUtility.labelWidth = 150;

            // lang
            MeshUtility.M17N.Getter.OnGuiSelectLang();

            EditorGUILayout.LabelField("ExportRoot");
            {
                m_state.ExportRoot = (GameObject)EditorGUILayout.ObjectField(m_state.ExportRoot, typeof(GameObject), true);
            }

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);

            bool modified = ScrollArea();

            EditorGUILayout.EndScrollView();

            // Create and Other Buttons
            {
                // errors            
                GUILayout.BeginVertical();
                // GUILayout.FlexibleSpace();

                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = m_state.Validations.All(x => x.CanExport);

                    if (GUILayout.Button("Export", GUILayout.MinWidth(100)))
                    {
                        OnExportClicked(m_state.ExportRoot);
                        Close();
                        GUIUtility.ExitGUI();
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(8);

            if (modified)
            {
                m_state.Invalidate();
            }
        }

        bool ScrollArea()
        {
            //
            // Validation
            //
            foreach (var v in m_state.Validations)
            {
                v.DrawGUI();
                if (v.ErrorLevel == MeshUtility.ErrorLevels.Critical)
                {
                    // Export UI を表示しない
                    return false;
                }
            }

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

        const string EXTENSION = ".vrm";
        private static string m_lastExportDir;
        void OnExportClicked(GameObject root)
        {
            m_logLabel = "";

            string directory;
            if (string.IsNullOrEmpty(m_lastExportDir))
                directory = Directory.GetParent(Application.dataPath).ToString();
            else
                directory = m_lastExportDir;

            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    directory,
                    root.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");

            m_logLabel += $"export...\n";

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
                exporter.Export(root, model, converter, option, Meta ? Meta : m_tmpMeta);

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
