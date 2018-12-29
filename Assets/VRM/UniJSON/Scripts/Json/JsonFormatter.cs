using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public class JsonFormatter : IFormatter, IRpc
    {
        IStore m_w;
        protected IStore Store
        {
            get { return m_w; }
        }

        enum Current
        {
            ROOT,
            ARRAY,
            OBJECT
        }

        class Context
        {
            public Current Current;
            public int Count;

            public Context(Current current)
            {
                Current = current;
                Count = 0;
            }
        }

        Stack<Context> m_stack = new Stack<Context>();

        string m_indent;
        void Indent()
        {
            if (!string.IsNullOrEmpty(m_indent))
            {
                m_w.Write('\n');
                for (int i = 0; i < m_stack.Count - 1; ++i)
                {
                    m_w.Write(m_indent);
                }
            }
        }

        string m_colon;

        public JsonFormatter(int indent = 0)
            : this(new BytesStore(128), indent)
        {
        }

        public JsonFormatter(IStore w, int indent = 0)
        {
            m_w = w;
            m_stack.Push(new Context(Current.ROOT));
            m_indent = new string(Enumerable.Range(0, indent).Select(x => ' ').ToArray());
            m_colon = indent == 0 ? ":" : ": ";
        }

        public override string ToString()
        {
            var bytes = this.GetStoreBytes();
            return Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public IStore GetStore()
        {
            return m_w;
        }

        public void Clear()
        {
            m_w.Clear();
            m_stack.Clear();
            m_stack.Push(new Context(Current.ROOT));
        }

        protected void CommaCheck(bool isKey = false)
        {
            var top = m_stack.Pop();
            switch (top.Current)
            {
                case Current.ROOT:
                    {
                        if (top.Count != 0) throw new FormatterException("multiple root value");
                    }
                    break;

                case Current.ARRAY:
                    {
                        if (top.Count != 0)
                        {
                            m_w.Write(',');
                        }
                    }
                    break;

                case Current.OBJECT:
                    {
                        if (top.Count % 2 == 0)
                        {
                            if (!isKey) throw new FormatterException("key exptected");
                            if (top.Count != 0)
                            {
                                m_w.Write(',');
                            }
                        }
                        else
                        {
                            if (isKey) throw new FormatterException("key not exptected");
                        }
                    }
                    break;
            }
            top.Count += 1;
            /*
            {
                var debug = string.Format("{0} {1} = {2}", m_stack.Count, top.Current, top.Count);
                Debug.Log(debug);
            }
            */
            m_stack.Push(top);
        }

        static Utf8String s_null = Utf8String.From("null");
        public void Null()
        {
            CommaCheck();
            m_w.Write(s_null.Bytes);
        }

        public void BeginList(int _ = 0)
        {
            CommaCheck();
            m_w.Write('[');
            m_stack.Push(new Context(Current.ARRAY));
        }

        public void EndList()
        {
            if (m_stack.Peek().Current != Current.ARRAY)
            {
                throw new InvalidOperationException();
            }
            m_w.Write(']');
            m_stack.Pop();
        }

        public void BeginMap(int _ = 0)
        {
            CommaCheck();
            m_w.Write('{');
            m_stack.Push(new Context(Current.OBJECT));
        }

        public void EndMap()
        {
            if (m_stack.Peek().Current != Current.OBJECT)
            {
                throw new InvalidOperationException();
            }
            m_stack.Pop();
            Indent();
            m_w.Write('}');
        }

        public void Key(Utf8String key)
        {
            _Value(key, true);
            m_w.Write(m_colon);
        }

        public void Value(string x)
        {
            Value(Utf8String.From(x));
        }

        public void Value(Utf8String key)
        {
            _Value(key, false);
        }

        void _Value(Utf8String key, bool isKey)
        {
            CommaCheck(isKey);
            if (isKey)
            {
                Indent();
            }
            JsonString.Quote(key, m_w);
        }

        static Utf8String s_true = Utf8String.From("true");
        static Utf8String s_false = Utf8String.From("false");
        public void Value(Boolean x)
        {
            CommaCheck();
            m_w.Write(x ? s_true.Bytes : s_false.Bytes);
        }

        public void Value(SByte x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Int16 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Int32 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Int64 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }

        public void Value(Byte x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(UInt16 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(UInt32 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(UInt64 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }

        public void Value(Single x)
        {
            CommaCheck();
            m_w.Write(x.ToString("R", CultureInfo.InvariantCulture));
        }
        public void Value(Double x)
        {
            CommaCheck();
            m_w.Write(x.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Value(ArraySegment<Byte> x)
        {
            CommaCheck();
            m_w.Write('"');
            m_w.Write(Convert.ToBase64String(x.Array, x.Offset, x.Count));
            m_w.Write('"');
        }

        // ISO-8601: YYYY-MM-DD“T”hh:mm:ss“Z”
        public void Value(DateTimeOffset x)
        {
            Value(x.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        public void Value(ListTreeNode<JsonValue> node)
        {
            CommaCheck();
            m_w.Write(node.Value.Bytes);
        }

        #region IRpc
        int m_nextRequestId = 1;

        static Utf8String s_jsonrpc = Utf8String.From("jsonrpc");
        static Utf8String s_20 = Utf8String.From("2.0");
        static Utf8String s_method = Utf8String.From("method");
        static Utf8String s_params = Utf8String.From("params");

        public void Notify(Utf8String method)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
            }
            EndList();
            EndMap();
        }

        public void Notify<A0>(Utf8String method, A0 a0)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
            }
            EndList();
            EndMap();
        }

        public void Notify<A0, A1>(Utf8String method, A0 a0, A1 a1)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
            }
            EndList();
            EndMap();
        }

        public void Notify<A0, A1, A2>(Utf8String method, A0 a0, A1 a1, A2 a2)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
            }
            EndList();
            EndMap();
        }

        public void Notify<A0, A1, A2, A3>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
                this.Serialize(a3);
            }
            EndList();
            EndMap();
        }

        public void Notify<A0, A1, A2, A3, A4>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
                this.Serialize(a3);
                this.Serialize(a4);
            }
            EndList();
            EndMap();
        }

        public void Notify<A0, A1, A2, A3, A4, A5>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
                this.Serialize(a3);
                this.Serialize(a4);
                this.Serialize(a5);
            }
            EndList();
            EndMap();
        }

        static Utf8String s_id = Utf8String.From("id");

        public void Request(Utf8String method)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
            }
            EndList();
            EndMap();
        }

        public void Request<A0>(Utf8String method,
            A0 a0)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
            }
            EndList();
            EndMap();
        }

        public void Request<A0, A1>(Utf8String method,
            A0 a0, A1 a1)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
            }
            EndList();
            EndMap();
        }

        public void Request<A0, A1, A2>(Utf8String method,
            A0 a0, A1 a1, A2 a2)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
            }
            EndList();
            EndMap();
        }

        public void Request<A0, A1, A2, A3>(Utf8String method,
            A0 a0, A1 a1, A2 a2, A3 a3)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
                this.Serialize(a3);
            }
            EndList();
            EndMap();
        }

        public void Request<A0, A1, A2, A3, A4>(Utf8String method,
            A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
                this.Serialize(a3);
                this.Serialize(a4);
            }
            EndList();
            EndMap();
        }

        public void Request<A0, A1, A2, A3, A4, A5>(Utf8String method,
            A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(m_nextRequestId++);
            Key(s_method); Value(method);
            Key(s_params); BeginList();
            {
                this.Serialize(a0);
                this.Serialize(a1);
                this.Serialize(a2);
                this.Serialize(a3);
                this.Serialize(a4);
                this.Serialize(a5);
            }
            EndList();
            EndMap();
        }

        static Utf8String s_error = Utf8String.From("error");

        public void ResponseError(int id, Exception error)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(id);
            Key(s_error); this.Serialize(error);
            EndMap();
        }

        static Utf8String s_result = Utf8String.From("result");

        public void ResponseSuccess(int id)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(id);
            Key(s_result); Null();
            EndMap();
        }

        public void ResponseSuccess<T>(int id, T result)
        {
            BeginMap();
            Key(s_jsonrpc); Value(s_20);
            Key(s_id); Value(id);
            Key(s_result); this.Serialize(result);
            EndMap();
        }
        #endregion
    }
}
