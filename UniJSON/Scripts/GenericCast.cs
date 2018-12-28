using System;
using System.Linq.Expressions;


namespace UniJSON
{
    struct GenericCast<S, T>
    {
        public static T Null()
        {
            if (typeof(T).IsClass)
            {
                return default(T);
            }
            else
            {
                throw new MsgPackTypeException("can not null");
            }
        }

        delegate T CastFunc(S value);
        static CastFunc s_cast;

        delegate Func<T> ConstFuncCreator(S value);
        static ConstFuncCreator s_const;

        public static Func<T> Const(S value)
        {
            if (s_const == null)
            {
                s_const = new ConstFuncCreator(GenericCast.CreateConst<S, T>());
            }
            return s_const(value);
        }

        public static T Cast(S value)
        {
            if (s_cast == null)
            {
                s_cast = new CastFunc(GenericCast.CreateCast<S, T>());
            }
            return s_cast(value);
        }
    }

    static class GenericCast
    {
        public static Func<S, T> CreateCast<S, T>()
        {
            if (typeof(S) == typeof(T))
            {
                // through
                var src = Expression.Parameter(typeof(S), "src");
                var lambda = Expression.Lambda(src, src);
                return (Func<S, T>)lambda.Compile();
            }
            else
            {
                // cast
                var src = Expression.Parameter(typeof(S), "src");
                var cast = Expression.Convert(src, typeof(T));
                var lambda = Expression.Lambda(cast, src);
                return (Func<S, T>)lambda.Compile();
            }
        }

        public static Func<S, Func<T>> CreateConst<S, T>()
        {
            var src = Expression.Parameter(typeof(S), "src");
            var convert = Expression.Convert(src, typeof(T));
            var lambda = (Func<S, T>)Expression.Lambda(convert, src).Compile();
            return s =>
            {
                var t = lambda(s);
                return () => t;
            };
        }
    }
}
