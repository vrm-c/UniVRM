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
            GltfIOAxis = (Axises)EditorGUILayout.EnumPopup("Default Invert axis", GltfIOAxis);
            EditorGUILayout.HelpBox($"Default invert axis when glb/gltf import/export", MessageType.Info, true);

            if (EditorGUI.EndChangeCheck())
            {
            }
        }

        const string AXIS_KEY = "UNIGLTF_IO_AXIS";
        static Axises? s_axis;
        public static Axises GltfIOAxis
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
                    var value = EditorPrefs.GetString(AXIS_KEY, default(Axises).ToString());
                    if (Enum.TryParse<Axises>(value, out Axises parsed))
                    {
                        s_axis = parsed;
                    }
                    else
                    {
                        s_axis = default(Axises);
                    }
                }
                return s_axis.Value;
            }
        }
    }
}
