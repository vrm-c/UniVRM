using System;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniJSON
{
    public static partial class ConcreteCast
    {
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

        [MenuItem(UniGLTF.UniGLTFVersion.MENU + "/Generate ConcreteCast")]
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
        public static $1 Cast$0To$1($0 src)
        {
            return ($1)src;
        }
".Replace("$0", x.Name).Replace("$1", y.Name));
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
