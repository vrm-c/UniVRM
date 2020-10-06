using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// エクスポートダイアログ
    /// </summary>
    public class VRMExporterWizard : EditorWindow
    {
        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export humanoid";

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            VRMExporterWizard.CreateWizard();
        }

        enum Tabs
        {
            Meta,
            Mesh,
            ExportSettings,
        }
        Tabs _tab;

        GUIStyle TabButtonStyle => "LargeButton";

        // GUI.ToolbarButtonSize.FitToContentsも設定できる
        GUI.ToolbarButtonSize TabButtonSize => GUI.ToolbarButtonSize.Fixed;
        const string EXTENSION = ".vrm";

        private static string m_lastExportDir;


        GameObject ExportRoot;

        VRMExportSettings m_settings;
        VRMExportMeshes m_meshes;

        VRMMetaObject m_meta;
        VRMMetaObject Meta
        {
            get { return m_meta; }
            set
            {
                if (m_meta == value)
                {
                    return;
                }
                m_requireValidation = true;
                if (m_metaEditor != null)
                {
                    UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                    m_metaEditor = null;
                }
                m_meta = value;
            }
        }

        void UpdateRoot(GameObject root)
        {
            if (root == ExportRoot)
            {
                return;
            }
            m_requireValidation = true;
            ExportRoot = root;
            UnityEditor.Editor.DestroyImmediate(m_metaEditor);
            m_metaEditor = null;

            if (ExportRoot == null)
            {
                Meta = null;
            }
            else
            {
                // do validation
                Validate();

                // default setting
                m_settings.PoseFreeze =
                VRMExporterValidator.HasRotationOrScale(ExportRoot)
                || m_meshes.Meshes.Any(x => x.ExportBlendShapeCount > 0 && !x.HasSkinning)
                ;

                var meta = ExportRoot.GetComponent<VRMMeta>();
                if (meta != null)
                {
                    Meta = meta.Meta;
                }
                else
                {
                    Meta = null;
                }
            }
        }

        void Validate()
        {
            if (!m_requireValidation)
            {
                return;
            }
            m_validator.Validate(ExportRoot, m_settings, Meta != null ? Meta : m_tmpMeta);
            m_requireValidation = false;
            m_meshes.SetRoot(ExportRoot, m_settings);
        }

        VRMMetaObject m_tmpMeta;

        Editor m_metaEditor;
        Editor m_settingsInspector;
        Editor m_meshesInspector;

        VRMExporterValidator m_validator = new VRMExporterValidator();
        bool m_requireValidation = true;

        private Vector2 m_ScrollPosition;

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += OnWizardUpdate;
            Selection.selectionChanged += OnWizardUpdate;

            m_tmpMeta = ScriptableObject.CreateInstance<VRMMetaObject>();

            m_settings = ScriptableObject.CreateInstance<VRMExportSettings>();
            m_settingsInspector = Editor.CreateEditor(m_settings);

            m_meshes = ScriptableObject.CreateInstance<VRMExportMeshes>();
            m_meshesInspector = Editor.CreateEditor(m_meshes);
        }

        void OnDisable()
        {
            ExportRoot = null;

            // Debug.Log("OnDisable");
            Selection.selectionChanged -= OnWizardUpdate;
            Undo.willFlushUndoRecord -= OnWizardUpdate;

            // m_metaEditor
            UnityEditor.Editor.DestroyImmediate(m_metaEditor);
            m_metaEditor = null;
            // m_settingsInspector
            UnityEditor.Editor.DestroyImmediate(m_settingsInspector);
            m_settingsInspector = null;
            // m_meshesInspector
            UnityEditor.Editor.DestroyImmediate(m_meshesInspector);
            m_meshesInspector = null;
            // Meta
            Meta = null;
            ScriptableObject.DestroyImmediate(m_tmpMeta);
            m_tmpMeta = null;
            // m_settings
            ScriptableObject.DestroyImmediate(m_settings);
            m_settings = null;
            // m_meshes
            ScriptableObject.DestroyImmediate(m_meshes);
            m_meshes = null;
        }

        private void InvokeWizardUpdate()
        {
            const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            MethodInfo method = GetType().GetMethod("OnWizardUpdate", kInstanceInvokeFlags);
            if (method != null)
                method.Invoke(this, null);
        }

        private class Styles
        {
            public static string errorText = "Wizard Error";
            public static string box = "Wizard Box";
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

        private void OnGUI()
        {
            if (m_tmpMeta == null)
            {
                // OnDisable
                return;
            }

            EditorGUIUtility.labelWidth = 150;

            // lang
            M17N.Getter.OnGuiSelectLang();

            EditorGUILayout.LabelField("ExportRoot");
            {
                var root = (GameObject)EditorGUILayout.ObjectField(ExportRoot, typeof(GameObject), true);
                UpdateRoot(root);
            }

            // ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint Aborting
            // Validation により GUI の表示項目が変わる場合があるので、
            // EventType.Layout と EventType.Repaint 間で内容が変わらないようしている。
            if (Event.current.type == EventType.Layout)
            {
                Validate();
            }

            //
            // Humanoid として適正か？ ここで失敗する場合は Export UI を表示しない
            //
            if (!m_validator.RootAndHumanoidCheck(ExportRoot, m_settings, m_meshes.Meshes))
            {
                return;
            }

            EditorGUILayout.HelpBox($"Mesh size: {m_meshes.ExpectedExportByteSize / 1000000.0f:0.0} MByte", MessageType.Info);
            _tab = TabBar.OnGUI(_tab, TabButtonStyle, TabButtonSize);

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);

            //
            // VRM の Validation
            //
            foreach (var v in m_validator.Validations)
            {
                v.DrawGUI();
            }
            foreach (var meshInfo in m_meshes.Meshes)
            {
                switch (meshInfo.VertexColor)
                {
                    case UniGLTF.MeshExportInfo.VertexColorState.ExistsAndMixed:
                        Validation.Warning($"{meshInfo.Renderer}: Both vcolor.multiply and not multiply unlit materials exist").DrawGUI();
                        break;
                }
            }

            bool modified = DrawWizardGUI();
            EditorGUILayout.EndScrollView();

            // Create and Other Buttons
            {
                // errors            
                GUILayout.BeginVertical();
                // GUILayout.FlexibleSpace();

                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = m_validator.IsValid;

                    if (GUILayout.Button("Export", GUILayout.MinWidth(100)))
                    {
                        OnWizardCreate();
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
                m_requireValidation = true;
                Repaint();
            }
        }

        bool DrawWizardGUI()
        {
            if (m_tmpMeta == null)
            {
                // disabled
                return false;
            }

            // tabbar
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
                    m_settingsInspector.OnInspectorGUI();
                    break;

                case Tabs.Mesh:
                    m_meshesInspector.OnInspectorGUI();
                    break;
            }

            return true;
        }

        // Creates a wizard.
        public static VRMExporterWizard DisplayWizard()
        {
            VRMExporterWizard wizard = CreateInstance<VRMExporterWizard>();
            wizard.titleContent = new GUIContent("VRM Exporter");
            if (wizard != null)
            {
                wizard.InvokeWizardUpdate();
                wizard.ShowUtility();
            }
            return wizard;
        }

        public static void CreateWizard()
        {
            var wiz = VRMExporterWizard.DisplayWizard();
            var go = Selection.activeObject as GameObject;

            // update checkbox
            wiz.UpdateRoot(go);

            if (go != null)
            {
                wiz.m_settings.PoseFreeze = VRMExporterValidator.HasRotationOrScale(go);
            }

            wiz.OnWizardUpdate();
        }

        void OnWizardCreate()
        {
            string directory;
            if (string.IsNullOrEmpty(m_lastExportDir))
                directory = Directory.GetParent(Application.dataPath).ToString();
            else
                directory = m_lastExportDir;

            // save dialog
            var path = EditorUtility.SaveFilePanel(
                    "Save vrm",
                    directory,
                    ExportRoot.name + EXTENSION,
                    EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastExportDir = Path.GetDirectoryName(path).Replace("\\", "/");

            // export
            VRMEditorExporter.Export(path, ExportRoot, Meta != null ? Meta : m_tmpMeta, m_settings, m_meshes.Meshes);
        }

        void OnWizardUpdate()
        {
            UpdateRoot(ExportRoot);
            m_requireValidation = true;
            Repaint();
        }
    }
}
