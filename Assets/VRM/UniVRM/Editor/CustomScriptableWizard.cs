// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using uei = UnityEngine.Internal;

namespace VRM
{
    // Derive from this class to create an editor wizard.
    public class CustomScriptableWizard : EditorWindow
    {
        Editor m_Inspector;

        private string m_HelpString = "";
        private string m_ErrorString = "";
        private bool m_IsValid = true;
        private Vector2 m_ScrollPosition;
        private string m_CreateButton = "Create";
        private string m_OtherButton = "";

        private void OnDestroy()
        {
            UnityEditor.Editor.DestroyImmediate(m_Inspector);
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
            GUILayout.Label(m_HelpString, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));

            // Render contents using Generic Inspector GUI
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            GUIUtility.GetControlID(645789, FocusType.Passive);
            bool modified = DrawWizardGUI();
            EditorGUILayout.EndScrollView();

            // Create and Other Buttons
            GUILayout.BeginVertical();
            if (m_ErrorString != string.Empty)
                GUILayout.Label(m_ErrorString, Styles.errorText, GUILayout.MinHeight(32));
            else
                GUILayout.Label(string.Empty, GUILayout.MinHeight(32));
            GUILayout.FlexibleSpace();

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
            GUILayout.EndVertical();
            if (modified)
                InvokeWizardUpdate();

            GUILayout.Space(8);
        }

        protected virtual bool DrawWizardGUI()
        {
            if (m_Inspector == null)
            {
                m_Inspector = Editor.CreateEditor(this);
            }
            m_Inspector.OnInspectorGUI();
            return true;
        }

        // Creates a wizard.
        public static T DisplayWizard<T>(string title) where T : CustomScriptableWizard
        {
            return DisplayWizard<T>(title, "Create", "");
        }

        ///*listonly*
        public static T DisplayWizard<T>(string title, string createButtonName) where T : CustomScriptableWizard
        {
            return DisplayWizard<T>(title, createButtonName, "");
        }

        ///*listonly*
        public static T DisplayWizard<T>(string title, string createButtonName, string otherButtonName) where T : CustomScriptableWizard
        {
            return (T)DisplayWizard(title, typeof(T), createButtonName, otherButtonName);
        }

        [uei.ExcludeFromDocsAttribute]
        public static CustomScriptableWizard DisplayWizard(string title, Type klass, string createButtonName)
        {
            string otherButtonName = "";
            return DisplayWizard(title, klass, createButtonName, otherButtonName);
        }

        [uei.ExcludeFromDocsAttribute]
        public static CustomScriptableWizard DisplayWizard(string title, Type klass)
        {
            string otherButtonName = "";
            string createButtonName = "Create";
            return DisplayWizard(title, klass, createButtonName, otherButtonName);
        }

        // Creates a wizard.
        public static CustomScriptableWizard DisplayWizard(string title, Type klass, [uei.DefaultValueAttribute("\"Create\"")] string createButtonName, [uei.DefaultValueAttribute("\"\"")] string otherButtonName)
        {
            CustomScriptableWizard wizard = CreateInstance(klass) as CustomScriptableWizard;
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

        // Allows you to set the help text of the wizard.
        public string helpString
        {
            get { return m_HelpString; }
            set
            {
                var newString = value ?? string.Empty;
                if (m_HelpString != newString)
                {
                    m_HelpString = newString;
                    Repaint();
                }
            }
        }

        // Allows you to set the error text of the wizard.
        public string errorString
        {
            get { return m_ErrorString; }
            set
            {
                var newString = value ?? string.Empty;
                if (m_ErrorString != newString)
                {
                    m_ErrorString = newString;
                    Repaint();
                }
            }
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

        // Allows you to enable and disable the wizard create button, so that the user can not click it.
        public bool isValid
        {
            get { return m_IsValid; }
            set { m_IsValid = value; }
        }
    }
}
