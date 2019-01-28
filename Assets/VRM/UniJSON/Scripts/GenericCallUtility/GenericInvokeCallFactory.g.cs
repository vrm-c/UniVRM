
using System;
using System.Reflection;


namespace UniJSON
{
    public static partial class GenericInvokeCallFactory
    {

//////////// Create

#if UNITY_5
        public static Delegate Create<S, A0>(MethodInfo m)
#else
        public static Action<S, A0> Create<S, A0>(MethodInfo m)
#endif
        {
            Action<S, A0> callback=
            (s, a0) =>
            {
                m.Invoke(s, new object[] { a0 });
            };
            return callback;
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1>(MethodInfo m)
#else
        public static Action<S, A0, A1> Create<S, A0, A1>(MethodInfo m)
#endif
        {
            Action<S, A0, A1> callback=
            (s, a0, a1) =>
            {
                m.Invoke(s, new object[] { a0, a1 });
            };
            return callback;
        }


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2> Create<S, A0, A1, A2>(MethodInfo m)
#endif
        {
            Action<S, A0, A1, A2> callback=
            (s, a0, a1, a2) =>
            {
                m.Invoke(s, new object[] { a0, a1, a2 });
            };
            return callback;
        }


#if UNITY_5
#else


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2, A3>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2, A3> Create<S, A0, A1, A2, A3>(MethodInfo m)
#endif
        {
            Action<S, A0, A1, A2, A3> callback=
            (s, a0, a1, a2, a3) =>
            {
                m.Invoke(s, new object[] { a0, a1, a2, a3 });
            };
            return callback;
        }


#endif


#if UNITY_5
#else


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2, A3, A4>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2, A3, A4> Create<S, A0, A1, A2, A3, A4>(MethodInfo m)
#endif
        {
            Action<S, A0, A1, A2, A3, A4> callback=
            (s, a0, a1, a2, a3, a4) =>
            {
                m.Invoke(s, new object[] { a0, a1, a2, a3, a4 });
            };
            return callback;
        }


#endif


#if UNITY_5
#else


#if UNITY_5
        public static Delegate Create<S, A0, A1, A2, A3, A4, A5>(MethodInfo m)
#else
        public static Action<S, A0, A1, A2, A3, A4, A5> Create<S, A0, A1, A2, A3, A4, A5>(MethodInfo m)
#endif
        {
            Action<S, A0, A1, A2, A3, A4, A5> callback=
            (s, a0, a1, a2, a3, a4, a5) =>
            {
                m.Invoke(s, new object[] { a0, a1, a2, a3, a4, a5 });
            };
            return callback;
        }


#endif

//////////// CreateWithThis

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

            // ToDo: CreateDelegate
            Action<A0> callback=
            (a0) => {
                m.Invoke(instance, new object[]{ a0 });
            };
            return callback;
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

            // ToDo: CreateDelegate
            Action<A0, A1> callback=
            (a0, a1) => {
                m.Invoke(instance, new object[]{ a0, a1 });
            };
            return callback;
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

            // ToDo: CreateDelegate
            Action<A0, A1, A2> callback=
            (a0, a1, a2) => {
                m.Invoke(instance, new object[]{ a0, a1, a2 });
            };
            return callback;
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

            // ToDo: CreateDelegate
            Action<A0, A1, A2, A3> callback=
            (a0, a1, a2, a3) => {
                m.Invoke(instance, new object[]{ a0, a1, a2, a3 });
            };
            return callback;
        }


#if UNITY_5
#else


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

            // ToDo: CreateDelegate
            Action<A0, A1, A2, A3, A4> callback=
            (a0, a1, a2, a3, a4) => {
                m.Invoke(instance, new object[]{ a0, a1, a2, a3, a4 });
            };
            return callback;
        }


#endif


#if UNITY_5
#else


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

            // ToDo: CreateDelegate
            Action<A0, A1, A2, A3, A4, A5> callback=
            (a0, a1, a2, a3, a4, a5) => {
                m.Invoke(instance, new object[]{ a0, a1, a2, a3, a4, a5 });
            };
            return callback;
        }


#endif


    }
}

