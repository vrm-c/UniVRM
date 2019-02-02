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
        const int NET35MAX = 4;
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
                w.WriteLine("//////////// CreateAction");

                // Create
                for (int i = 1; i <= ARGS && i< NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());



                    var source = @"
        public static Action<S, $0> CreateAction<S, $0>(MethodInfo m)
        {
            Action<S, $0> callback = null;
            if (m.IsStatic)
            {
                callback = (s, $1) =>
                {
                    m.Invoke(null, new object[] { s, $1 });
                };
            }
            else
            {
                callback = (s, $1) =>
                {
                    m.Invoke(s, new object[] { $1 });
                };
            }
            return callback;
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }


                // CreateWithThis
                w.WriteLine("//////////// CreateWithThis");
                for (int i = 1; i <= ARGS && i<= NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Action<$0> CreateActionBindThis<S, $0>(MethodInfo m, S instance)
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

        #region zero arguments
        public static Action<S> CreateAction<S>(MethodInfo m)
        {
            return (s) =>
            {
                m.Invoke(s, new object[] { });
            };
        }

        public static Func<S, T> CreateFunc<S, T>(MethodInfo m)
        {
            return (s) =>
            {
                return (T)m.Invoke(s, new object[] { });
            };
        }

        public static Action CreateActionBindThis<S>(MethodInfo m, S instance)
        {
            return () =>
            {
                m.Invoke(instance, new object[] { });
            };
        }
        #endregion
    }
}
