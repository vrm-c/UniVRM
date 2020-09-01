using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public class VRMVersionMenu : EditorWindow
    {
        const string VersionPath = "Assets/VRM/UniVRM/Scripts/Format/VRMVersion.cs";
        const string VersionTemplate = @"
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

        const string VRMShadersPackagePath = "Assets/VRMShaders/package.json";
        const string VRMShadersPackageTemplate = @"{{
  ""name"": ""com.vrmc.vrmshaders"",
  ""version"": ""{0}.{1}.{2}"",
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
";

        const string MeshUtilityPath = "Assets/MeshUtility/package.json";
        const string MeshUtilityTemplate = @"{{
  ""name"": ""com.vrmc.meshutility"",
  ""version"": ""{0}.{1}.{2}"",
  ""displayName"": ""MeshUtility"",
  ""unity"": ""2018.4"",
  ""description"": ""MeshUtility is a package for mesh separation, etc. \n\nCheck out the latest information here: <https://github.com/vrm-c/UniVRM/tree/master/Assets/MeshUtility>"",
  ""keywords"": [
    ""mesh""
  ],
  ""author"": {{
    ""name"": ""VRM Consortium""
  }}
}}
";

        const string VRMPackagePath = "Assets/VRM/package.json";
        const string VRMPackageTemplate = @"{{
  ""name"": ""com.vrmc.univrm"",
  ""version"": ""{0}.{1}.{2}"",
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
    ""com.vrmc.vrmshaders"": ""{0}.{1}.{2}"",
    ""com.vrmc.meshutility"": ""{0}.{1}.{2}""
  }}
}}
";

        [SerializeField]
        string m_version;

        void OnGUI()
        {
            GUILayout.Label($"Current version: {VRMVersion.VERSION}");

            m_version = EditorGUILayout.TextField("Major.Minor.Patch", m_version);

            if (GUILayout.Button("Apply"))
            {
                if (string.IsNullOrEmpty(m_version))
                {
                    return;
                }
                var splitted = m_version.Split('.');
                if (splitted.Length != 3)
                {
                    Debug.LogWarning($"InvalidFormat: {m_version}");
                    return;
                }
                var values = new int[3];
                for (int i = 0; i < 3; ++i)
                {
                    values[i] = int.Parse(splitted[i]);
                }

                // generate
                var utf8 = new UTF8Encoding(false);
                File.WriteAllText(VersionPath, string.Format(VersionTemplate,
                    values[0],
                    values[1],
                    values[2]), utf8);
                File.WriteAllText(VRMShadersPackagePath, string.Format(VRMShadersPackageTemplate,
                    values[0],
                    values[1],
                    values[2]), utf8);
                File.WriteAllText(MeshUtilityPath, string.Format(MeshUtilityTemplate,
                    values[0],
                    values[1],
                    values[2]), utf8);
                File.WriteAllText(VRMPackagePath, string.Format(VRMPackageTemplate,
                    values[0],
                    values[1],
                    values[2]), utf8);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.MENU + "/VersionDialog")]
#endif
        static void ShowVersionDialog()
        {
            var window = ScriptableObject.CreateInstance<VRMVersionMenu>();
            window.m_version = VRMVersion.VERSION;
            window.ShowUtility();
        }
    }
}
