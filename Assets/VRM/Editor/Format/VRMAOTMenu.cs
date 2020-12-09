using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UniGLTF;
using UniJSON;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public static class VRMAOTMenu
    {
        /// <summary>
        /// AOT向けにダミーのGenerics呼び出しを作成する
        /// </summary>
#if VRM_DEVELOP
        [MenuItem(VRMVersion.MENU + "/GenerateAOTCall")]
#endif
        static void GenerateAOTCall()
        {
            var path = UnityPath.FromUnityPath("Assets/VRM/UniVRM/Scripts/AOTCall.g.cs");
            var encoding = new UTF8Encoding(false);
            using (var s = new MemoryStream())
            {
                using (var w = new StreamWriter(s, encoding))
                {
                    w.WriteLine(@"
using System;
using UniJSON;
using UniGLTF;
using System.Collections.Generic;


namespace VRM {
    public static partial class VRMAOTCall {
        static void glTF()
        {       
            {       
                var f = new JsonFormatter();
");

                    {
                        var excludes = new List<Type>
                        {
                            typeof(object),
                        };

                        foreach (var t in new Type[]
                        {
                        typeof(string),
                        typeof(bool),

                        typeof(byte),
                        typeof(ushort),
                        typeof(uint),
                        typeof(ulong),

                        typeof(sbyte),
                        typeof(short),
                        typeof(int),
                        typeof(long),

                        typeof(float),
                        typeof(double),
                        typeof(Vector2),
                        typeof(Vector3),
                        typeof(Vector4),
                        typeof(Quaternion),
                        typeof(glTF),
                        })
                        {
                            TraverseType("JsonValue", w, t, excludes);
                        }
                    }

                    w.WriteLine(@"}

{
                var f = new MsgPackFormatter();
");

                    {
                        var excludes = new List<Type>
                        {
                            typeof(object),
                        };

                        foreach (var t in new Type[]
                        {
                        typeof(string),
                        typeof(bool),

                        typeof(byte),
                        typeof(ushort),
                        typeof(uint),
                        typeof(ulong),

                        typeof(sbyte),
                        typeof(short),
                        typeof(int),
                        typeof(long),

                        typeof(float),
                        typeof(double),
                        typeof(Vector2),
                        typeof(Vector3),
                        typeof(Vector4),
                        typeof(Quaternion),
                        })
                        {
                            TraverseType("MsgPackValue", w, t, excludes);
                        }
                    }

                    w.WriteLine(@"
            }
        }
    }
}
");
                }

                var text = encoding.GetString(s.ToArray());
                File.WriteAllText(path.FullPath, text.Replace("\r\n", "\n"), encoding);
            }

            path.ImportAsset();
        }

        static bool IsGenericList(Type t)
        {
            if (t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool IsGenericDictionary(Type t)
        {
            if (t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && t.GetGenericArguments()[0] == typeof(string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static IEnumerable<Type> GetNestedTypes(Type t)
        {
            if (t.DeclaringType == null)
            {
                yield break;
            }

            foreach(var x in GetNestedTypes(t.DeclaringType))
            {
                yield return x;
            }

            yield return t.DeclaringType;
        }

        static string GenericTypeName(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.Name;
            }
            else
            {
                return t.Name.Split('`')[0]
                    + "<"
                    + string.Join(",", t.GetGenericArguments().Select(x => GenericTypeName(x)).ToArray())
                    + ">"
                    ;
            }
        }

        static string GetTypeName(Type t)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(t.Name))
            {
                sb.Append(t.Namespace);
                sb.Append(".");
            }

            foreach(var x in GetNestedTypes(t))
            {
                sb.Append(x.Name);
                sb.Append(".");
            }

            sb.Append(GenericTypeName(t));

            return sb.ToString();
        }

        static void TraverseType(string value, TextWriter w, Type t, List<Type> excludes)
        {
            if (excludes.Contains(t))
            {
                return;
            }

            w.WriteLine();
            w.WriteLine("// $0".Replace("$0", t.Name));
            excludes.Add(t);

            if (t.IsArray)
            {
                var valueType = t.GetElementType();
                w.WriteLine("f.Serialize(default($0[]));".Replace("$0", valueType.Name));
                w.WriteLine(@"{
var value = default($0[]);
default(ListTreeNode<$2>).Deserialize(ref value);
GenericDeserializer<$2, $0[]>.GenericArrayDeserializer<$0>(default(ListTreeNode<$2>));
}"
.Replace("$0", valueType.Name)
.Replace("$2", value)
);

                return;
            }

            {
                // list
                if (IsGenericList(t))
                {
                    var name = GetTypeName(t.GetGenericArguments()[0]);
                    w.WriteLine("f.Serialize(default(List<$0>));".Replace("$0", name));
                    w.WriteLine(@"{
var value = default(List<$0>);
default(ListTreeNode<$2>).Deserialize(ref value);
GenericDeserializer<$2, List<$0>>.GenericListDeserializer<$0>(default(ListTreeNode<$2>));
}"
.Replace("$0", name)
.Replace("$2", value)
);

                    TraverseType(value, w, t.GetGenericArguments()[0], excludes);

                    return;
                }
            }

            {
                // dict
                if (IsGenericDictionary(t))
                {
                    var name = GetTypeName(t.GetGenericArguments()[1]);
                    w.WriteLine("f.Serialize(default(Dictionary<string, $0>));".Replace("$0", name));
                    w.WriteLine(@"{
var value = default(Dictionary<string, $0>);
default(ListTreeNode<$2>).Deserialize(ref value);
GenericDeserializer<$2, Dictionary<string, $0>>.DictionaryDeserializer<$0>(default(ListTreeNode<$2>));
}"
.Replace("$0", name)
.Replace("$2", value)
);

                    TraverseType(value, w, t.GetGenericArguments()[1], excludes);
                    return;
                }
            }

            {
                var name = GetTypeName(t);
                w.WriteLine("f.Serialize(default($0));".Replace("$0", name));
                w.WriteLine(@"{
var value = default($0);
default(ListTreeNode<$2>).Deserialize(ref value);
}"
.Replace("$0", name)
.Replace("$2", value)
);
            }

            // object
            //if (t.IsClass)
            {
                foreach (var fi in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var fieldTypeName = GetTypeName(fi.FieldType);
                    w.WriteLine(@"{
JsonObjectValidator.GenericDeserializer<$2,$0>.DeserializeField<$1>(default(JsonSchema), default(ListTreeNode<$2>));
}"
.Replace("$0", GetTypeName(t))
.Replace("$1", GetTypeName(fi.FieldType))
.Replace("$2", value)
);

                    TraverseType(value, w, fi.FieldType, excludes);
                }
            }
        }
    }
}
