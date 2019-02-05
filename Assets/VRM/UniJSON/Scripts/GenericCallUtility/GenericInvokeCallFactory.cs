using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR && VRM_DEVELOP
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
#endif


namespace UniJSON
{
    /// <summary>
    /// MethodInfoからDelegateを作成する
    /// 
    /// * StaticAction/Func StaticMethod呼び出し
    /// * OpenAction/Func 第1引数にthisを受けるメソッド呼び出し
    /// * BindAction/Func thisを内部に保持したメソッド呼び出し
    /// 
    /// </summary>
    public static partial class GenericInvokeCallFactory
    {
#if UNITY_EDITOR && VRM_DEVELOP
        const int NET35MAX = 4;
        const int ARGS = 6;
        const string GENERATE_PATH = "/VRM/UniJSON/Scripts/GenericCallUtility/GenericInvokeCallFactory.g.cs";

        static System.Collections.Generic.IEnumerable<string> GetArgs(string prefix, int n)
        {
            for (int i = 0; i < n; ++i)
            {
                yield return prefix + i;
            }
        }

        [MenuItem("VRM/UniJSON/Generate GenericInvokeCallFactory")]
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

                // StaticAction
                w.WriteLine("//////////// StaticAction");
                for (int i = 1; i <= ARGS && i <= NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Action<$0> StaticAction<$0>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format(""{0} is not static"", m));
            }

            return (Action<$0>)Delegate.CreateDelegate(typeof(Action<$0>), null, m);
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }

                // OpenAction
                w.WriteLine("//////////// OpenAction");
                for (int i = 1; i <= ARGS && i < NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Action<S, $0> OpenAction<S, $0>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format(""{0} is static"", m));
            }

            return (Action<S, $0>)Delegate.CreateDelegate(typeof(Action<S, $0>), m);
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }

                // BindAction
                w.WriteLine("//////////// BindAction");
                for (int i = 1; i <= ARGS && i <= NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Action<$0> BindAction<S, $0>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format(""{0} is static"", m));
            }

            return (Action<$0>)Delegate.CreateDelegate(typeof(Action<$0>), instance, m);
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }

                // StaticFunc
                w.WriteLine("//////////// StaticFunc");
                for (int i = 1; i <= ARGS && i <= NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Func<$0, T> StaticFunc<$0, T>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format(""{0} is not static"", m));
            }

            return (Func<$0, T>)Delegate.CreateDelegate(typeof(Func<$0, T>), null, m);
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }

                // OpenFunc
                w.WriteLine("//////////// OpenFunc");
                for (int i = 1; i <= ARGS && i < NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Func<S, $0, T> OpenFunc<S, $0, T>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format(""{0} is static"", m));
            }

            return (Func<S, $0, T>)Delegate.CreateDelegate(typeof(Func<S, $0, T>), m);
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }

                // BindFunc
                w.WriteLine("//////////// BindFunc");
                for (int i = 1; i <= ARGS && i <= NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Func<$0, T> BindFunc<S, $0, T>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format(""{0} is static"", m));
            }

            return (Func<$0, T>)Delegate.CreateDelegate(typeof(Func<$0, T>), instance, m);
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);

                }


                w.WriteLine(@"
    }
}
");
            }

            var path = Path.GetFullPath(Application.dataPath + GENERATE_PATH).Replace("\\", "/");
            File.WriteAllText(path, sb.ToString().Replace("\r\n", "\n"));
        }
#endif

        #region Action without arguments
        public static Action StaticAction(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (Action)Delegate.CreateDelegate(typeof(Action), null, m);
        }

        public static Action<S> OpenAction<S>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s) =>
            {
                m.Invoke(s, new object[] { });
            };
        }

        public static Action BindAction<S>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return () =>
            {
                m.Invoke(instance, new object[] { });
            };
        }
        #endregion

        #region Func without arguments
        public static Func<T> StaticFunc<T>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return () => (T)m.Invoke(null, new object[] { });
        }

        public static Func<S, T> OpenFunc<S, T>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s) => (T)m.Invoke(s, new object[] { });
        }

        public static Func<T> BindFunc<S, T>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return () => (T)m.Invoke(instance, new object[] { });
        }
        #endregion
    }
}
