using System;
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

        static string GetTitle(JsonNode node)
        {
            try
            {
                var titleNode = node["title"];
                if (titleNode.Value.ValueType == JsonValueType.String)
                {
                    return titleNode.Value.GetString();
                }
            }
            catch(Exception)
            {
            }
            return "";
        }

        static void TraverseItem(JsonNode node, JsonFormatter f, UnityPath dir)
        {
            var title = GetTitle(node);
            if (string.IsNullOrEmpty(title))
            {
                Traverse(node, f, dir);
            }
            else
            {
                // ref
                f.BeginMap();
                f.Key("$ref");
                var fileName = string.Format("{0}.schema.json", title);
                f.Value(fileName);
                f.EndMap();

                // new formatter
                {
                    var subFormatter = new JsonFormatter(4);

                    subFormatter.BeginMap();
                    foreach (var _kv in node.ObjectItems)
                    {
                        subFormatter.Key(_kv.Key);
                        Traverse(_kv.Value, subFormatter, dir);
                    }
                    subFormatter.EndMap();

                    var subJson = subFormatter.ToString();
                    var path = dir.Child(fileName);
                    File.WriteAllText(path.FullPath, subJson);
                }
            }
        }

        static void Traverse(JsonNode node, JsonFormatter f, UnityPath dir)
        {
            switch(node.Value.ValueType)
            {
                case JsonValueType.Array:
                    f.BeginList();
                    foreach(var x in node.ArrayItems)
                    {
                        TraverseItem(x, f, dir);
                    }
                    f.EndList();
                    break;

                case JsonValueType.Object:
                    //Debug.LogFormat("title: {0}", title);
                    {
                        f.BeginMap();
                        foreach (var kv in node.ObjectItems)
                        {
                            f.Key(kv.Key);
                            TraverseItem(kv.Value, f, dir);
                        }
                        f.EndMap();
                    }
                    break;

                default:
                    f.Value(node);
                    break;
            }
        }

        static UnityPath SplitAndWriteJson(JsonNode parsed, UnityPath dir)
        {
            var f = new JsonFormatter(4);
            Traverse(parsed, f, dir);
            var json = f.ToString();
                
            var path = dir.Child("vrm.schema.json");
            Debug.LogFormat("write JsonSchema: {0}", path.FullPath);
            File.WriteAllText(path.FullPath, json);
            return path;
        }

#if VRM_DEVELOP
        [MenuItem(VRMVersion.VRM_VERSION + "/Export JsonSchema")]
#endif
        static void ExportJsonSchema()
        {
            var schema = JsonSchema.FromType<glTF_VRM_extensions>();
            var f = new JsonFormatter(2);
            schema.ToJson(f);
            var json = f.ToString();

            var dir = UnityPath.FromFullpath(Application.dataPath + "/VRM/specification/0.0/schema");
            dir.EnsureFolder();

            var path = SplitAndWriteJson(JsonParser.Parse(json), dir);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path.Value);
        }
    }
}
