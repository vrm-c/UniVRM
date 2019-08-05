using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UniJSON;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class SerializerGenerator
    {
        const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "VRM/UniGLTF/Scripts/IO/FormatterExtensionsGltf.g.cs");
            }
        }

        /// <summary>
        /// AOT向けにシリアライザを生成する
        /// </summary>
        [MenuItem(VRM.VRMVersion.MENU + "/Generate Serializer")]
        static void GenerateSerializer()
        {
            var path = OutPath;

            using (var g = new Generator(path))
            {
                var rootType = typeof(glTF);
                g.Generate(rootType, "gltf");
            }
        }

        class Generator : IDisposable
        {
            String m_path;
            Stream m_s;
            StreamWriter m_w;

            static Dictionary<Type, string> snipets = new Dictionary<Type, string>
            {

            };

            public Generator(string path)
            {
                m_path = path;
                m_s = File.Open(path, FileMode.Create);
                m_w = new StreamWriter(m_s, Encoding.UTF8);

                // begin
                m_w.Write(@"
using System;
using System.Collections.Generic;
using UniJSON;
using UnityEngine;
using VRM;

namespace UniGLTF {

    static public class IFormatterExtensionsGltf
    {

");
            }

            public void Dispose()
            {
                // end
                m_w.Write(@"
    } // class
} // namespace
");

                m_w.Dispose();
                m_s.Dispose();
                UnityPath.FromFullpath(m_path).ImportAsset();
            }

            HashSet<Type> m_used = new HashSet<Type>
            {
                typeof(object),
            };

            public void Generate(Type t, string path, int level = 0)
            {
                if (m_used.Contains(t))
                {
                    // 処理済み
                    return;
                }
                m_used.Add(t);

                // primitive
                try
                {
                    var mi = typeof(IFormatter).GetMethod("Value", new Type[] { t });
                    if (mi != null)
                    {
                        m_w.Write(@"
    public static void GenSerialize(this IFormatter f, $0 value)
    {
        f.Value(value);
    }
".Replace("$0", t.Name));

                        return;
                    }
                }
                catch (AmbiguousMatchException)
                {
                    // skip
                }

                if (t.IsEnum)
                {
                    m_w.Write(@"
    public static void GenSerialize(this IFormatter f, $0 value)
    {
        f.Value((int)value);
    }
".Replace("$0", t.Name));
                }
                else if (t.IsArray)
                {
                    var et = t.GetElementType();
                    m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, $0[] value)
    {
        if (value == null) return;
        f.BeginList(value.Length);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
                    "
                    .Replace("$0", et.Name)
                    .Replace("$1", path)
                    );
                    Generate(et, path + "[]", level + 1);
                }
                else if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var et = t.GetGenericArguments()[0];
                        m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, List<$0> value)
    {
        if (value == null) return;
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
"
.Replace("$0", et.Name)
.Replace("$1", path));
                        Generate(et, path + "[]", level + 1);
                    }
                    else if (t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                    && t.GetGenericArguments()[0] == typeof(string))
                    {
                        var et = t.GetGenericArguments()[1];
                        m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, Dictionary<string, $0> value)
    {
        if (value == null) return;
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }

"
.Replace("$0", et.Name)
.Replace("$1", path));
                        Generate(et, path + "{}", level + 1);
                    }
                    else
                    {
                        Debug.LogWarningFormat("unknown type: {0}", t);
                    }
                }
                else
                {
                    Debug.LogFormat("{0}({1})", path, t.Name);

                    m_w.Write(@"
    /// $1
    public static void GenSerialize(this IFormatter f, $0 value)
    {
        f.BeginMap(0); // dummy
"
.Replace("$0", t.Name)
.Replace("$1", path)
);

                    foreach (var fi in t.GetFields(FIELD_FLAGS))
                    {
                        if (fi.FieldType == typeof(object))
                        {
                            continue;
                        }
                        if (fi.IsLiteral && !fi.IsInitOnly)
                        {
                            continue;
                        }

                        var snipet = fi.FieldType.IsClass ? "if(value." + fi.Name + "!=null)" : "";

                        m_w.Write(@"
        $1
        {
            f.Key(""$0""); f.GenSerialize(value.$0);
        }
"
.Replace("$0", fi.Name)
.Replace("$1", snipet)
);
                    }

                    m_w.Write(@"
        f.EndMap();
    }
");

                    foreach (var fi in t.GetFields(FIELD_FLAGS))
                    {
                        if (fi.FieldType == typeof(object))
                        {
                            continue;
                        }
                        if (fi.IsLiteral && !fi.IsInitOnly)
                        {
                            continue;
                        }

                        Generate(fi.FieldType, path + "/" + fi.Name, level + 1);
                    }
                }
            }
        }
    }
}