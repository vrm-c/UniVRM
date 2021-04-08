using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using UniGLTF;

namespace VrmLib
{
    public enum AccessorVectorType
    {
        SCALAR,
        VEC2,
        VEC3,
        VEC4,
        MAT2,
        MAT3,
        MAT4,
    }

    public static class GltfAccessorTypeExtensions
    {
        public static int TypeCount(this AccessorVectorType type)
        {
            switch (type)
            {
                case AccessorVectorType.SCALAR:
                    return 1;
                case AccessorVectorType.VEC2:
                    return 2;
                case AccessorVectorType.VEC3:
                    return 3;
                case AccessorVectorType.VEC4:
                case AccessorVectorType.MAT2:
                    return 4;
                case AccessorVectorType.MAT3:
                    return 9;
                case AccessorVectorType.MAT4:
                    return 16;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum AccessorValueType : int
    {
        BYTE = 5120, // signed ?
        UNSIGNED_BYTE = 5121,

        SHORT = 5122,
        UNSIGNED_SHORT = 5123,

        //INT = 5124,
        UNSIGNED_INT = 5125,

        FLOAT = 5126,
    }

    public static class GltfComponentTypeExtensions
    {
        public static int ByteSize(this AccessorValueType t)
        {
            switch (t)
            {
                case AccessorValueType.BYTE: return 1;
                case AccessorValueType.UNSIGNED_BYTE: return 4;
                case AccessorValueType.SHORT: return 2;
                case AccessorValueType.UNSIGNED_SHORT: return 2;
                case AccessorValueType.UNSIGNED_INT: return 4;
                case AccessorValueType.FLOAT: return 4;
                default: throw new ArgumentException();
            }
        }
    }


    public class BufferAccessor
    {
        public ArraySegment<byte> Bytes;

        public AccessorValueType ComponentType;

        public AccessorVectorType AccessorType;

        public int Stride => ComponentType.ByteSize() * AccessorType.TypeCount();

        public int Count;

        public int ByteLength => Stride * Count;

        public override string ToString()
        {
            return $"{Stride}stride x{Count}";
        }

        public SpanLike<T> GetSpan<T>(bool checkStride = true) where T : struct
        {
            if (checkStride && Marshal.SizeOf(typeof(T)) != Stride)
            {
                throw new Exception("different sizeof(T) with stride");
            }
            return SpanLike.Wrap<T>(Bytes);
        }

        public void Assign<T>(T[] values) where T : struct
        {
            if (Marshal.SizeOf(typeof(T)) != Stride)
            {
                throw new Exception("invalid element size");
            }
            var array = new byte[Stride * values.Length];
            Bytes = new ArraySegment<byte>(array);
            values.ToBytes(Bytes);
            Count = values.Length;
        }

        public void Assign<T>(SpanLike<T> values) where T : struct
        {
            if (Marshal.SizeOf(typeof(T)) != Stride)
            {
                throw new Exception("invalid element size");
            }
            Bytes = values.Bytes;
            Count = values.Length;
        }

        // for index buffer
        public void AssignAsShort(SpanLike<int> values)
        {
            if (AccessorType != AccessorVectorType.SCALAR)
            {
                throw new NotImplementedException();
            }
            ComponentType = AccessorValueType.UNSIGNED_SHORT;

            Bytes = new ArraySegment<byte>(new byte[Stride * values.Length]);
            var span = GetSpan<ushort>();
            Count = values.Length;
            for (int i = 0; i < values.Length; ++i)
            {
                span[i] = (ushort)values[i];
            }
        }

        // Index用
        public int[] GetAsIntArray()
        {
            if (AccessorType != AccessorVectorType.SCALAR)
            {
                throw new InvalidOperationException("not scalar");
            }
            switch (ComponentType)
            {
                case AccessorValueType.UNSIGNED_SHORT:
                    {
                        var span = SpanLike.Wrap<UInt16>(Bytes);
                        var array = new int[span.Length];
                        for (int i = 0; i < span.Length; ++i)
                        {
                            array[i] = span[i];
                        }
                        return array;
                    }

                case AccessorValueType.UNSIGNED_INT:
                    return SpanLike.Wrap<Int32>(Bytes).ToArray();

                default:
                    throw new NotImplementedException();
            }
        }

        public List<int> GetAsIntList()
        {
            if (AccessorType != AccessorVectorType.SCALAR)
            {
                throw new InvalidOperationException("not scalar");
            }
            switch (ComponentType)
            {
                case AccessorValueType.UNSIGNED_SHORT:
                    {
                        var span = SpanLike.Wrap<UInt16>(Bytes);
                        var array = new List<int>(Count);
                        if (span.Length != Count)
                        {
                            for (int i = 0; i < Count; ++i)
                            {
                                array.Add(span[i]);
                            }
                        }
                        else
                        {
                            // Spanが動かない？WorkAround
                            var bytes = Bytes.ToArray();
                            var offset = 0;
                            for (int i = 0; i < Count; ++i)
                            {
                                array.Add(BitConverter.ToUInt16(bytes, offset));
                                offset += 2;
                            }
                        }
                        return array;
                    }

                case AccessorValueType.UNSIGNED_INT:
                    return SpanLike.Wrap<Int32>(Bytes).ToArray().ToList();

                default:
                    throw new NotImplementedException();
            }
        }

        // Joints用
        public UShort4[] GetAsUShort4()
        {
            if (AccessorType != AccessorVectorType.VEC4)
            {
                throw new InvalidOperationException("not vec4");
            }
            switch (ComponentType)
            {
                case AccessorValueType.UNSIGNED_SHORT:
                    return SpanLike.Wrap<UShort4>(Bytes).ToArray();

                case AccessorValueType.UNSIGNED_BYTE:
                    {
                        var array = new UShort4[Count];
                        var span = SpanLike.Wrap<Byte4>(Bytes);
                        for (int i = 0; i < span.Length; ++i)
                        {
                            array[i].X = span[i].X;
                            array[i].Y = span[i].Y;
                            array[i].Z = span[i].Z;
                            array[i].W = span[i].W;
                        }
                        return array;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        // Weigt用
        public Vector4[] GetAsVector4()
        {
            if (AccessorType != AccessorVectorType.VEC4)
            {
                throw new InvalidOperationException("not vec4");
            }
            switch (ComponentType)
            {
                case AccessorValueType.FLOAT:
                    return SpanLike.Wrap<Vector4>(Bytes).ToArray();

                default:
                    throw new NotImplementedException();
            }
        }

        public void Resize(int count)
        {
            if (count < Count)
            {
                throw new Exception();
            }
            ToByteLength(Stride * count);

            Count = count;
        }

        void ToByteLength(int byteLength)
        {
            var newBytes = new byte[byteLength];
            Buffer.BlockCopy(Bytes.Array, Bytes.Offset, newBytes, 0, Bytes.Count);
            Bytes = new ArraySegment<byte>(newBytes);
        }

        public void Extend(int count)
        {
            var oldLength = Bytes.Count;
            ToByteLength(oldLength + Stride * count);
            Count += count;
        }

        //
        // ArraySegment<byte> を新規に確保して置き換える
        //
        public void Append(BufferAccessor a, int offset = -1)
        {
            if (AccessorType != a.AccessorType)
            {
                System.Console.WriteLine(AccessorType.ToString() + "!=" + a.AccessorType.ToString());
                throw new Exception("different AccessorType");
            }

            // UNSIGNED_SHORT <-> UNSIGNED_INT の変換を許容して処理を続行
            // 統合メッシュのprimitiveのIndexBufferが65,535（ushort.MaxValue)を超える場合や、変換前にindexBuffer.ComponetTypeがushortとuint混在する場合など
            if (ComponentType != a.ComponentType)
            {
                switch (a.ComponentType)
                {
                    //ushort to uint
                    case AccessorValueType.UNSIGNED_SHORT:
                        {
                            var src = SpanLike.Wrap<UInt16>(a.Bytes).Slice(0, a.Count);
                            var bytes = new byte[src.Length * 4];
                            var dst = SpanLike.Wrap<UInt32>(new ArraySegment<byte>(bytes));
                            for (int i = 0; i < src.Length; ++i)
                            {
                                dst[i] = (uint)src[i];
                            }
                            var accessor = new BufferAccessor(new ArraySegment<byte>(bytes), AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, a.Count);
                            a = accessor;

                            break;
                        }

                    //uint to ushort (おそらく通ることはない)
                    case AccessorValueType.UNSIGNED_INT:
                        {
                            var src = SpanLike.Wrap<UInt32>(a.Bytes).Slice(0, a.Count);
                            var bytes = new byte[src.Length * 2];
                            var dst = SpanLike.Wrap<UInt16>(new ArraySegment<byte>(bytes));
                            for (int i = 0; i < src.Length; ++i)
                            {
                                dst[i] = (ushort)src[i];
                            }
                            var accessor = new BufferAccessor(new ArraySegment<byte>(bytes), ComponentType, AccessorVectorType.SCALAR, a.Count);
                            a = accessor;
                            break;
                        }

                    default:
                        throw new Exception("Cannot Convert ComponentType");

                }
            }

            // 連結した新しいバッファを確保
            var oldLength = Bytes.Count;
            ToByteLength(oldLength + a.Bytes.Count);
            // 後ろにコピー
            Buffer.BlockCopy(a.Bytes.Array, a.Bytes.Offset, Bytes.Array, Bytes.Offset + oldLength, a.Bytes.Count);
            Count += a.Count;

            if (offset > 0)
            {
                // 後半にoffsetを足す
                switch (ComponentType)
                {
                    case AccessorValueType.UNSIGNED_SHORT:
                        {
                            var span = SpanLike.Wrap<UInt16>(Bytes.Slice(oldLength));
                            var ushortOffset = (ushort)offset;
                            for (int i = 0; i < span.Length; ++i)
                            {
                                span[i] += ushortOffset;
                            }
                        }
                        break;

                    case AccessorValueType.UNSIGNED_INT:
                        {
                            var span = SpanLike.Wrap<UInt32>(Bytes.Slice(oldLength));
                            var uintOffset = (uint)offset;
                            for (int i = 0; i < span.Length; ++i)
                            {
                                span[i] += uintOffset;
                            }
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public BufferAccessor Skip(int skipFrames)
        {
            skipFrames = Math.Min(Count, skipFrames);
            if (skipFrames == 0)
            {
                return this;
            }

            return new BufferAccessor(Bytes.Slice(Stride * skipFrames), ComponentType, AccessorType, Count - skipFrames);
        }

        public BufferAccessor CloneWithOffset(int offsetCount)
        {
            var offsetSize = Stride * offsetCount;
            var buffer = new byte[offsetSize + Bytes.Count];

            Buffer.BlockCopy(Bytes.Array, Bytes.Offset, buffer, offsetSize, Bytes.Count);

            return new BufferAccessor(new ArraySegment<byte>(buffer), ComponentType, AccessorType, Count + offsetCount);
        }

        public BufferAccessor(ArraySegment<byte> bytes, AccessorValueType componentType, AccessorVectorType accessorType, int count)
        {
            Bytes = bytes;
            ComponentType = componentType;
            AccessorType = accessorType;
            Count = count;
        }

        public static BufferAccessor Create<T>(T[] list) where T : struct
        {
            var t = typeof(T);
            var bytes = new byte[list.Length * Marshal.SizeOf(t)];
            var span = SpanLike.Wrap<T>(new ArraySegment<byte>(bytes));
            for (int i = 0; i < list.Length; ++i)
            {
                span[i] = list[i];
            }
            AccessorValueType componentType = default(AccessorValueType);
            AccessorVectorType accessorType = default(AccessorVectorType);
            if (t == typeof(Vector2))
            {
                componentType = AccessorValueType.FLOAT;
                accessorType = AccessorVectorType.VEC2;
            }
            else if (t == typeof(Vector3))
            {
                componentType = AccessorValueType.FLOAT;
                accessorType = AccessorVectorType.VEC3;
            }
            else if (t == typeof(Vector4))
            {
                componentType = AccessorValueType.FLOAT;
                accessorType = AccessorVectorType.VEC4;
            }
            else if (t == typeof(Quaternion))
            {
                componentType = AccessorValueType.FLOAT;
                accessorType = AccessorVectorType.VEC4;
            }
            else if (t == typeof(SkinJoints))
            {
                componentType = AccessorValueType.UNSIGNED_SHORT;
                accessorType = AccessorVectorType.VEC4;
            }
            else if (t == typeof(int))
            {
                componentType = AccessorValueType.UNSIGNED_INT;
                accessorType = AccessorVectorType.SCALAR;
            }
            else
            {
                throw new NotImplementedException();
            }
            return new BufferAccessor(
                new ArraySegment<byte>(bytes), componentType, accessorType, list.Length);
        }

        public void AddTo(Dictionary<string, BufferAccessor> dict, string key)
        {
            dict.Add(key, this);
        }
    }
}
