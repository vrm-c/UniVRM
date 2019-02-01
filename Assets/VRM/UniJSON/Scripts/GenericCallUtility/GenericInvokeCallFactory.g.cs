
using System;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericInvokeCallFactory
    {

//////////// Create

        public static Action<S, A0> Create<S, A0>(MethodInfo m)
        {
            Action<S, A0> callback = null;
            if (m.IsStatic)
            {
                callback = (s, a0) =>
                {
                    m.Invoke(null, new object[] { s, a0 });
                };
            }
            else
            {
                callback = (s, a0) =>
                {
                    m.Invoke(s, new object[] { a0 });
                };
            }
            return callback;
        }


        public static Action<S, A0, A1> Create<S, A0, A1>(MethodInfo m)
        {
            Action<S, A0, A1> callback = null;
            if (m.IsStatic)
            {
                callback = (s, a0, a1) =>
                {
                    m.Invoke(null, new object[] { s, a0, a1 });
                };
            }
            else
            {
                callback = (s, a0, a1) =>
                {
                    m.Invoke(s, new object[] { a0, a1 });
                };
            }
            return callback;
        }


        public static Action<S, A0, A1, A2> Create<S, A0, A1, A2>(MethodInfo m)
        {
            Action<S, A0, A1, A2> callback = null;
            if (m.IsStatic)
            {
                callback = (s, a0, a1, a2) =>
                {
                    m.Invoke(null, new object[] { s, a0, a1, a2 });
                };
            }
            else
            {
                callback = (s, a0, a1, a2) =>
                {
                    m.Invoke(s, new object[] { a0, a1, a2 });
                };
            }
            return callback;
        }

//////////// CreateWithThis

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

            // ToDo: CreateDelegate
            Action<A0> callback=
            (a0) => {
                m.Invoke(instance, new object[]{ a0 });
            };
            return callback;
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

            // ToDo: CreateDelegate
            Action<A0, A1> callback=
            (a0, a1) => {
                m.Invoke(instance, new object[]{ a0, a1 });
            };
            return callback;
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

            // ToDo: CreateDelegate
            Action<A0, A1, A2> callback=
            (a0, a1, a2) => {
                m.Invoke(instance, new object[]{ a0, a1, a2 });
            };
            return callback;
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

            // ToDo: CreateDelegate
            Action<A0, A1, A2, A3> callback=
            (a0, a1, a2, a3) => {
                m.Invoke(instance, new object[]{ a0, a1, a2, a3 });
            };
            return callback;
        }


    }
}

