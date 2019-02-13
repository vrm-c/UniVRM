using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UniGLTF;
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
            var f = new JsonFormatter();
");

                    foreach (var t in TraverseType(typeof(glTF), new List<Type>
                {
                    typeof(object),
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

                    typeof(Vector3),
                }))
                    {
                        var typeName = t.Name;
                        var listType = GetGenericListType(t);
                        if (listType != null)
                        {
                            typeName = "List<$0>".Replace("$0", listType.Name);
                        }
                        var dictType = GetDictionaryValueType(t);
                        if (dictType != null)
                        {
                            typeName = "Dictionary<string, $0>".Replace("$0", dictType.Name);
                        }

                        w.WriteLine("f.Serialize(default($0));".Replace("$0", typeName));
                        w.WriteLine(@"{
    var value = default($0);
    default(ListTreeNode<JsonValue>).Deserialize(ref value);
}".Replace("$0", typeName));
                        w.WriteLine();
                    }

                    w.WriteLine(@"
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

        static Type GetGenericListType(Type t)
        {
            if(t.IsGenericType 
                && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                return t.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }

        static Type GetDictionaryValueType(Type t)
        {
            if (t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && t.GetGenericArguments()[0] == typeof(string))
            {
                return t.GetGenericArguments()[1];
            }
            else
            {
                return null;
            }
        }

        static IEnumerable<Type> TraverseType(Type t, List<Type> excludes)
        {
            if (excludes.Contains(t))
            {
                yield break;
            }

            Debug.LogFormat("{0}", t);
            excludes.Add(t);
            yield return t;

            var listType = GetGenericListType(t);
            if (listType!=null)
            {
                foreach (var x in TraverseType(listType, excludes))
                {
                    yield return x;
                }
            }
            else
            {
                foreach (var fi in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    foreach (var x in TraverseType(fi.FieldType, excludes))
                    {
                        yield return x;
                    }
                }
            }
        }
    }
}
