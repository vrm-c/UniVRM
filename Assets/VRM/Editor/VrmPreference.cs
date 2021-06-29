using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Graphs;
#endif

namespace VRM
{
    public static class VrmPreference
    {
        const string KEY_STOP_VRMASSETPOSTPROCESSOR = "StopVrmAssetPostProcessor";
        const string ASSETPOSTPROCESSOR_STOP_SYMBOL = "VRM_STOP_ASSETPOSTPROCESSOR";

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/VRM0", SettingsScope.User);
            provider.guiHandler = (sarchContext) => OnPreferenceGUI();
            return provider;
        }
#else
        [PreferenceItem("VRM0")]
#endif
        private static void OnPreferenceGUI()
        {
            UniGLTF.UniGLTFPreference.ToggleSymbol(KEY_STOP_VRMASSETPOSTPROCESSOR, ASSETPOSTPROCESSOR_STOP_SYMBOL);
        }
    }
}
