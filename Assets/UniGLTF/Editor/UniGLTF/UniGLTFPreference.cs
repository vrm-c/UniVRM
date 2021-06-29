using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Graphs;
#endif

namespace UniGLTF
{
    public static class UniGLTFPreference
    {
        public const string UNIGLTF_ENABLE_ZIPARCHVIE_IMPORTER = "UNIGLTF_ENABLE_ZIPARCHVIE_IMPORTER";

        static IEnumerable<string> GetReimportPaths()
        {
            String[] guids = AssetDatabase.FindAssets("t:GameObject", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(assetPath) is GlbScriptedImporter glb)
                {
                    if (glb.m_reverseAxis == ScriptedImporterAxes.Default)
                    {
                        Debug.Log($"[reimport] {assetPath}");
                        yield return assetPath;
                    }
                }
                else if (AssetImporter.GetAtPath(assetPath) is GltfScriptedImporter gltf)
                {
                    if (gltf.m_reverseAxis == ScriptedImporterAxes.Default)
                    {
                        Debug.Log($"[reimport] {assetPath}");
                        yield return assetPath;
                    }
                }
            }
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/UniGLTF", SettingsScope.User);
            provider.guiHandler = (sarchContext) => OnPreferenceGUI();
            return provider;
        }
#else
        [PreferenceItem("UniGLTF")]
#endif
        private static void OnPreferenceGUI()
        {
            // language
            M17N.LanguageGetter.OnGuiSelectLang();
            EditorGUILayout.HelpBox($"Custom editor language setting", MessageType.Info, true);

            // default axis
            EditorGUI.BeginChangeCheck();
            var gltfIOAxis = (Axes)EditorGUILayout.EnumPopup("Default Invert axis", GltfIOAxis);
            EditorGUILayout.HelpBox($"Default invert axis when glb/gltf import/export", MessageType.Info, true);
            if (EditorGUI.EndChangeCheck())
            {
                // global setting
                GltfIOAxis = gltfIOAxis;

                // apply assets
                foreach (var path in GetReimportPaths().ToArray())
                {
                    AssetDatabase.ImportAsset(path, default);
                }
            }

            // zip
            UniGLTF.UniGLTFPreference.ToggleSymbol("Enable zip importer that contains gltf", UNIGLTF_ENABLE_ZIPARCHVIE_IMPORTER);
            EditorGUILayout.HelpBox("This is `.zip` extension importer", MessageType.Warning);
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

        public static bool HasSymbol(string symbol)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');
            return current.Contains(symbol);
        }

        public static void AddSymbol(string symbol)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target,
                string.Join(";", current.Concat(new[] { symbol }))
            );
        }

        public static void RemoveSymbol(string symbol)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target,
                string.Join(";", current.Where(x => x != symbol))
            );
        }

        public static void ToggleSymbol(string title, string symbol)
        {
            EditorGUI.BeginChangeCheck();
            var isStop = HasSymbol(symbol);
            var newValue = GUILayout.Toggle(isStop, title);
            EditorGUILayout.HelpBox($"define C# symbol '{symbol}'", MessageType.Info, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (newValue)
                {
                    AddSymbol(symbol);
                }
                else
                {
                    RemoveSymbol(symbol);
                }
            }
        }
    }
}
