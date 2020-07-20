using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public class VRMVersionMenu : EditorWindow
    {
        const string path = "Assets/VRM/UniVRM/Scripts/Format/VRMVersion.cs";
        const string template = @"
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

                // generate new VRMVersion.cs
                var source = string.Format(
                    template,
                    values[0],
                    values[1],
                    values[2]
                    );
                File.WriteAllText(path, source);
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
