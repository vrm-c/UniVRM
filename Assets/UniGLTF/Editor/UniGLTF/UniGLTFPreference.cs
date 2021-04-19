using UnityEngine;
using UnityEditor;
using System.Linq;

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
            EditorGUILayout.HelpBox($"UniGLTF, UniVRM custom editor language setting", MessageType.Info, true);

            // default axis

            if (EditorGUI.EndChangeCheck())
            {
            }
        }
    }
}
