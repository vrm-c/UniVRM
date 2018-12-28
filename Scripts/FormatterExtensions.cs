using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static partial class FormatterExtensions
    {
        public static void Clear(this IFormatter f)
        {
            f.GetStore().Clear();
        }

        public static ArraySegment<Byte> GetStoreBytes(this IFormatter f)
        {
            return f.GetStore().Bytes;
        }

        public static void Key(this IFormatter f, string x)
        {
            f.Key(Utf8String.From(x));
        }

        public static void Value(this IFormatter f, IEnumerable<byte> raw, int count)
        {
            f.Value(new ArraySegment<byte>(raw.Take(count).ToArray()));
        }

        public static void Value(this IFormatter f, Byte[] bytes)
        {
            f.Value(new ArraySegment<Byte>(bytes));
        }

        public static void Value(this IFormatter f, UnityEngine.Vector3 v)
        {
            //CommaCheck();
            f.BeginMap(3);
            f.Key("x"); f.Value(v.x);
            f.Key("y"); f.Value(v.y);
            f.Key("z"); f.Value(v.z);
            f.EndMap();
        }

        static MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            var method = typeof(FormatterExtensions).GetMethod("Serialize");
            return method.MakeGenericMethod(typeof(T));
        }

        public static void KeyValue<T>(this IFormatter f, Expression<Func<T>> expression)
        {
            var func = expression.Compile();
            var value = func();
            if (value != null)
            {
                var body = expression.Body as MemberExpression;
                if (body == null)
                {
                    body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
                }
                f.Key(body.Member.Name);
                f.Serialize(expression.Compile()());
                //var method = GetMethod(expression);
                //method.Invoke(this, new object[] { value });
            }
        }
    }
}
