using System.IO;
using UnityEditor;


namespace VRM
{
    public static class VRMVersionMenu
    {
        const string path = "Assets/VRM/Scripts/Format/VRMVersion.cs";
        const string template = @"
namespace VRM
{{
    public static partial class VRMVersion
    {{
        public const int MAJOR = {0};
        public const int MINOR = {1};

        public const string VERSION = ""{0}.{1}"";
    }}
}}
";

#if VRM_DEVELOP
        [MenuItem(VRMVersion.VRM_VERSION+"/Increment")]
#endif
        public static void IncrementVersion()
        {
            var source = string.Format(template, VRMVersion.MAJOR, VRMVersion.MINOR + 1);
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.VRM_VERSION+"/Decrement")]
#endif
        public static void DecrementVersion()
        {
            var source = string.Format(template, VRMVersion.MAJOR, VRMVersion.MINOR - 1);
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }
    }
}
