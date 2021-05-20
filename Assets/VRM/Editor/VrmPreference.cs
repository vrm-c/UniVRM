using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VRM
{
    public static class VrmPreference
    {
        const string KEY_STOP_VRMASSETPOSTPROCESSOR = "StopVrmAssetPostProcessor";
        const string ASSETPOSTPROCESSOR_STOP_SYMBOL = "VRM_STOP_ASSETPOSTPROCESSOR";

        [PreferenceItem("VRM0")]
        private static void OnPreferenceGUI()
        {
            UniGLTF.UniGLTFPreference.ToggleSymbol(KEY_STOP_VRMASSETPOSTPROCESSOR, ASSETPOSTPROCESSOR_STOP_SYMBOL);
        }
    }
}
