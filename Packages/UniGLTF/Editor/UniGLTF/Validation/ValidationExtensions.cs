using System;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class ValidationExtensions
    {
        static GUIContent s_info;
        public static GUIContent Info
        {
            get
            {
                if (s_info == null)
                {
                    s_info = EditorGUIUtility.IconContent("console.infoicon");
                }
                return s_info;
            }
        }

        static GUIContent s_warn;
        public static GUIContent Warn
        {
            get
            {
                if (s_warn == null)
                {
                    s_warn = EditorGUIUtility.IconContent("console.warnicon");
                }
                return s_warn;
            }
        }

        static GUIContent s_error;
        public static GUIContent Error
        {
            get
            {
                if (s_error == null)
                {
                    s_error = EditorGUIUtility.IconContent("console.erroricon");
                }
                return s_error;
            }
        }

        public static void DrawGUI(this Validation self)
        {
            if (string.IsNullOrEmpty(self.Message))
            {
                return;
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    switch (self.ErrorLevel)
                    {
                        case ErrorLevels.Info:
                            EditorGUILayout.LabelField(Info, GUILayout.Width(30), GUILayout.Height(30));
                            break;

                        case ErrorLevels.Warning:
                            EditorGUILayout.LabelField(Warn, GUILayout.Width(30), GUILayout.Height(30));
                            break;

                        case ErrorLevels.Error:
                            EditorGUILayout.LabelField(Error, GUILayout.Width(30), GUILayout.Height(30));
                            break;
                    }

                    using (new GUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(self.Message);
                        if (self.Context.Context != null)
                        {
                            EditorGUILayout.ObjectField(self.Context.Context, self.Context.Type, true);
                        }
                        if (self.Context.Extended != null)
                        {
                            self.Context.Extended();
                        }
                    }
                }
            }
        }
    }
}
