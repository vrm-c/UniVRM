using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System;

namespace VRM
{
    /// <summary>
    /// VersionDialog
    /// 
    /// UniGLTFVersion は、 VRMVersion から自動的に決まる。
    /// 
    /// Major = 2
    /// Minor = VRMVersion.MINOR - 64
    /// Patch = VRMVersion.PATCH
    /// 
    /// </summary>
    public class VRMVersionMenu : EditorWindow
    {
        /// <summary>
        /// UNIGLTF
        /// </summary>
        static string UniGltfVersionPath = "Assets/UniGLTF/Runtime/UniGLTF/UniGLTFVersion.cs";

        const string UniGltfVersionTemplate = @"
namespace UniGLTF
{{
    public static partial class UniGLTFVersion
    {{
        public const int MAJOR = {0};
        public const int MINOR = {1};
        public const int PATCH = {2};
        public const string VERSION = ""{0}.{1}.{2}"";
    }}
}}
";

        /// <summary>
        /// VRM
        /// </summary>
        const string VrmVersionPath = "Assets/VRM/Runtime/Format/VRMVersion.cs";
        const string VrmVersionTemplate = @"
namespace VRM
{{
    public static partial class VRMVersion
    {{
        public const int MAJOR = {0};
        public const int MINOR = {1};
        public const int PATCH = {2};
        public const string VERSION = ""{0}.{1}.{2}"";
    }}
}}
";

        struct UpmPackage
        {
            public readonly string Name;
            public readonly string Path;
            public readonly string Template;

            public UpmPackage(string name, string path, string template)
            {
                Name = name;
                Path = path;
                Template = template;
            }
        }

        UpmPackage[] Packages = new UpmPackage[]
        {
            new UpmPackage("VRMShaders", "Assets/VRMShaders/package.json",
@"{{
  ""name"": ""com.vrmc.vrmshaders"",
  ""version"": ""{1}"",
  ""displayName"": ""VRM Shaders"",
  ""description"": ""VRM Shaders"",
  ""unity"": ""2018.4"",
  ""keywords"": [
    ""vrm"",
    ""shader""
  ],
  ""author"": {{
    ""name"": ""VRM Consortium""
  }}
}}
"),
            new UpmPackage("VRM", "Assets/VRM/package.json",
@"{{
  ""name"": ""com.vrmc.univrm"",
  ""version"": ""{1}"",
  ""displayName"": ""VRM"",
  ""description"": ""VRM importer"",
  ""unity"": ""2018.4"",
  ""keywords"": [
    ""vrm"",
    ""importer"",
    ""avatar"",
    ""vr""
  ],
  ""author"": {{
    ""name"": ""VRM Consortium""
  }},
  ""dependencies"": {{
    ""com.vrmc.vrmshaders"": ""{1}"",
    ""com.vrmc.unigltf"": ""{0}""
  }}
}}
"),
        };

        UpmPackage UniGLTFPackage = new UpmPackage("UniGLTF", "Assets/UniGLTF/package.json",
@"{{
  ""name"": ""com.vrmc.unigltf"",
  ""version"": ""{0}"",
  ""displayName"": ""UniGLTF"",
  ""description"": ""GLTF importer and exporter"",
  ""unity"": ""2018.4"",
  ""keywords"": [
    ""gltf""
  ],
  ""author"": {{
    ""name"": ""VRM Consortium""
  }},
  ""dependencies"": {{
    ""com.vrmc.vrmshaders"": ""{1}""
  }}
}}");

        [SerializeField]
        string m_vrmVersion;

        (int, int, int) m_uniGltfVersion
        {
            get
            {
                if (TryGetVersion(m_vrmVersion, out (int, int, int) vrmVersion))
                {
                    return (2, vrmVersion.Item2 - 64, vrmVersion.Item3);
                }
                else
                {
                    return (0, 0, 0);
                }
            }
        }

        static bool TryGetVersion(string src, out (int, int, int) version)
        {
            try
            {
                if (string.IsNullOrEmpty(src))
                {
                    version = default;
                    return false;
                }

                var splitted = src.Split('.');
                if (splitted.Length != 3)
                {
                    version = default;
                    return false;
                }

                version = (
                    int.Parse(splitted[0]),
                    int.Parse(splitted[1]),
                    int.Parse(splitted[2])
                );
                return true;
            }
            catch (Exception)
            {
                version = default;
                return false;
            }
        }

        /// <summary>
        /// バージョン管理ダイアログ
        /// </summary>
        void OnGUI()
        {
            GUILayout.Label("VRM");
            GUILayout.Label($"Current version: {VRMVersion.VERSION}");
            m_vrmVersion = EditorGUILayout.TextField("Major.Minor.Patch", m_vrmVersion);
            GUILayout.Space(30);

            GUILayout.Label("UniGLTF");
            GUILayout.Label($"Current version: {UniGLTFVersion.VERSION}");
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.TextField("Major.Minor.Patch", $"{m_uniGltfVersion}");
                GUI.enabled = enabled;
            }
            GUILayout.Space(30);

            if (GUILayout.Button("Apply"))
            {
                if (TryGetVersion(m_vrmVersion, out (int, int, int) vrmVersion))
                {
                    UpdateVrmVersion(m_uniGltfVersion, vrmVersion);
                    UpdateUniGLTFVersion(m_uniGltfVersion, vrmVersion);
                    AssetDatabase.Refresh();
                    Debug.Log($"{m_uniGltfVersion}, {vrmVersion}");
                }
                else
                {
                    Debug.LogWarning($"InvalidFormat: {m_vrmVersion}");
                }
            }

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

        void UpdateUniGLTFVersion((int, int, int) uniGltf, (int, int, int) vrm)
        {
            var utf8 = new UTF8Encoding(false);
            File.WriteAllText(UniGltfVersionPath, string.Format(UniGltfVersionTemplate,
                uniGltf.Item1,
                uniGltf.Item2,
                uniGltf.Item3), utf8);

            File.WriteAllText(UniGLTFPackage.Path, string.Format(UniGLTFPackage.Template,
                $"{uniGltf.Item1}.{uniGltf.Item2}.{uniGltf.Item3}",
                $"{vrm.Item1}.{vrm.Item2}.{vrm.Item3}"
                ), utf8);
        }

        void UpdateVrmVersion((int, int, int) uniGltf, (int, int, int) vrm)
        {
            // generate
            var utf8 = new UTF8Encoding(false);
            File.WriteAllText(VrmVersionPath, string.Format(VrmVersionTemplate,
                vrm.Item1,
                vrm.Item2,
                vrm.Item3), utf8);
            // UPM
            foreach (var upm in Packages)
            {
                File.WriteAllText(upm.Path, string.Format(upm.Template,
                    $"{uniGltf.Item1}.{uniGltf.Item2}.{uniGltf.Item3}",
                    $"{vrm.Item1}.{vrm.Item2}.{vrm.Item3}"
                    ), utf8);
            }
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.MENU + "/VersionDialog")]
#endif
        static void ShowVersionDialog()
        {
            var window = ScriptableObject.CreateInstance<VRMVersionMenu>();
            window.m_vrmVersion = VRMVersion.VERSION;
            // window.m_uniGltfVersion = UniGLTFVersion.VERSION;
            window.ShowUtility();
        }
    }
}
