using System;
using System.IO;
using System.Text;
using System.Reflection;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif


namespace UniJSON
{
    public static partial class ConcreteCast
    {
        public static string GetMethodName(Type src, Type dst)
        {
            return string.Format("Cast{0}To{1}", src.Name, dst.Name);
        }

        public static MethodInfo GetMethod(Type src, Type dst)
        {
            var name = GetMethodName(src, dst);
            var mi = typeof(ConcreteCast).GetMethod(name, 
                BindingFlags.Static | BindingFlags.Public);
            return mi;
        }

#if UNITY_EDITOR

        static Type[] s_castTypes = new Type[]
        {
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
        };

        [MenuItem("UniGLTF/UniJSON/Generate ConcreteCast")]
        public static void GenerateGenericCast()
        {
            var s = new StringBuilder();
            using (var w = new StringWriter(s))
            {
                w.WriteLine(@"
using System;

namespace UniJSON {
    public static partial class ConcreteCast
    {
");
                foreach (var x in s_castTypes)
                {
                    foreach (var y in s_castTypes)
                    {
                        w.WriteLine(@"
        public static $1 $2($0 src)
        {
            return ($1)src;
        }
".Replace("$0", x.Name).Replace("$1", y.Name).Replace("$2", GetMethodName(x, y)));
                    }
                }
                w.WriteLine(@"
    }
}
");
            }

            var path = Application.dataPath + SOURCE;
            Debug.LogFormat("{0}", path);
            File.WriteAllText(path, s.ToString().Replace("\r\n", "\n"), new UTF8Encoding(false));
            AssetDatabase.ImportAsset("Assets" + SOURCE);
        }
        const string SOURCE = "/VRM/UniJSON/Scripts/ConcreteCast.g.cs";
#endif
    }
}
