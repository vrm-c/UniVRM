using System;


namespace UniJSON
{
    public enum ValueNodeType
    {
        Null,
        Boolean,
        String,
        Binary,
        Integer,
        Number,
        Array,
        Object,
        NaN,
        Infinity,
        MinusInfinity,
    }

    public interface IValue<T>
    {
        T New(ArraySegment<byte> bytes, ValueNodeType valueType, int parentIndex);
        T Key(Utf8String key, int parentIndex);
        ValueNodeType ValueType { get; }
        ArraySegment<Byte> Bytes { get; }
        void SetBytesCount(int count);
        Boolean GetBoolean();
        String GetString();
        Utf8String GetUtf8String();
        SByte GetSByte();
        Int16 GetInt16();
        Int32 GetInt32();
        Int64 GetInt64();
        Byte GetByte();
        UInt16 GetUInt16();
        UInt32 GetUInt32();
        UInt64 GetUInt64();
        Single GetSingle();
        Double GetDouble();
        U GetValue<U>();
    }
}
