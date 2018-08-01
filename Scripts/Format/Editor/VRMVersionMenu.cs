using System.IO;
using System.Text;
using UniGLTF;
using UniJSON;
using UnityEditor;
using UnityEngine;

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
        [MenuItem(VRMVersion.VRM_VERSION + "/Increment")]
#endif
        static void IncrementVersion()
        {
            var source = string.Format(template, VRMVersion.MAJOR, VRMVersion.MINOR + 1);
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.VRM_VERSION + "/Decrement")]
#endif
        static void DecrementVersion()
        {
            var source = string.Format(template, VRMVersion.MAJOR, VRMVersion.MINOR - 1);
            File.WriteAllText(path, source);
            AssetDatabase.Refresh();
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.VRM_VERSION + "/Export JsonSchema")]
#endif
        static void ExportJsonSchema()
        {
            var dir = UnityPath.FromFullpath(Application.dataPath + "/VRM/specification/0.0/schema");
            dir.EnsureFolder();

            var path = dir.Child("vrm.schema.json");

            Debug.LogFormat("write SsonSchema: {0}", path.FullPath);
            var schema = JsonSchema.FromType<glTF_VRM_extensions>();
            var f = new JsonFormatter();
            schema.ToJson(f);
            var json = f.ToString();
            File.WriteAllText(path.FullPath, json, Encoding.UTF8);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path.Value);
        }
    }
}
