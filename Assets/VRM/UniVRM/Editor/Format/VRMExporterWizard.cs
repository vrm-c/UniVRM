using System;
using System.Collections.Generic;
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
        [SerializeField]
        public GameObject ExportRoot;

        SerializedProperty m_exportRoot;

        [SerializeField]
        public VRMExportSettings m_settings;

        VRMMetaObject m_meta;
        VRMMetaObject Meta
        {
            get { return m_meta; }
            set
            {
                if (m_meta == value) return;
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
            ExportRoot = root;
            if (ExportRoot == null)
            {
                Meta = null;
            }
            else
            {
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
        VRMMetaObject TmpMeta
        {
            get
            {
                if (m_tmpMeta == null)
                {
                    m_tmpMeta = ScriptableObject.CreateInstance<VRMMetaObject>();
                }
                return m_tmpMeta;
            }
        }

        Editor m_metaEditor;
        Editor m_Inspector;

        private bool m_IsValid = true;

        List<Validation> m_validations = new List<Validation>();

        private Vector2 m_ScrollPosition;
        private string m_CreateButton = "Create";
        private string m_OtherButton = "";

        private void OnDestroy()
        {
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
            Meta = null;
            ScriptableObject.DestroyImmediate(m_tmpMeta);
            ScriptableObject.DestroyImmediate(m_settings);
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
                if (s_func is null)
                {
                    var methods = typeof(EditorGUILayout).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == "BeginVerticalScrollView").ToArray();
                    var method = methods.First(x => x.GetParameters()[1].ParameterType == typeof(bool));
                    s_func = (BeginVerticalScrollViewFunc)method.CreateDelegate(typeof(BeginVerticalScrollViewFunc));
                }
                return s_func;
            }
        }


        //@TODO: Force repaint if scripts recompile
        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.LabelField("ExportRoot");
            var root = (GameObject)EditorGUILayout.ObjectField(ExportRoot, typeof(GameObject), true);
            UpdateRoot(root);

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);
            bool modified = DrawWizardGUI();
            EditorGUILayout.EndScrollView();

            // Create and Other Buttons
            {
                // errors            
                GUILayout.BeginVertical();
                // foreach (var v in m_validations)
                // {
                //     v.DrawGUI();
                // }
                GUILayout.FlexibleSpace();

                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = m_IsValid;

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
            if (modified)
                InvokeWizardUpdate();

            GUILayout.Space(8);
        }

        // Style定義
        enum Tabs
        {
            Meta,
            ExportSettings,
        }

        static class TabStyles
        {
            private static GUIContent[] _tabToggles = null;
            public static GUIContent[] TabToggles
            {
                get
                {
                    if (_tabToggles == null)
                    {
                        _tabToggles = System.Enum.GetNames(typeof(Tabs)).Select(x => new GUIContent(x)).ToArray();
                    }
                    return _tabToggles;
                }
            }

            public static readonly GUIStyle TabButtonStyle = "LargeButton";

            // GUI.ToolbarButtonSize.FitToContentsも設定できる
            public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;
        }

        Tabs _tab;

        protected virtual bool DrawWizardGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                // タブを描画する
                _tab = (Tabs)GUILayout.Toolbar((int)_tab, TabStyles.TabToggles, TabStyles.TabButtonStyle, TabStyles.TabButtonSize);
                GUILayout.FlexibleSpace();
            }

            {
                if (m_metaEditor == null)
                {
                    if (Meta != null)
                    {
                        m_metaEditor = Editor.CreateEditor(Meta);
                    }
                    else
                    {
                        m_metaEditor = Editor.CreateEditor(TmpMeta);
                    }
                }
            }
            {
                if (m_Inspector == null)
                {
                    if (m_settings == null)
                    {
                        m_settings = ScriptableObject.CreateInstance<VRMExportSettings>();
                    }
                    m_Inspector = Editor.CreateEditor(m_settings);
                }
            }

            switch (_tab)
            {
                case Tabs.Meta:
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

        // // Magic Methods

        // // This is called when the wizard is opened or whenever the user changes something in the wizard.
        // void OnWizardUpdate();

        // // This is called when the user clicks on the Create button.
        // void OnWizardCreate();

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

        // Allows you to enable and disable the wizard create button, so that the user can not click it.
        public bool isValid
        {
            get { return m_IsValid; }
            set { m_IsValid = value; }
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

            wiz.OnWizardUpdate();
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += OnWizardUpdate;
            Selection.selectionChanged += OnWizardUpdate;
        }

        void OnDisable()
        {
            // Debug.Log("OnDisable");
            Selection.selectionChanged -= OnWizardUpdate;
            Undo.willFlushUndoRecord -= OnWizardUpdate;
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
            VRMEditorExporter.Export(path, ExportRoot, m_settings);
        }

        void OnWizardUpdate()
        {
            // m_validations.Clear();
            // m_validations.AddRange(m_settings.Validate());
            // if (Meta != null)
            // {
            //     m_validations.AddRange(Meta.Validate());
            // }
            // else
            // {
            //     m_validations.Add(Validation.Error("meta がありません"));
            // }

            // var hasError = m_validations.Any(x => !x.CanExport);
            // m_IsValid = !hasError;

            UpdateRoot(ExportRoot);

            Repaint();
            // GUIUtility.ExitGUI();
        }
    }

}
