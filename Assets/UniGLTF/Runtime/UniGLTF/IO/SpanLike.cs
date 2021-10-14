using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    static class BitWriter
    {
        public static void Write(byte[] bytes, int i, Int32 value)
        {
            var tmp = BitConverter.GetBytes(value);
            bytes[i++] = tmp[0];
            bytes[i++] = tmp[1];
            bytes[i++] = tmp[2];
            bytes[i++] = tmp[3];
        }

        #region UINT
        public static void Write(byte[] bytes, int i, Byte value)
        {
            bytes[i++] = value;
        }

        public static void Write(byte[] bytes, int i, UInt16 value)
        {
            var tmp = BitConverter.GetBytes(value);
            bytes[i++] = tmp[0];
            bytes[i++] = tmp[1];
        }

        public static void Write(byte[] bytes, int i, UInt32 value)
        {
            var tmp = BitConverter.GetBytes(value);
            bytes[i++] = tmp[0];
            bytes[i++] = tmp[1];
            bytes[i++] = tmp[2];
            bytes[i++] = tmp[3];
        }
        #endregion

        public static void Write(byte[] bytes, int i, Single value)
        {
            var tmp = BitConverter.GetBytes(value);
            bytes[i++] = tmp[0];
            bytes[i++] = tmp[1];
            bytes[i++] = tmp[2];
            bytes[i++] = tmp[3];
        }

        public static void Write(byte[] bytes, int i, System.Numerics.Vector2 value)
        {
            Write(bytes, i, value.X);
            Write(bytes, i + 4, value.Y);
        }

        public static void Write(byte[] bytes, int i, System.Numerics.Vector3 value)
        {
            Write(bytes, i, value.X);
            Write(bytes, i + 4, value.Y);
            Write(bytes, i + 8, value.Z);
        }

        public static void Write(byte[] bytes, int i, System.Numerics.Vector4 value)
        {
            Write(bytes, i, value.X);
            Write(bytes, i + 4, value.Y);
            Write(bytes, i + 8, value.Z);
            Write(bytes, i + 12, value.W);
        }

        public static void Write(byte[] bytes, int i, System.Numerics.Quaternion value)
        {
            Write(bytes, i, value.X);
            Write(bytes, i + 4, value.Y);
            Write(bytes, i + 8, value.Z);
            Write(bytes, i + 12, value.W);
        }

        public static void Write(byte[] bytes, int i, System.Numerics.Matrix4x4 value)
        {
            Write(bytes, i, value.M11);
            Write(bytes, i + 4, value.M12);
            Write(bytes, i + 8, value.M13);
            Write(bytes, i + 12, value.M14);
            Write(bytes, i + 16, value.M21);
            Write(bytes, i + 20, value.M22);
            Write(bytes, i + 24, value.M23);
            Write(bytes, i + 28, value.M24);
            Write(bytes, i + 32, value.M31);
            Write(bytes, i + 36, value.M32);
            Write(bytes, i + 40, value.M33);
            Write(bytes, i + 44, value.M34);
            Write(bytes, i + 48, value.M41);
            Write(bytes, i + 52, value.M42);
            Write(bytes, i + 56, value.M43);
            Write(bytes, i + 60, value.M44);
        }

        public static void Write(byte[] bytes, int i, UShort4 value)
        {
            Write(bytes, i, value.x);
            Write(bytes, i + 2, value.y);
            Write(bytes, i + 4, value.z);
            Write(bytes, i + 6, value.w);
        }

        public static void Write(byte[] bytes, int i, SkinJoints value)
        {
            Write(bytes, i, value.Joint0);
            Write(bytes, i + 2, value.Joint1);
            Write(bytes, i + 4, value.Joint2);
            Write(bytes, i + 6, value.Joint3);
        }

        public static void Write(byte[] bytes, int i, Byte4 value)
        {
            bytes[i++] = value.x;
            bytes[i++] = value.y;
            bytes[i++] = value.z;
            bytes[i++] = value.w;
        }

        public static void Write(byte[] bytes, int i, UnityEngine.Vector2 value)
        {
            Write(bytes, i, value.x);
            Write(bytes, i + 4, value.y);
        }

        public static void Write(byte[] bytes, int i, UnityEngine.Vector3 value)
        {
            Write(bytes, i, value.x);
            Write(bytes, i + 4, value.y);
            Write(bytes, i + 8, value.z);
        }

        public static void Write(byte[] bytes, int i, UnityEngine.Vector4 value)
        {
            Write(bytes, i, value.x);
            Write(bytes, i + 4, value.y);
            Write(bytes, i + 8, value.z);
            Write(bytes, i + 12, value.w);
        }

        public static void Write(byte[] bytes, int i, UnityEngine.Color value)
        {
            Write(bytes, i, value.r);
            Write(bytes, i + 4, value.g);
            Write(bytes, i + 8, value.b);
            Write(bytes, i + 12, value.a);
        }

        public static void Write(byte[] bytes, int i, UnityEngine.Matrix4x4 value)
        {
            Write(bytes, i, value.m00);
            Write(bytes, i + 4, value.m01);
            Write(bytes, i + 8, value.m02);
            Write(bytes, i + 12, value.m03);
            Write(bytes, i + 16, value.m10);
            Write(bytes, i + 20, value.m11);
            Write(bytes, i + 24, value.m12);
            Write(bytes, i + 28, value.m13);
            Write(bytes, i + 32, value.m20);
            Write(bytes, i + 36, value.m21);
            Write(bytes, i + 40, value.m22);
            Write(bytes, i + 44, value.m23);
            Write(bytes, i + 48, value.m30);
            Write(bytes, i + 52, value.m31);
            Write(bytes, i + 56, value.m32);
            Write(bytes, i + 60, value.m33);
        }
    }

    public static class KeyValuePariExtensions
    {
        public static void Deconstruct<T, U>(this KeyValuePair<T, U> pair, out T key, out U value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }

    public struct SpanLike<T> : IEquatable<SpanLike<T>>, IEnumerable<T>
    where T : struct
    {
        public readonly ArraySegment<byte> Bytes;

        readonly int m_itemSize;
        public readonly int Length;

        public delegate T Getter(byte[] bytes, int start);
        readonly Getter m_getter;

        public delegate void Setter(byte[] bytes, int i, T value);
        readonly Setter m_setter;

        public T this[int i]
        {
            set
            {
                m_setter(Bytes.Array, Bytes.Offset + i * m_itemSize, value);
            }
            get
            {
                return m_getter(Bytes.Array, Bytes.Offset + i * m_itemSize);
            }
        }

        public SpanLike(ArraySegment<byte> bytes, int itemSize, Getter getter, Setter setter)
        {
            Bytes = bytes;
            m_itemSize = itemSize;
            Length = Bytes.Count / m_itemSize;
            m_getter = getter;
            m_setter = setter;
        }

        public SpanLike<T> Slice(int offset, int count)
        {
            var bytesOffset = offset * m_itemSize;
            var bytesLength = count * m_itemSize;
            if (bytesOffset + bytesLength > Bytes.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return new SpanLike<T>(new ArraySegment<byte>(
                Bytes.Array,
                Bytes.Offset + bytesOffset,
                bytesLength
            ), m_itemSize, m_getter, m_setter);
        }

        public SpanLike<T> Slice(int offset)
        {
            var bytesOffset = offset * m_itemSize;
            if (bytesOffset > Bytes.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            var bytesLength = Bytes.Count - bytesOffset;
            return new SpanLike<T>(new ArraySegment<byte>(
                Bytes.Array,
                Bytes.Offset + bytesOffset,
                bytesLength
            ), m_itemSize, m_getter, m_setter);
        }

        public T[] ToArray()
        {
            var array = new T[Length];
            Bytes.FromBytes(array);
            return array;
        }

        public bool Equals(SpanLike<T> other)
        {
            if (Length != other.Length)
            {
                return false;
            }

            var end = Length;
            for (int i = 0; i < end; ++i)
            {
                if (!this[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }

    public static class SpanLike
    {
        struct GetSet<T> where T : struct
        {
            public SpanLike<T>.Getter Getter;
            public SpanLike<T>.Setter Setter;
        }

        public static readonly Dictionary<Type, object> Map = new Dictionary<Type, object>()
        {
            {typeof(Byte), new GetSet<byte>{
                Getter = (array, start) => array[start],
                Setter = BitWriter.Write}},
            {typeof(UInt16), new GetSet<UInt16>{
                Getter = BitConverter.ToUInt16,
                Setter = BitWriter.Write}},
            {typeof(UInt32), new GetSet<UInt32>{
                Getter = BitConverter.ToUInt32,
                Setter = BitWriter.Write}},
            {typeof(Int32), new GetSet<Int32>{
                Getter = BitConverter.ToInt32,
                Setter = BitWriter.Write}},
            {typeof(Single), new GetSet<Single>{
                Getter = BitConverter.ToSingle,
                Setter = BitWriter.Write}},
            {typeof(System.Numerics.Vector2), new GetSet<System.Numerics.Vector2>{
                Getter = (array, start) =>
                    new System.Numerics.Vector2(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(System.Numerics.Vector3), new GetSet<System.Numerics.Vector3>{
                Getter = (array, start) =>
                    new System.Numerics.Vector3(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(System.Numerics.Vector4), new GetSet<System.Numerics.Vector4>{
                Getter = (array, start) =>
                    new System.Numerics.Vector4(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8),
                        BitConverter.ToSingle(array, start + 12)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(System.Numerics.Quaternion), new GetSet<System.Numerics.Quaternion>{
                Getter = (array, start) =>
                    new System.Numerics.Quaternion(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8),
                        BitConverter.ToSingle(array, start + 12)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(System.Numerics.Matrix4x4), new GetSet<System.Numerics.Matrix4x4>{
                Getter = (array, start) =>
                    new System.Numerics.Matrix4x4(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8),
                        BitConverter.ToSingle(array, start + 12),
                        BitConverter.ToSingle(array, start + 16),
                        BitConverter.ToSingle(array, start + 20),
                        BitConverter.ToSingle(array, start + 24),
                        BitConverter.ToSingle(array, start + 28),
                        BitConverter.ToSingle(array, start + 32),
                        BitConverter.ToSingle(array, start + 36),
                        BitConverter.ToSingle(array, start + 40),
                        BitConverter.ToSingle(array, start + 44),
                        BitConverter.ToSingle(array, start + 48),
                        BitConverter.ToSingle(array, start + 52),
                        BitConverter.ToSingle(array, start + 56),
                        BitConverter.ToSingle(array, start + 60)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(Byte4), new GetSet<Byte4>{
                Getter = (array, start) =>
                    new Byte4(
                        array[start],
                        array[start + 1],
                        array[start + 2],
                        array[start + 3]
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(UShort4), new GetSet<UShort4>{
                Getter = (array, start) =>
                    new UShort4(
                        BitConverter.ToUInt16(array, start),
                        BitConverter.ToUInt16(array, start + 2),
                        BitConverter.ToUInt16(array, start + 4),
                        BitConverter.ToUInt16(array, start + 6)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(SkinJoints), new GetSet<SkinJoints>{
                Getter = (array, start) =>
                    new SkinJoints
                    {
                        Joint0 = BitConverter.ToUInt16(array, start),
                        Joint1 = BitConverter.ToUInt16(array, start + 2),
                        Joint2 = BitConverter.ToUInt16(array, start + 4),
                        Joint3 = BitConverter.ToUInt16(array, start + 6),
                    },
                Setter = BitWriter.Write
            }},
            {typeof(UnityEngine.Vector2), new GetSet<UnityEngine.Vector2>{
                Getter = (array, start) =>
                    new UnityEngine.Vector2(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(UnityEngine.Vector3), new GetSet<UnityEngine.Vector3>{
                Getter = (array, start) =>
                    new UnityEngine.Vector3(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(UnityEngine.Vector4), new GetSet<UnityEngine.Vector4>{
                Getter = (array, start) =>
                    new UnityEngine.Vector4(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8),
                        BitConverter.ToSingle(array, start + 12)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(UnityEngine.Color), new GetSet<UnityEngine.Color>{
                Getter = (array, start) =>
                    new UnityEngine.Color(
                        BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 4),
                        BitConverter.ToSingle(array, start + 8),
                        BitConverter.ToSingle(array, start + 12)
                    ),
                Setter = BitWriter.Write
            }},
            {typeof(UnityEngine.Matrix4x4), new GetSet<UnityEngine.Matrix4x4>{
                Getter = (array, start) =>
                    // UnityEngine.Matrix4x4
                    // * 列優先メモリレイアウト(col0, col1, col2, col3)
                    // * Constructor 引き数が行優先 new Matrix4x4(row0, row1, row2, row3);
                    new UnityEngine.Matrix4x4(
                        new UnityEngine.Vector4(BitConverter.ToSingle(array, start),
                        BitConverter.ToSingle(array, start + 16),
                        BitConverter.ToSingle(array, start + 32),
                        BitConverter.ToSingle(array, start + 48)),
                        new UnityEngine.Vector4(BitConverter.ToSingle(array, start+4),
                        BitConverter.ToSingle(array, start + 16+4),
                        BitConverter.ToSingle(array, start + 32+4),
                        BitConverter.ToSingle(array, start + 48+4)),
                        new UnityEngine.Vector4(BitConverter.ToSingle(array, start+8),
                        BitConverter.ToSingle(array, start + 16+8),
                        BitConverter.ToSingle(array, start + 32+8),
                        BitConverter.ToSingle(array, start + 48+8)),
                        new UnityEngine.Vector4(BitConverter.ToSingle(array, start+12),
                        BitConverter.ToSingle(array, start + 16+12),
                        BitConverter.ToSingle(array, start + 32+12),
                        BitConverter.ToSingle(array, start + 48+12))
                    ),
                Setter = BitWriter.Write
            }},

        };

        public static SpanLike<T> Wrap<T>(ArraySegment<byte> bytes) where T : struct
        {
            if (!Map.TryGetValue(typeof(T), out object value))
            {
                throw new KeyNotFoundException($"{typeof(T)}");
            }
            var getset = (GetSet<T>)value;
            return new SpanLike<T>(bytes, Marshal.SizeOf<T>(), getset.Getter, getset.Setter);
        }

        public static SpanLike<T> Create<T>(int itemCount) where T : struct
        {
            var itemSize = Marshal.SizeOf<T>();
            var array = new byte[itemCount * itemSize];
            return Wrap<T>(new ArraySegment<byte>(array));
        }

        public static SpanLike<T> CopyFrom<T>(T[] src) where T : struct
        {
            var buffer = new byte[src.Length * Marshal.SizeOf<T>()];
            src.ToBytes(new ArraySegment<byte>(buffer));
            return Wrap<T>(new ArraySegment<byte>(buffer));
        }
    }
}
