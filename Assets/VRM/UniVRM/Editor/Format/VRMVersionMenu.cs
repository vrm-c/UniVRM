using System.IO;
using UnityEditor;


namespace VRM
{
    public static class VRMVersionMenu
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
        public const string PRE_ID = ""{3}"";

        public const string VERSION = ""{0}.{1}.{2}{4}"";
    }}
}}
";

#if VRM_DEVELOP
        [MenuItem(VRMVersion.MENU + "/Increment")]
#endif
        static void IncrementVersion()
        {
            var source = string.Format(
                template,
                VRMVersion.MAJOR,
                VRMVersion.MINOR + 1,
                VRMVersion.PATCH,
                VRMVersion.PRE_ID,
                VRMVersion.PRE_ID != "" ? string.Format("-{0}", VRMVersion.PRE_ID) : ""
                );
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.MENU + "/Decrement")]
#endif
        static void DecrementVersion()
        {
            var source = string.Format(
                template,
                VRMVersion.MAJOR,
                VRMVersion.MINOR - 1,
                VRMVersion.PATCH,
                VRMVersion.PRE_ID,
                VRMVersion.PRE_ID != "" ? string.Format("-{0}", VRMVersion.PRE_ID) : ""
                );
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }
    }
}
