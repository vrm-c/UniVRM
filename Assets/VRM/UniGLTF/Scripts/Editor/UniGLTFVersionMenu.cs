#if VRM_DEVELOP
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;


namespace UniGLTF
{
    public static class UniGLTFVersionMenu
    {
        public const int MENU_PRIORITY = 99;
        static string path = "Assets/UniGLTF/Core/Scripts/UniGLTFVersion.cs";

        const string template = @"
namespace UniGLTF
{{
    public static partial class UniGLTFVersion
    {{
        public const int MAJOR = {0};
        public const int MINOR = {1};

        public const string VERSION = ""{0}.{1}"";
    }}
}}
";

        [MenuItem(UniGLTFVersion.MENU + "/Increment", priority = MENU_PRIORITY)]
        public static void IncrementVersion()
        {
            var source = string.Format(template, UniGLTFVersion.MAJOR, UniGLTFVersion.MINOR + 1);
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }

        [MenuItem(UniGLTFVersion.MENU + "/Decrement", priority = MENU_PRIORITY)]
        public static void DecrementVersion()
        {
            var source = string.Format(template, UniGLTFVersion.MAJOR, UniGLTFVersion.MINOR - 1);
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }
    }
}
#endif
