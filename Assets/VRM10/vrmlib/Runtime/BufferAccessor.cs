using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UniGLTF;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

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
                case AccessorValueType.UNSIGNED_BYTE: return 1;
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
        public INativeArrayManager ArrayManager { get; }

        public NativeArray<byte> Bytes;

        public AccessorValueType ComponentType;

        public AccessorVectorType AccessorType;

        public int Stride => ComponentType.ByteSize() * AccessorType.TypeCount();

        public int Count;

        public int ByteLength => Stride * Count;

        public BufferAccessor(INativeArrayManager arrayManager, NativeArray<byte> bytes, AccessorValueType componentType, AccessorVectorType accessorType, int count)
        {
            ArrayManager = arrayManager;
            Bytes = bytes;
            ComponentType = componentType;
            AccessorType = accessorType;
            Count = count;
        }

        public static BufferAccessor Create<T>(INativeArrayManager arrayManager, T[] list) where T : struct
        {
            var t = typeof(T);
            var bytes = arrayManager.CreateNativeArray<byte>(list.Length * Marshal.SizeOf(t));
            var span = bytes.Reinterpret<T>(1);
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
            return new BufferAccessor(arrayManager, bytes, componentType, accessorType, list.Length);
        }

        public override string ToString()
        {
            return $"{Stride}stride x{Count}";
        }

        public NativeArray<T> GetSpan<T>(bool checkStride = true) where T : struct
        {
            if (checkStride && Marshal.SizeOf(typeof(T)) != Stride)
            {
                throw new Exception("different sizeof(T) with stride");
            }
            return Bytes.Reinterpret<T>(1);
        }

        /// <summary>
        /// バッファをNativeSliceへと書き込む
        /// </summary>
        public unsafe void CopyToNativeSlice<T>(NativeSlice<T> destArray) where T : unmanaged
        {
            var byteArray = NativeArrayUnsafeUtility.GetUnsafePtr(Bytes);
            UnsafeUtility.MemCpy((T*)destArray.GetUnsafePtr(), byteArray, Bytes.Length);
        }

        public void Assign<T>(T[] values) where T : struct
        {
            if (Marshal.SizeOf(typeof(T)) != Stride)
            {
                throw new Exception("invalid element size");
            }
            Bytes = ArrayManager.CreateNativeArray<byte>(Stride * values.Length);
            Count = values.Length;
            Bytes.Reinterpret<T>(1).CopyFrom(values);
        }

        public void Assign<T>(NativeArray<T> values) where T : struct
        {
            if (Marshal.SizeOf(typeof(T)) != Stride)
            {
                throw new Exception("invalid element size");
            }
            Bytes = ArrayManager.CreateNativeArray<byte>(Marshal.SizeOf<T>() * values.Length);
            NativeArray<T>.Copy(values, Bytes.Reinterpret<T>(1));
            Count = values.Length;
        }

        // for index buffer
        public void AssignAsShort(NativeArray<int> values)
        {
            if (AccessorType != AccessorVectorType.SCALAR)
            {
                throw new NotImplementedException();
            }
            ComponentType = AccessorValueType.UNSIGNED_SHORT;

            Bytes = ArrayManager.Convert(values, (int x) => (ushort)x).Reinterpret<Byte>(Marshal.SizeOf<ushort>());
            Count = values.Length;
        }

        // Index用
        public NativeArray<int> GetAsIntArray()
        {
            if (AccessorType != AccessorVectorType.SCALAR)
            {
                throw new InvalidOperationException("not scalar");
            }
            switch (ComponentType)
            {
                case AccessorValueType.UNSIGNED_SHORT:
                    return ArrayManager.Convert(Bytes.Reinterpret<ushort>(1), (ushort x) => (int)x);

                case AccessorValueType.UNSIGNED_INT:
                    return Bytes.Reinterpret<Int32>(1);

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
            var newBytes = ArrayManager.CreateNativeArray<byte>(byteLength);
            NativeArray<byte>.Copy(Bytes, newBytes);
            Bytes = newBytes;
        }

        public void Extend(int count)
        {
            var oldLength = Bytes.Length;
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
                            var bytes = ArrayManager.Convert(a.Bytes.Reinterpret<UInt16>(1), (UInt16 x) => (UInt32)x).Reinterpret<byte>(Marshal.SizeOf<UInt32>());
                            var accessor = new BufferAccessor(ArrayManager, bytes, AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, a.Count);
                            a = accessor;
                            break;
                        }

                    //uint to ushort (おそらく通ることはない)
                    case AccessorValueType.UNSIGNED_INT:
                        {
                            var bytes = ArrayManager.Convert(a.Bytes.Reinterpret<UInt32>(1), (UInt32 x) => (UInt16)x).Reinterpret<byte>(Marshal.SizeOf<UInt16>());
                            var accessor = new BufferAccessor(ArrayManager, bytes, ComponentType, AccessorVectorType.SCALAR, a.Count);
                            a = accessor;
                            break;
                        }

                    default:
                        throw new Exception("Cannot Convert ComponentType");

                }
            }

            // 連結した新しいバッファを確保
            var oldLength = Bytes.Length;
            ToByteLength(oldLength + a.Bytes.Length);
            // 後ろにコピー
            NativeArray<byte>.Copy(a.Bytes, Bytes.GetSubArray(oldLength, Bytes.Length - oldLength));
            Count += a.Count;

            if (offset > 0)
            {
                // 後半にoffsetを足す
                switch (ComponentType)
                {
                    case AccessorValueType.UNSIGNED_SHORT:
                        {
                            var span = Bytes.GetSubArray(oldLength, Bytes.Length - oldLength).Reinterpret<UInt16>(1);
                            var ushortOffset = (ushort)offset;
                            for (int i = 0; i < span.Length; ++i)
                            {
                                span[i] += ushortOffset;
                            }
                        }
                        break;

                    case AccessorValueType.UNSIGNED_INT:
                        {
                            var span = Bytes.GetSubArray(oldLength, Bytes.Length - oldLength).Reinterpret<UInt32>(1);
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
            var start = Stride * skipFrames;
            return new BufferAccessor(ArrayManager, Bytes.GetSubArray(start, Bytes.Length - start), ComponentType, AccessorType, Count - skipFrames);
        }

        public BufferAccessor CloneWithOffset(int offsetCount)
        {
            var offsetSize = Stride * offsetCount;
            var buffer = ArrayManager.CreateNativeArray<byte>(offsetSize + Bytes.Length);
            NativeArray<byte>.Copy(Bytes, buffer.GetSubArray(offsetSize, buffer.Length - offsetSize));
            return new BufferAccessor(ArrayManager, buffer, ComponentType, AccessorType, Count + offsetCount);
        }

        public void AddTo(Dictionary<string, BufferAccessor> dict, string key)
        {
            dict.Add(key, this);
        }
    }
}
