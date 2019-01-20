using System;


namespace UniJSON
{
    public class MsgPackFormatter : IFormatter, IRpc
    {
        IStore m_store;
        public MsgPackFormatter(IStore store)
        {
            m_store = store;
        }

        public MsgPackFormatter() : this(new BytesStore())
        {
        }

        public void Clear()
        {
            m_store.Clear();
        }

#if false
        public bool MsgPack_Ext(IList list)
        {
            var t = list.GetType();
            var et = t.GetElementType();
            if (et.IsClass())
            {
                return false;
            }
            m_store.Write((Byte)MsgPackType.EXT32);
            var itemSize = Marshal.SizeOf(et);
            WriteInt32_NBO(list.Count * itemSize);

            Action<Object> pack;
            if (et == typeof(UInt16))
            {
                m_store.Write((Byte)ExtType.UINT16_BE);
                pack = o => WriteUInt16_NBO((UInt16)o);
            }
            else if (et == typeof(UInt32))
            {
                m_store.Write((Byte)ExtType.UINT32_BE);
                pack = o => WriteUInt32_NBO((UInt32)o);
            }
            else if (et == typeof(UInt64))
            {
                m_store.Write((Byte)ExtType.UINT64_BE);
                pack = o => WriteUInt64_NBO((UInt64)o);
            }
            else if (et == typeof(Int16))
            {
                m_store.Write((Byte)ExtType.INT16_BE);
                pack = o => WriteInt16_NBO((Int16)o);
            }
            else if (et == typeof(Int32))
            {
                m_store.Write((Byte)ExtType.INT32_BE);
                pack = o => WriteInt32_NBO((Int32)o);
            }
            else if (et == typeof(Int64))
            {
                m_store.Write((Byte)ExtType.INT64_BE);
                pack = o => WriteInt64_NBO((Int64)o);
            }
            else if (et == typeof(Single))
            {
                m_store.Write((Byte)ExtType.SINGLE_BE);
                pack = o => WriteSingle_NBO((Single)o);
            }
            else if (et == typeof(Double))
            {
                m_store.Write((Byte)ExtType.DOUBLE_BE);
                pack = o => WriteDouble_NBO((Double)o);
            }
            else
            {
                return false;
            }

            foreach (var i in list)
            {
                pack(i);
            }
            return true;
        }
#endif

        public void BeginList(int n)
        {
            if (n < 0x0F)
            {
                m_store.Write((Byte)((Byte)MsgPackType.FIX_ARRAY | n));
            }
            else if (n < 0xFFFF)
            {
                m_store.Write((Byte)MsgPackType.ARRAY16);
                m_store.WriteBigEndian((UInt16)n);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.ARRAY32);
                m_store.WriteBigEndian(n);
            }
        }

        public void EndList()
        {
        }

