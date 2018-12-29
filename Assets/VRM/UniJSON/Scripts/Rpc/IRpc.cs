namespace UniJSON
{
    public interface IRpc
    {
        void Request(Utf8String method);
        void Request<A0>(Utf8String method, A0 a0);
        void Request<A0, A1>(Utf8String method, A0 a0, A1 a1);
        void Request<A0, A1, A2>(Utf8String method, A0 a0, A1 a1, A2 a2);
        void Request<A0, A1, A2, A3>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3);
        void Request<A0, A1, A2, A3, A4>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
        void Request<A0, A1, A2, A3, A4, A5>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
        void ResponseSuccess(int id);
        void ResponseSuccess<T>(int id, T result);
        void ResponseError(int id, System.Exception error);
        void Notify(Utf8String method);
        void Notify<A0>(Utf8String method, A0 a0);
        void Notify<A0, A1>(Utf8String method, A0 a0, A1 a1);
        void Notify<A0, A1, A2>(Utf8String method, A0 a0, A1 a1, A2 a2);
        void Notify<A0, A1, A2, A3>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3);
        void Notify<A0, A1, A2, A3, A4>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
        void Notify<A0, A1, A2, A3, A4, A5>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
    }

    public static class RpcExtensions
    {
        public static void Request(this IRpc rpc, string method)
        {
            rpc.Request(Utf8String.From(method));
        }
        public static void Request<A0>(this IRpc rpc, string method, A0 a0)
        {
            rpc.Request(Utf8String.From(method), a0);
        }
        public static void Request<A0, A1>(this IRpc rpc, string method, A0 a0, A1 a1)
        {
            rpc.Request(Utf8String.From(method), a0, a1);
        }
        public static void Request<A0, A1, A2>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2)
        {
            rpc.Request(Utf8String.From(method), a0, a1, a2);
        }
        public static void Request<A0, A1, A2, A3>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2, A3 a3)
        {
            rpc.Request(Utf8String.From(method), a0, a1, a2, a3);
        }
        public static void Request<A0, A1, A2, A3, A4>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            rpc.Request(Utf8String.From(method), a0, a1, a2, a3, a4);
        }
        public static void Request<A0, A1, A2, A3, A4, A5>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            rpc.Request(Utf8String.From(method), a0, a1, a2, a3, a4, a5);
        }

        public static void Notify(this IRpc rpc, string method)
        {
            rpc.Notify(Utf8String.From(method));
        }
        public static void Notify<A0>(this IRpc rpc, string method, A0 a0)
        {
            rpc.Notify(Utf8String.From(method), a0);
        }
        public static void Notify<A0, A1>(this IRpc rpc, string method, A0 a0, A1 a1)
        {
            rpc.Notify(Utf8String.From(method), a0, a1);
        }
        public static void Notify<A0, A1, A2>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2)
        {
            rpc.Notify(Utf8String.From(method), a0, a1, a2);
        }
        public static void Notify<A0, A1, A2, A3>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2, A3 a3)
        {
            rpc.Notify(Utf8String.From(method), a0, a1, a2, a3);
        }
        public static void Notify<A0, A1, A2, A3, A4>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            rpc.Notify(Utf8String.From(method), a0, a1, a2, a3, a4);
        }
        public static void Notify<A0, A1, A2, A3, A4, A5>(this IRpc rpc, string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            rpc.Notify(Utf8String.From(method), a0, a1, a2, a3, a4, a5);
        }
    }
}
