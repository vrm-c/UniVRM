
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericExpressionCallFactory
    {


#if UNITY_5
        public static Delegate Create<S, A0>(MethodInfo m)
#else
        public static Action<S, A0> Create<S, A0>(MethodInfo m)
#endif
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
#if UNITY_5
#else
                (Action<S, A0>)
#endif
                Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1>(MethodInfo m)
#else
        public static Action<S, A0, A1> Create<S, A0, A1>(MethodInfo m)
#endif
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
#if UNITY_5
#else
                (Action<S, A0, A1>)
#endif
                Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2> Create<S, A0, A1, A2>(MethodInfo m)
#endif
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
#if UNITY_5
#else
                (Action<S, A0, A1, A2>)
#endif
                Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2, A3>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2, A3> Create<S, A0, A1, A2, A3>(MethodInfo m)
#endif
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
#if UNITY_5
#else
                (Action<S, A0, A1, A2, A3>)
#endif
                Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2, A3, A4>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2, A3, A4> Create<S, A0, A1, A2, A3, A4>(MethodInfo m)
#endif
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
#if UNITY_5
#else
                (Action<S, A0, A1, A2, A3, A4>)
#endif
                Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2, A3, A4, A5>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2, A3, A4, A5> Create<S, A0, A1, A2, A3, A4, A5>(MethodInfo m)
#endif
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
#if UNITY_5
#else
                (Action<S, A0, A1, A2, A3, A4, A5>)
#endif
                Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


#if UNITY_5
        public static Delegate CreateWithThis<S, A0>(MethodInfo m, S instance)
#else
        public static Action<A0> CreateWithThis<S, A0>(MethodInfo m, S instance)
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
#if UNITY_5
#else
                (Action<A0>)
#endif
                Expression.Lambda(call, args).Compile();
        }


#if UNITY_5
        public static Delegate CreateWithThis<S, A0, A1>(MethodInfo m, S instance)
#else
        public static Action<A0, A1> CreateWithThis<S, A0, A1>(MethodInfo m, S instance)
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
#if UNITY_5
#else
                (Action<A0, A1>)
#endif
                Expression.Lambda(call, args).Compile();
        }


#if UNITY_5
        public static Delegate CreateWithThis<S, A0, A1, A2>(MethodInfo m, S instance)
#else
        public static Action<A0, A1, A2> CreateWithThis<S, A0, A1, A2>(MethodInfo m, S instance)
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
#if UNITY_5
#else
                (Action<A0, A1, A2>)
#endif
                Expression.Lambda(call, args).Compile();
        }


#if UNITY_5
        public static Delegate CreateWithThis<S, A0, A1, A2, A3>(MethodInfo m, S instance)
#else
        public static Action<A0, A1, A2, A3> CreateWithThis<S, A0, A1, A2, A3>(MethodInfo m, S instance)
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
#if UNITY_5
#else
                (Action<A0, A1, A2, A3>)
#endif
                Expression.Lambda(call, args).Compile();
        }


#if UNITY_5
        public static Delegate CreateWithThis<S, A0, A1, A2, A3, A4>(MethodInfo m, S instance)
#else
        public static Action<A0, A1, A2, A3, A4> CreateWithThis<S, A0, A1, A2, A3, A4>(MethodInfo m, S instance)
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
#if UNITY_5
#else
                (Action<A0, A1, A2, A3, A4>)
#endif
                Expression.Lambda(call, args).Compile();
        }


#if UNITY_5
        public static Delegate CreateWithThis<S, A0, A1, A2, A3, A4, A5>(MethodInfo m, S instance)
#else
        public static Action<A0, A1, A2, A3, A4, A5> CreateWithThis<S, A0, A1, A2, A3, A4, A5>(MethodInfo m, S instance)
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
#if UNITY_5
#else
                (Action<A0, A1, A2, A3, A4, A5>)
#endif
                Expression.Lambda(call, args).Compile();
        }


    }
}

