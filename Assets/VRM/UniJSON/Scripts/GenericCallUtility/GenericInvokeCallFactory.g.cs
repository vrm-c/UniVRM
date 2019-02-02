
using System;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericInvokeCallFactory
    {

//////////// StaticAction

        public static Action<A0> StaticAction<A0>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0) =>
            {
                m.Invoke(null, new object[] { a0 });
            };
        }


        public static Action<A0, A1> StaticAction<A0, A1>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0, a1) =>
            {
                m.Invoke(null, new object[] { a0, a1 });
            };
        }


        public static Action<A0, A1, A2> StaticAction<A0, A1, A2>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0, a1, a2) =>
            {
                m.Invoke(null, new object[] { a0, a1, a2 });
            };
        }


        public static Action<A0, A1, A2, A3> StaticAction<A0, A1, A2, A3>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0, a1, a2, a3) =>
            {
                m.Invoke(null, new object[] { a0, a1, a2, a3 });
            };
        }

//////////// OpenAction

        public static Action<S, A0> OpenAction<S, A0>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s, a0) =>
            {
                m.Invoke(s, new object[] { a0 });
            };
        }


        public static Action<S, A0, A1> OpenAction<S, A0, A1>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s, a0, a1) =>
            {
                m.Invoke(s, new object[] { a0, a1 });
            };
        }


        public static Action<S, A0, A1, A2> OpenAction<S, A0, A1, A2>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s, a0, a1, a2) =>
            {
                m.Invoke(s, new object[] { a0, a1, a2 });
            };
        }

//////////// BindAction

        public static Action<A0> BindAction<S, A0>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0) =>
            {
                m.Invoke(instance, new object[] { a0 });
            };
        }


        public static Action<A0, A1> BindAction<S, A0, A1>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0, a1) =>
            {
                m.Invoke(instance, new object[] { a0, a1 });
            };
        }


        public static Action<A0, A1, A2> BindAction<S, A0, A1, A2>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0, a1, a2) =>
            {
                m.Invoke(instance, new object[] { a0, a1, a2 });
            };
        }


        public static Action<A0, A1, A2, A3> BindAction<S, A0, A1, A2, A3>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0, a1, a2, a3) =>
            {
                m.Invoke(instance, new object[] { a0, a1, a2, a3 });
            };
        }

//////////// StaticFunc

        public static Func<A0, T> StaticFunc<A0, T>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0) => (T)m.Invoke(null, new object[] { a0 });
        }


        public static Func<A0, A1, T> StaticFunc<A0, A1, T>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0, a1) => (T)m.Invoke(null, new object[] { a0, a1 });
        }


        public static Func<A0, A1, A2, T> StaticFunc<A0, A1, A2, T>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0, a1, a2) => (T)m.Invoke(null, new object[] { a0, a1, a2 });
        }


        public static Func<A0, A1, A2, A3, T> StaticFunc<A0, A1, A2, A3, T>(MethodInfo m)
        {
            if (!m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is not static", m));
            }

            return (a0, a1, a2, a3) => (T)m.Invoke(null, new object[] { a0, a1, a2, a3 });
        }

//////////// OpenFunc

        public static Func<S, A0, T> OpenFunc<S, A0, T>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s, a0) => (T)m.Invoke(s, new object[] { a0 });
        }


        public static Func<S, A0, A1, T> OpenFunc<S, A0, A1, T>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s, a0, a1) => (T)m.Invoke(s, new object[] { a0, a1 });
        }


        public static Func<S, A0, A1, A2, T> OpenFunc<S, A0, A1, A2, T>(MethodInfo m)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (s, a0, a1, a2) => (T)m.Invoke(s, new object[] { a0, a1, a2 });
        }

//////////// BindFunc

        public static Func<A0, T> BindFunc<S, A0, T>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0) => (T)m.Invoke(instance, new object[] { a0 });
        }


        public static Func<A0, A1, T> BindFunc<S, A0, A1, T>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0, a1) => (T)m.Invoke(instance, new object[] { a0, a1 });
        }


        public static Func<A0, A1, A2, T> BindFunc<S, A0, A1, A2, T>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0, a1, a2) => (T)m.Invoke(instance, new object[] { a0, a1, a2 });
        }


        public static Func<A0, A1, A2, A3, T> BindFunc<S, A0, A1, A2, A3, T>(MethodInfo m, S instance)
        {
            if (m.IsStatic)
            {
                throw new ArgumentException(string.Format("{0} is static", m));
            }

            return (a0, a1, a2, a3) => (T)m.Invoke(instance, new object[] { a0, a1, a2, a3 });
        }


    }
}

