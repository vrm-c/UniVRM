using System;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif


namespace UniJSON
{
    public static partial class GenericExpressionCallFactory
    {
#if UNITY_EDITOR && VRM_DEVELOP
        const int NET35MAX = 4;
        const int ARGS = 6;
        const string GENERATE_PATH = "/VRM/UniJSON/Scripts/GenericCallUtility/GenericExpressionCallFactory.g.cs";

        static System.Collections.Generic.IEnumerable<string> GetArgs(string prefix, int n)
        {
            for (int i = 0; i < n; ++i)
            {
                yield return prefix + i;
            }
        }

        [MenuItem("VRM/UniJSON/Generate GenericExpressionCallFactory")]
        static void Generate()
        {
            var sb = new StringBuilder();
            using (var w = new StringWriter(sb))
            {
                w.WriteLine(@"
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericExpressionCallFactory
    {
");
                // Create
                for (int i = 1; i <= ARGS && i<NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());
                    var a = String.Join(", ", GetArgs("a", i).ToArray());

                    var source = @"
        public static Action<S, $0> Create<S, $0>(MethodInfo m)
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
                (Action<S, $0>)Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }
".Replace("$0", g).Replace("$1", a);

                    w.WriteLine(source);
                }

                // CreateWithThis
                for (int i = 1; i <= ARGS && i<=NET35MAX; ++i)
                {
                    var g = String.Join(", ", GetArgs("A", i).ToArray());

                    var source = @"
        public static Action<$0> CreateWithThis<S, $0>(MethodInfo m, S instance)
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

            var self = Expression.Constant(instance, typeof(S)); // thisを定数化
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            MethodCallExpression call;
            if (m.IsStatic)
            {
                call = Expression.Call(m, args);
            }
            else
            {
                call = Expression.Call(self, m, args);
            }
            return 
                (Action<$0>)Expression.Lambda(call, args).Compile();
        }
".Replace("$0", g);

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
    }
}
