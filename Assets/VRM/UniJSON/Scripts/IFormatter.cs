using System;


namespace UniJSON
{
    public interface IFormatter
    {
        IStore GetStore();
        void Clear();

        void BeginList(int n);
        void EndList();

        void BeginMap(int n);
        void EndMap();

        void Key(Utf8String x);

        void Null();

        void Value(Utf8String x);
        void Value(String x);

        void Value(ArraySegment<Byte> bytes);

        void Value(Boolean x);

        void Value(Byte x);
        void Value(UInt16 x);
        void Value(UInt32 x);
        void Value(UInt64 x);

        void Value(SByte x);
        void Value(Int16 x);
        void Value(Int32 x);
        void Value(Int64 x);

        void Value(Single x);
        void Value(Double x);

        void Value(DateTimeOffset x);
    }
}
