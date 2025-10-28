using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static partial class FormatterExtensions
    {
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

#if UNITY_5_6_OR_NEWER
        public static void Value(this IFormatter f, UnityEngine.Vector2 v)
        {
            //CommaCheck();
            f.BeginMap(2);
            f.Key("x"); f.Value(v.x);
            f.Key("y"); f.Value(v.y);
            f.EndMap();
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

        public static void Value(this IFormatter f, UnityEngine.Vector4 v)
        {
            //CommaCheck();
            f.BeginMap(4);
            f.Key("x"); f.Value(v.x);
            f.Key("y"); f.Value(v.y);
            f.Key("z"); f.Value(v.z);
            f.Key("w"); f.Value(v.w);
            f.EndMap();
        }
#endif

        static MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            var method = typeof(FormatterExtensions).GetMethod("Serialize");
            return method.MakeGenericMethod(typeof(T));
        }
    }
}
