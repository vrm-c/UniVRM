using System;


namespace UniJSON
{
    public struct JsonValue
    {
        public Utf8String Segment;
        public ArraySegment<Byte> Bytes { get { return Segment.Bytes; } }
        public void SetBytesCount(int count)
        {
            Segment = new Utf8String(new ArraySegment<byte>(Bytes.Array, Bytes.Offset, count));
        }

        public ValueNodeType ValueType
        {
            get;
            private set;
        }

        public int ParentIndex
        {
            get;
            private set;
        }

        int _childCount;
        public int ChildCount
        {
            get { return _childCount; }
        }
        public void SetChildCount(int count)
        {
            _childCount = count;
        }

        public JsonValue(Utf8String segment, ValueNodeType valueType, int parentIndex) : this()
        {
            Segment = segment;
            ValueType = valueType;
            ParentIndex = parentIndex;
        }

        public JsonValue New(ArraySegment<byte> bytes, ValueNodeType valueType, int parentIndex)
        {
            return new JsonValue(new Utf8String(bytes), valueType, parentIndex);
        }

        public JsonValue Key(Utf8String key, int parentIndex)
        {
            return new JsonValue(JsonString.Quote(key), ValueNodeType.String, parentIndex);
        }

        public override string ToString()
        {
            switch (ValueType)
            {
                case ValueNodeType.Null:
                case ValueNodeType.Boolean:
                case ValueNodeType.Integer:
                case ValueNodeType.Number:
                case ValueNodeType.Array:
                case ValueNodeType.Object:
                case ValueNodeType.String:
                case ValueNodeType.NaN:
                case ValueNodeType.Infinity:
                case ValueNodeType.MinusInfinity:
                    return Segment.ToString();

                default:
                    throw new NotImplementedException();
            }
        }

        static Utf8String s_true = Utf8String.From("true");
        static Utf8String s_false = Utf8String.From("false");

        public Boolean GetBoolean()
        {
            if (Segment == s_true)
            {
                return true;
            }
            else if (Segment == s_false)
            {
                return false;
            }
            else
            {
                throw new DeserializationException("invalid boolean: " + Segment.ToString());
            }
        }

        public SByte GetSByte() { return Segment.ToSByte(); }
        public Int16 GetInt16() { return Segment.ToInt16(); }
        public Int32 GetInt32() { return Segment.ToInt32(); }
        public Int64 GetInt64() { return Segment.ToInt64(); }
        public Byte GetByte() { return Segment.ToByte(); }
        public UInt16 GetUInt16() { return Segment.ToUInt16(); }
        public UInt32 GetUInt32() { return Segment.ToUInt32(); }
        public UInt64 GetUInt64() { return Segment.ToUInt64(); }
        public Single GetSingle() { return Segment.ToSingle(); }
        public Double GetDouble() { return Segment.ToDouble(); }
        public String GetString() { return JsonString.Unquote(Segment.ToString()); }
        public Utf8String GetUtf8String() { return JsonString.Unquote(Segment); }
    }
}
