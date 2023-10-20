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
    /// v0.81.0: com.vrmc.unigltf to com.vrmc.gltf and same version with univrm.
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
        const string VrmVersionPath = "Assets/UniGLTF/Runtime/UniGLTF/PackageVersion.cs";
        const string VrmVersionTemplate = @"
namespace UniGLTF
{{
    public static partial class PackageVersion
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
            public readonly string Path;
            public readonly string Template;

            public UpmPackage(string path, string template)
            {
                Path = path;
                Template = template;
            }
        }

        UpmPackage[] Packages = new UpmPackage[]
        {
            new UpmPackage("Assets/VRMShaders/package.json",
@"{{
  ""name"": ""com.vrmc.vrmshaders"",
  ""version"": ""{1}"",
  ""displayName"": ""VRM Shaders"",
  ""description"": ""VRM Shaders"",
  ""unity"": ""2021.3"",
  ""keywords"": [
    ""vrm"",
    ""shader""
  ],
  ""author"": {{
    ""name"": ""VRM Consortium""
  }},
  ""dependencies"": {{
    ""com.unity.modules.imageconversion"": ""1.0.0""
  }}
}}
"),
            new UpmPackage("Assets/VRM/package.json",
@"{{
  ""name"": ""com.vrmc.univrm"",
  ""version"": ""{1}"",
  ""displayName"": ""VRM"",
  ""description"": ""VRM importer"",
  ""unity"": ""2021.3"",
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
    ""com.vrmc.gltf"": ""{0}"",
    ""com.unity.ugui"": ""1.0.0""
  }},
  ""samples"": [
    {{
      ""displayName"": ""SimpleViewer"",
      ""description"": ""VRM runtime loader sample"",
      ""path"": ""Samples~/SimpleViewer""
    }},
    {{
      ""displayName"": ""FirstPersonSample"",
      ""description"": ""First Person layer sample with multi camera"",
      ""path"": ""Samples~/FirstPersonSample""
    }},
    {{
      ""displayName"": ""RuntimeExporterSample"",
      ""description"": ""VRM runtime exporter sample"",
      ""path"": ""Samples~/RuntimeExporterSample""
    }},
    {{
      ""displayName"": ""AnimationBridgeSample"",
      ""description"": ""BlendShape animation clip sample"",
      ""path"": ""Samples~/AnimationBridgeSample""
    }}
  ]
}}
"),

            new UpmPackage("Assets/VRM10/package.json",
@"{{
  ""name"": ""com.vrmc.vrm"",
  ""version"": ""{1}"",
  ""displayName"": ""VRM-1.0"",
  ""description"": ""VRM-1.0 importer"",
  ""unity"": ""2021.3"",
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
    ""com.vrmc.gltf"": ""{0}""
  }},
  ""samples"": [
    {{
      ""displayName"": ""VRM10Viewer"",
      ""description"": ""VRM10 runtime loader sample"",
      ""path"": ""Samples~/VRM10Viewer""
    }},
    {{
      ""displayName"": ""VRM10FirstPersonSample"",
      ""description"": ""First Person layer sample with multi camera"",
      ""path"": ""Samples~/VRM10FirstPersonSample""
    }}
  ]
}}
"),

        };

        UpmPackage UniGLTFPackage = new UpmPackage("Assets/UniGLTF/package.json",
@"{{
  ""name"": ""com.vrmc.gltf"",
  ""version"": ""{0}"",
  ""displayName"": ""UniGLTF"",
  ""description"": ""GLTF importer and exporter"",
  ""unity"": ""2021.3"",
  ""keywords"": [
    ""gltf""
  ],
  ""author"": {{
    ""name"": ""VRM Consortium""
  }},
  ""dependencies"": {{
    ""com.vrmc.vrmshaders"": ""{1}"",
    ""com.unity.modules.animation"": ""1.0.0""
  }},
  ""samples"": [
    {{
      ""displayName"": ""GltfViewer"",
      ""description"": ""UniGLTF runtime loader sample"",
      ""path"": ""Samples~/GltfViewer""
    }}
  ]
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
            GUILayout.Label($"Current version: {PackageVersion.VERSION}");
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
                    UpdateVrmVersion(vrmVersion);
                    UpdateUniGLTFVersion(m_uniGltfVersion, vrmVersion);
                    AssetDatabase.Refresh();
                    Debug.Log($"{m_uniGltfVersion}, {vrmVersion}");
                }
                else
                {
                    Debug.LogWarning($"InvalidFormat: {m_vrmVersion}");
                }

                // COPY
                VRMSampleCopy.Execute();
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
                $"{vrm.Item1}.{vrm.Item2}.{vrm.Item3}",
                $"{vrm.Item1}.{vrm.Item2}.{vrm.Item3}"
                ), utf8);
        }

        void UpdateVrmVersion((int, int, int) vrm)
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
                    $"{vrm.Item1}.{vrm.Item2}.{vrm.Item3}",
                    $"{vrm.Item1}.{vrm.Item2}.{vrm.Item3}"
                    ), utf8);
            }
        }

        public static void ShowVersionDialog()
        {
            var window = ScriptableObject.CreateInstance<VRMVersionMenu>();
            window.m_vrmVersion = PackageVersion.VERSION;
            // window.m_uniGltfVersion = UniGLTFVersion.VERSION;
            window.ShowUtility();
        }
    }
}
