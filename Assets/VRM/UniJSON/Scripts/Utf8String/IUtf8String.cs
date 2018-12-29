using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;


namespace UniJSON
{
    public interface IUtf8String: IEnumerable<Byte>
    {
        int ByteLength { get; }
    }

    /// <summary>
    /// Immutable short utf8 string
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Utf8String4 : IEquatable<Utf8String4>, IUtf8String
    {
        [FieldOffset(0)]
        uint _value;

        [FieldOffset(0)]
        byte _byte0;

        [FieldOffset(1)]
        byte _byte1;

        [FieldOffset(2)]
        byte _byte2;

        [FieldOffset(3)]
        byte _byte3;

        public int ByteLength
        {
            get
            {
                if (_byte0 == 0) return 0;
                if (_byte1 == 0) return 1;
                if (_byte2 == 0) return 2;
                if (_byte3 == 0) return 3;
                return 4;
            }
        }

        static Utf8String4 Create(uint value)
        {
            return new Utf8String4
            {
                _value = value
            };
        }

        public static Utf8String4 Create(IEnumerable<byte> bytes)
        {
            var u = new Utf8String4();
            var it = bytes.GetEnumerator();

            if (!it.MoveNext()) return u;
            u._byte0 = it.Current;

            if (!it.MoveNext()) return u;
            u._byte1 = it.Current;

            if (!it.MoveNext()) return u;
            u._byte2 = it.Current;

            if (!it.MoveNext()) return u;
            u._byte3 = it.Current;

            if (!it.MoveNext())
            {
                throw new ArgumentOutOfRangeException();
            }

            return u;
        }

        public static Utf8String4 Create(string src)
        {
            return Create(Utf8String.Encoding.GetBytes(src));
        }

        public bool Equals(Utf8String4 other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is Utf8String4)
            {
                return Equals((Utf8String4)obj);
            }

            {
                var s = obj as string;
                if (s != null)
                {
                    return ToString() == s;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return Utf8String.Encoding.GetString(this.ToArray());
        }

        public IEnumerator<byte> GetEnumerator()
        {
            if (_byte0 == 0) yield break;
            yield return _byte0;
            if (_byte1 == 0) yield break;
            yield return _byte1;
            if (_byte2 == 0) yield break;
            yield return _byte2;
            if (_byte3 == 0) yield break;
            yield return _byte3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