        public void BeginMap(int n)
        {
            if (n < 0x0F)
            {
                m_store.Write((Byte)((Byte)MsgPackType.FIX_MAP | n));
            }
            else if (n < 0xFFFF)
            {
                m_store.Write((Byte)MsgPackType.MAP16);
                m_store.WriteBigEndian((UInt16)n);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.MAP32);
                m_store.WriteBigEndian(n.ToNetworkByteOrder());
            }
        }

        public void EndMap()
        {
        }

        public void Null()
        {
            m_store.Write((Byte)MsgPackType.NIL);
        }

        public void Key(Utf8String key)
        {
            Value(key);
        }

        public void Value(String s)
        {
            Value(Utf8String.From(s));
        }

        public void Value(Utf8String s)
        {
            var bytes = s.Bytes;
            int size = bytes.Count;
            if (size < 32)
            {
                m_store.Write((Byte)((Byte)MsgPackType.FIX_STR | size));
                m_store.Write(bytes);
            }
            else if (size < 0xFF)
            {
                m_store.Write((Byte)(MsgPackType.STR8));
                m_store.Write((Byte)(size));
                m_store.Write(bytes);
            }
            else if (size < 0xFFFF)
            {
                m_store.Write((Byte)MsgPackType.STR16);
                m_store.WriteBigEndian((UInt16)size);
                m_store.Write(bytes);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.STR32);
                m_store.WriteBigEndian(size);
                m_store.Write(bytes);
            }
        }

        public void Value(bool value)
        {
            if (value)
            {
                m_store.Write((Byte)MsgPackType.TRUE);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.FALSE);
            }
        }

        #region Singed
        public void Value(sbyte n)
        {
            if (n >= 0)
            {
                // positive
                Value((Byte)n);
            }
            else if (n >= -32)
            {
                var value = (MsgPackType)((n + 32) + (Byte)MsgPackType.NEGATIVE_FIXNUM);
                m_store.Write((Byte)value);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.INT8);
                m_store.Write((Byte)n);
            }
        }

        public void Value(short n)
        {
            if (n >= 0)
            {
                // positive
                if (n <= 0xFF)
                {
                    Value((Byte)n);
                }
                else
                {
                    Value((UInt16)n);
                }
            }
            else
            {
                // negative
                if (n >= -128)
                {
                    m_store.Write((SByte)n);
                }
                else
                {
                    m_store.Write((Byte)MsgPackType.INT16);
                    m_store.WriteBigEndian(n);
                }
            }
        }

        public void Value(int n)
        {
            if (n >= 0)
            {
                // positive
                if (n <= 0xFF)
                {
                    Value((Byte)n);
                }
                else if (n <= 0xFFFF)
                {
                    Value((UInt16)n);
                }
                else
                {
                    Value((UInt32)n);
                }
            }
            else
            {
                // negative
                if (n >= -128)
                {
                    Value((SByte)n);
                }
                else if (n >= -32768)
                {
                    Value((Int16)n);
                }
                else
                {
                    m_store.Write((Byte)MsgPackType.INT32);
                    m_store.WriteBigEndian(n);
                }
            }
        }

        public void Value(long n)
        {
            if (n >= 0)
            {
                // positive
                if (n <= 0xFF)
                {
                    Value((Byte)n);
                }
                else if (n <= 0xFFFF)
                {
                    Value((UInt16)n);
                }
                else if (n <= 0xFFFFFFFF)
                {
                    Value((UInt32)n);
                }
                else
                {
                    Value((UInt64)n);
                }
            }
            else
            {
                // negative
                if (n >= -128)
                {
                    Value((SByte)n);
                }
                else if (n >= -32768)
                {
                    Value((Int16)n);
                }
                else if (n >= -2147483648)
                {
                    Value((Int32)n);
                }
                else
                {
                    m_store.Write((Byte)MsgPackType.INT64);
                    m_store.WriteBigEndian(n);
                }
            }
        }
        #endregion

        #region Unsigned
        public void Value(byte n)
        {
            if (n <= 0x7F)
            {
                // FormatType.POSITIVE_FIXNUM
                m_store.Write(n);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.UINT8);
                m_store.Write(n);
            }
        }

        public void Value(ushort n)
        {
            if (n <= 0xFF)
            {
                Value((Byte)n);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.UINT16);
                m_store.WriteBigEndian(n);
            }
        }

        public void Value(uint n)
        {
            if (n <= 0xFF)
            {
                Value((Byte)n);
            }
            else if (n <= 0xFFFF)
            {
                Value((UInt16)n);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.UINT32);
                m_store.WriteBigEndian(n);
            }
        }

        public void Value(ulong n)
        {
            if (n <= 0xFF)
            {
                Value((Byte)n);
            }
            else if (n <= 0xFFFF)
            {
                Value((UInt16)n);
            }
            else if (n <= 0xFFFFFFFF)
            {
                Value((UInt32)n);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.UINT64);
                m_store.WriteBigEndian(n);
            }
        }
        #endregion

        public void Value(float value)
        {
            m_store.Write((Byte)MsgPackType.FLOAT);
            m_store.WriteBigEndian(value);
        }

        public void Value(double value)
        {
            m_store.Write((Byte)MsgPackType.DOUBLE);
            m_store.WriteBigEndian(value);
        }

        public void Value(ArraySegment<byte> bytes)
        {
            if (bytes.Count < 0xFF)
            {
                m_store.Write((Byte)(MsgPackType.BIN8));
                m_store.Write((Byte)(bytes.Count));
                m_store.Write(bytes);
            }
            else if (bytes.Count < 0xFFFF)
            {
                m_store.Write((Byte)MsgPackType.BIN16);
                m_store.WriteBigEndian((UInt16)bytes.Count);
                m_store.Write(bytes);
            }
            else
            {
                m_store.Write((Byte)MsgPackType.BIN32);
                m_store.WriteBigEndian(bytes.Count);
                m_store.Write(bytes);
            }
        }

        public void TimeStamp32(DateTimeOffset time)
        {
            // https://github.com/ousttrue/UniJSON/blob/1.2/Scripts/Extensions/DateTimeOffsetExtensions.cs#L13-L16
            if (time < DateTimeOffsetExtensions.EpochTime)
            {
                throw new ArgumentOutOfRangeException();
            }
            m_store.Write((Byte)MsgPackType.FIX_EXT_4);
            m_store.Write((SByte)(-1));
            m_store.WriteBigEndian((uint)time.ToUnixTimeSeconds());
        }

        public void Value(DateTimeOffset time)
        {
            TimeStamp32(time);
        }

        public void Value(ListTreeNode<MsgPackValue> node)
        {
            m_store.Write(node.Value.Bytes);
        }

        public IStore GetStore()
        {
            return m_store;
        }

        #region IRpc
        public const int REQUEST_TYPE = 0;
        public const int RESPONSE_TYPE = 1;
        public const int NOTIFY_TYPE = 2;

        int m_msgId = 1;

        public void Request(Utf8String method)
        {
            BeginList(4);
            Value(REQUEST_TYPE);
            Value(m_msgId++);
            Value(method);
            BeginList(0); // params
            {
            }
            EndList();
            EndList();
        }

        public void Request<A0>(Utf8String method, A0 a0)
        {
            BeginList(4);
            Value(REQUEST_TYPE);
            Value(m_msgId++);
            Value(method);
            BeginList(1); // params
            {
                this.Serialize(a0);
            }
            EndList();
            EndList();
        }

        public void Request<A0, A1>(Utf8String method, A0 a0, A1 a1)
        {
            BeginList(4);
            Value(REQUEST_TYPE);
            Value(m_msgId++);
            Value(method);
            BeginList(2); // params
            {
                this.Serialize(a0);
                this.Serialize(a1);
            }
            EndList();
            EndList();
        }

        public void Request<A0, A1, A2>(Utf8String method, A0 a0, A1 a1, A2 a2)
        {
            throw new NotImplementedException();
        }

        public void Request<A0, A1, A2, A3>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3)
        {
            throw new NotImplementedException();
        }

        public void Request<A0, A1, A2, A3, A4>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            throw new NotImplementedException();
        }

        public void Request<A0, A1, A2, A3, A4, A5>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            throw new NotImplementedException();
        }

        public void ResponseSuccess(int id)
        {
            BeginList(4);
            Value(RESPONSE_TYPE);
            Value(id);
            Null();
            Null();
            EndList();
        }

        public void ResponseSuccess<T>(int id, T result)
        {
            BeginList(4);
            Value(RESPONSE_TYPE);
            Value(id);
            Null();
            this.Serialize(result);
            EndList();
        }

        public void ResponseError(int id, Exception error)
        {
            BeginList(4);
            Value(RESPONSE_TYPE);
            Value(id);
            this.Serialize(error);
            Null();
            EndList();
        }

        public void Notify(Utf8String method)
        {
            BeginList(3);
            Value(NOTIFY_TYPE);
            Value(method);
            BeginList(0); // params
            {
            }
            EndList();
            EndList();
        }

        public void Notify<A0>(Utf8String method, A0 a0)
        {
            BeginList(3);
            Value(NOTIFY_TYPE);
            Value(method);
            BeginList(1); // params
            {
                this.Serialize(a0);
            }
            EndList();
            EndList();
        }

        public void Notify<A0, A1>(Utf8String method, A0 a0, A1 a1)
        {
            BeginList(3);
            Value(NOTIFY_TYPE);
            Value(method);
            BeginList(2); // params
            {
                this.Serialize(a0);
                this.Serialize(a1);
            }
            EndList();
            EndList();
        }

        public void Notify<A0, A1, A2>(Utf8String method, A0 a0, A1 a1, A2 a2)
        {
            throw new NotImplementedException();
        }

        public void Notify<A0, A1, A2, A3>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3)
        {
            throw new NotImplementedException();
        }

        public void Notify<A0, A1, A2, A3, A4>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            throw new NotImplementedException();
        }

        public void Notify<A0, A1, A2, A3, A4, A5>(Utf8String method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
