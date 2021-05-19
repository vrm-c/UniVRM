using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace UniGLTF
{
    public static class UniGLTFPreference
    {
        [PreferenceItem("UniGLTF")]
        private static void OnPreferenceGUI()
        {
            EditorGUI.BeginChangeCheck();

            // language
            M17N.LanguageGetter.OnGuiSelectLang();
            EditorGUILayout.HelpBox($"Custom editor language setting", MessageType.Info, true);

            // default axis
            GltfIOAxis = (Axes)EditorGUILayout.EnumPopup("Default Invert axis", GltfIOAxis);
            EditorGUILayout.HelpBox($"Default invert axis when glb/gltf import/export", MessageType.Info, true);

            if (EditorGUI.EndChangeCheck())
            {
            }
        }

        const string AXIS_KEY = "UNIGLTF_IO_AXIS";
        static Axes? s_axis;
        public static Axes GltfIOAxis
        {
            set
            {
                EditorPrefs.SetString(AXIS_KEY, value.ToString());
                s_axis = value;
            }
            get
            {
                if (!s_axis.HasValue)
                {
                    var value = EditorPrefs.GetString(AXIS_KEY, default(Axes).ToString());
                    if (Enum.TryParse<Axes>(value, out Axes parsed))
                    {
                        s_axis = parsed;
                    }
                    else
                    {
                        s_axis = default(Axes);
                    }
                }
                return s_axis.Value;
            }
        }
    }
}
