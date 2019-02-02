
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericExpressionCallFactory
    {


        public static Action<S, A0> Create<S, A0>(MethodInfo m)
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
                (Action<S, A0>)Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


        public static Action<S, A0, A1> Create<S, A0, A1>(MethodInfo m)
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
                (Action<S, A0, A1>)Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


        public static Action<S, A0, A1, A2> Create<S, A0, A1, A2>(MethodInfo m)
        {
            var self = Expression.Parameter(m.DeclaringType, m.Name);
            var args = m.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            var call = Expression.Call(self, m, args);
            return 
                (Action<S, A0, A1, A2>)Expression.Lambda(call, new[] { self }.Concat(args).ToArray()).Compile();
        }


        public static Action<A0> CreateWithThis<S, A0>(MethodInfo m, S instance)
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
                (Action<A0>)Expression.Lambda(call, args).Compile();
        }


        public static Action<A0, A1> CreateWithThis<S, A0, A1>(MethodInfo m, S instance)
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
                (Action<A0, A1>)Expression.Lambda(call, args).Compile();
        }


        public static Action<A0, A1, A2> CreateWithThis<S, A0, A1, A2>(MethodInfo m, S instance)
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
                (Action<A0, A1, A2>)Expression.Lambda(call, args).Compile();
        }


        public static Action<A0, A1, A2, A3> CreateWithThis<S, A0, A1, A2, A3>(MethodInfo m, S instance)
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
                (Action<A0, A1, A2, A3>)Expression.Lambda(call, args).Compile();
        }


    }
}

