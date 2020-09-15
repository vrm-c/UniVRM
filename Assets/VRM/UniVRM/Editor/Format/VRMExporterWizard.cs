using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using uei = UnityEngine.Internal;

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

        GameObject ExportRoot;

        VRMExportSettings m_settings;

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
                // default setting
                m_settings.PoseFreeze = VRMExporterValidator.HasRotationOrScale(ExportRoot);

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

        VRMMetaObject m_tmpMeta;

        Editor m_metaEditor;
        Editor m_Inspector;

        VRMExporterValidator m_validator = new VRMExporterValidator();
        bool m_requireValidation = true;

        private Vector2 m_ScrollPosition;
        private string m_CreateButton = "Create";
        private string m_OtherButton = "";

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += OnWizardUpdate;
            Selection.selectionChanged += OnWizardUpdate;

            m_tmpMeta = ScriptableObject.CreateInstance<VRMMetaObject>();

            if (m_settings == null)
            {
                m_settings = ScriptableObject.CreateInstance<VRMExportSettings>();
            }
            if (m_Inspector == null)
            {
                m_Inspector = Editor.CreateEditor(m_settings);
            }
        }

        void OnDisable()
        {
            ExportRoot = null;

            // Debug.Log("OnDisable");
            Selection.selectionChanged -= OnWizardUpdate;
            Undo.willFlushUndoRecord -= OnWizardUpdate;

            UnityEditor.Editor.DestroyImmediate(m_metaEditor);
            m_metaEditor = null;
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
            m_Inspector = null;
            Meta = null;
            ScriptableObject.DestroyImmediate(m_tmpMeta);
            m_tmpMeta = null;
            ScriptableObject.DestroyImmediate(m_settings);
            m_settings = null;
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

            if (Event.current.type == EventType.Layout)
            {
                // ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint Aborting
                // Validation により GUI の表示項目が変わる場合があるので、
                // EventType.Layout と EventType.Repaint 間で内容が変わらないようしている。
                if (m_requireValidation)
                {
                    m_validator.Validate(ExportRoot, m_settings, Meta != null ? Meta : m_tmpMeta);
                    m_requireValidation = false;
                }
            }

            //
            // Humanoid として適正か？ ここで失敗する場合は Export UI を表示しない
            //
            if (!m_validator.RootAndHumanoidCheck(ExportRoot, m_settings))
            {
                return;
            }

            EditorGUILayout.HelpBox($"Mesh size: {m_validator.ExpectedByteSize / 1000000.0f:0.0} MByte", MessageType.Info);

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

                    const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                    if (m_OtherButton != "" && GUILayout.Button(m_OtherButton, GUILayout.MinWidth(100)))
                    {
                        MethodInfo method = GetType().GetMethod("OnWizardOtherButton", kInstanceInvokeFlags);
                        if (method != null)
                        {
                            method.Invoke(this, null);
                            GUIUtility.ExitGUI();
                        }
                        else
                            Debug.LogError("OnWizardOtherButton has not been implemented in script");
                    }

                    if (m_CreateButton != "" && GUILayout.Button(m_CreateButton, GUILayout.MinWidth(100)))
                    {
                        MethodInfo method = GetType().GetMethod("OnWizardCreate", kInstanceInvokeFlags);
                        if (method != null)
                            method.Invoke(this, null);
                        else
                            Debug.LogError("OnWizardCreate has not been implemented in script");
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

        enum Tabs
        {
            Meta,
            ExportSettings,
        }
        Tabs _tab;

        GUIStyle TabButtonStyle => "LargeButton";

        // GUI.ToolbarButtonSize.FitToContentsも設定できる
        GUI.ToolbarButtonSize TabButtonSize => GUI.ToolbarButtonSize.Fixed;

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
                    m_Inspector.OnInspectorGUI();
                    break;
            }

            return true;
        }

        // Creates a wizard.
        public static T DisplayWizard<T>(string title) where T : VRMExporterWizard
        {
            return DisplayWizard<T>(title, "Create", "");
        }

        ///*listonly*
        public static T DisplayWizard<T>(string title, string createButtonName) where T : VRMExporterWizard
        {
            return DisplayWizard<T>(title, createButtonName, "");
        }

        ///*listonly*
        public static T DisplayWizard<T>(string title, string createButtonName, string otherButtonName) where T : VRMExporterWizard
        {
            return (T)DisplayWizard(title, typeof(T), createButtonName, otherButtonName);
        }

        [uei.ExcludeFromDocsAttribute]
        public static VRMExporterWizard DisplayWizard(string title, Type klass, string createButtonName)
        {
            string otherButtonName = "";
            return DisplayWizard(title, klass, createButtonName, otherButtonName);
        }

        [uei.ExcludeFromDocsAttribute]
        public static VRMExporterWizard DisplayWizard(string title, Type klass)
        {
            string otherButtonName = "";
            string createButtonName = "Create";
            return DisplayWizard(title, klass, createButtonName, otherButtonName);
        }

        // Creates a wizard.
        public static VRMExporterWizard DisplayWizard(string title, Type klass, [uei.DefaultValueAttribute("\"Create\"")] string createButtonName, [uei.DefaultValueAttribute("\"\"")] string otherButtonName)
        {
            VRMExporterWizard wizard = CreateInstance(klass) as VRMExporterWizard;
            wizard.m_CreateButton = createButtonName;
            wizard.m_OtherButton = otherButtonName;
            wizard.titleContent = new GUIContent(title);
            if (wizard != null)
            {
                wizard.InvokeWizardUpdate();
                wizard.ShowUtility();
            }
            return wizard;
        }

        // Allows you to set the create button text of the wizard.
        public string createButtonName
        {
            get { return m_CreateButton; }
            set
            {
                var newString = value ?? string.Empty;
                if (m_CreateButton != newString)
                {
                    m_CreateButton = newString;
                    Repaint();
                }
            }
        }

        // Allows you to set the other button text of the wizard.
        public string otherButtonName
        {
            get { return m_OtherButton; }
            set
            {
                var newString = value ?? string.Empty;
                if (m_OtherButton != newString)
                {
                    m_OtherButton = newString;
                    Repaint();
                }
            }
        }

        const string EXTENSION = ".vrm";

        private static string m_lastExportDir;

        public static void CreateWizard()
        {
            var wiz = VRMExporterWizard.DisplayWizard<VRMExporterWizard>(
                "VRM Exporter", "Export");
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
            VRMEditorExporter.Export(path, ExportRoot, Meta != null ? Meta : m_tmpMeta, m_settings);
        }

        void OnWizardUpdate()
        {
            UpdateRoot(ExportRoot);
            m_requireValidation = true;
            Repaint();
        }
    }
}
