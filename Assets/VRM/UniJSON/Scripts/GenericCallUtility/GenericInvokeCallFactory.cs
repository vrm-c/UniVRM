using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniJSON
{
    public static partial class GenericInvokeCallFactory
    {
#if UNITY_EDITOR && VRM_DEVELOP
        const int ARGS = 6;
        const string GENERATE_PATH = "Assets/VRM/UniJSON/Scripts/GenericCallUtility/GenericInvokeCallFactory.g.cs";

        static System.Collections.Generic.IEnumerable<string> GetArgs(string prefix, int n)
        {
            for (int i = 0; i < n; ++i)
            {
                yield return prefix + i;
            }
        }

        [MenuItem(VRM.VRMVersion.MENU + "/Generate GenericInvokeCallFactory")]
        static void Generate()
        {
            var sb = new StringBuilder();
            using (var w = new StringWriter(sb))
            {
                w.WriteLine(@"
using System;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericInvokeCallFactory
    {
");

                // CreateWithThis
                w.WriteLine("//////////// Create");

                // Create
                for (int i = 1; i <= ARGS; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    if (i >= 4) w.WriteLine(@"
#if UNITY_5
#else
");


                    var source = @"
#if UNITY_5
        public static Delegate Create<S, $0>(MethodInfo m)
#else
        public static Action<S, $0> Create<S, $0>(MethodInfo m)
#endif
        {
            Action<S, $0> callback=
            (s, $1) =>
            {
                m.Invoke(s, new object[] { $1 });
            };
            return callback;
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                    if (i >= 4) w.WriteLine(@"
#endif
");
                }


                // CreateWithThis
                w.WriteLine("//////////// CreateWithThis");
                for (int i = 1; i <= ARGS; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    if (i > 4) w.WriteLine(@"
#if UNITY_5
#else
");

                    var source = @"
#if UNITY_5
        public static Delegate CreateWithThis<S, $0>(MethodInfo m, S instance)
#else
        public static Action<$0> CreateWithThis<S, $0>(MethodInfo m, S instance)
#endif
        {
            if (m.IsStatic)
            {
                if (instance != null)
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                if (instance == null)
                {
                    throw new ArgumentNullException();
                }
            }

            // ToDo: CreateDelegate
            Action<$0> callback=
            ($1) => {
                m.Invoke(instance, new object[]{ $1 });
            };
            return callback;
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                    if (i > 4) w.WriteLine(@"
#endif
");
                }


                w.WriteLine(@"
    }
}
");
            }

            var path = UniGLTF.UnityPath.FromUnityPath(GENERATE_PATH);
            File.WriteAllText(path.FullPath, sb.ToString().Replace("\r\n", "\n"));
            path.ImportAsset();
        }
#endif

        #region no arguments
        public static Action<S> Create<S>(MethodInfo m)
        {
            return (s) =>
            {
                m.Invoke(s, new object[] { });
            };
        }

        public static Action CreateWithThis<S>(MethodInfo m, S instance)
        {
            return () =>
            {
                m.Invoke(instance, new object[] { });
            };
        }
        #endregion
    }
}
