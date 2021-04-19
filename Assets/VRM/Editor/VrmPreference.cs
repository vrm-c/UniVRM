using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VRM
{
    public static class VrmInfo
    {
        const string KEY_STOP_VRMASSETPOSTPROCESSOR = "StopVrmAssetPostProcessor";
        const string ASSETPOSTPROCESSOR_STOP_SYMBOL = "VRM_STOP_ASSETPOSTPROCESSOR";

        [PreferenceItem("VRM0")]
        private static void OnPreferenceGUI()
        {
            EditorGUI.BeginChangeCheck();

            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');

            var stop = current.Any(x => x == ASSETPOSTPROCESSOR_STOP_SYMBOL);
            var newValue = GUILayout.Toggle(stop, KEY_STOP_VRMASSETPOSTPROCESSOR);
            EditorGUILayout.HelpBox($"define C# symbol '{ASSETPOSTPROCESSOR_STOP_SYMBOL}'", MessageType.Info, true);
            if (EditorGUI.EndChangeCheck())
            {
            }

            if (stop != newValue)
            {
                stop = newValue;
                if (stop)
                {
                    // add symbol
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target,
                    string.Join(";", current.Concat(new[] { ASSETPOSTPROCESSOR_STOP_SYMBOL }))
                    );
                }
                else
                {
                    // remove symbol
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target,
                    string.Join(";", current.Where(x => x != ASSETPOSTPROCESSOR_STOP_SYMBOL))
                    );
                }
            }
        }
    }
}
