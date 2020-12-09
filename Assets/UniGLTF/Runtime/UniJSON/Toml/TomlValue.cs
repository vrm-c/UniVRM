using System;


namespace UniJSON
{
    public enum TomlValueType
    {
        BareKey, // key
        QuotedKey, // "key"
        DottedKey, // key.nested
        BasicString, // "str"
        MultilineBasicString, // """str"""
        LiteralString, // 'str'
        MultilineLiteralString, // '''str'''
        Integer,
        Float,
        Boolean,
        OffsetDatetime,
        Array, // [1, 2, 3]
        Table, // [table_name]
    }

    public struct TomlValue : IListTreeItem, IValue<TomlValue>
    {
        public override string ToString()
        {
            return m_segment.ToString();
        }

        public int ParentIndex { get; private set; }

        public TomlValueType TomlValueType
        {
            get;
            private set;
        }

        public ValueNodeType ValueType
        {
            get
            {
                switch (TomlValueType)
                {
                    case TomlValueType.Integer: return ValueNodeType.Integer;
                    case TomlValueType.Float: return ValueNodeType.Number;
                    case TomlValueType.Boolean: return ValueNodeType.Boolean;

                    case TomlValueType.BareKey: return ValueNodeType.String;
                    case TomlValueType.QuotedKey: return ValueNodeType.String;
                    case TomlValueType.DottedKey: return ValueNodeType.String;

                    case TomlValueType.BasicString: return ValueNodeType.String;
                    case TomlValueType.MultilineBasicString: return ValueNodeType.String;
                    case TomlValueType.LiteralString: return ValueNodeType.String;
                    case TomlValueType.MultilineLiteralString: return ValueNodeType.String;

                    case TomlValueType.Table: return ValueNodeType.Object;
                    case TomlValueType.Array: return ValueNodeType.Array;
                }
                throw new NotImplementedException();
            }
        }

        Utf8String m_segment;

        public ArraySegment<byte> Bytes { get { return m_segment.Bytes; } }
        public void SetBytesCount(int count)
        {
            throw new NotImplementedException();
        }

        public int ChildCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public void SetChildCount(int count)
        {
            throw new NotImplementedException();
        }

        public TomlValue(Utf8String segment, TomlValueType valueType, int parentIndex) : this()
        {
            ParentIndex = parentIndex;
            TomlValueType = valueType;
            m_segment = segment;
        }

        public bool GetBoolean()
        {
            throw new NotImplementedException();
        }

        public byte GetByte()
        {
            throw new NotImplementedException();
        }

        public double GetDouble()
        {
            throw new NotImplementedException();
        }

        public short GetInt16()
        {
            throw new NotImplementedException();
        }

        public int GetInt32()
        {
            return m_segment.ToInt32();
        }

        public long GetInt64()
        {
            throw new NotImplementedException();
        }

        public sbyte GetSByte()
        {
            throw new NotImplementedException();
        }

        public float GetSingle()
        {
            throw new NotImplementedException();
        }

        public string GetString()
        {
            throw new NotImplementedException();
        }

        public ushort GetUInt16()
        {
            throw new NotImplementedException();
        }

        public uint GetUInt32()
        {
            throw new NotImplementedException();
        }

        public ulong GetUInt64()
        {
            throw new NotImplementedException();
        }

        public Utf8String GetUtf8String()
        {
            return m_segment;
        }

        public U GetValue<U>()
        {
            throw new NotImplementedException();
        }

        public TomlValue Key(Utf8String key, int parentIndex)
        {
            throw new NotImplementedException();
        }

        public TomlValue New(ArraySegment<byte> bytes, ValueNodeType valueType, int parentIndex)
        {
            throw new NotImplementedException();
        }
    }
}
