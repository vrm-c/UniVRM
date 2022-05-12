using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace UniGLTF
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

        public NativeArray<byte> Bytes { get; private set; }

        public AccessorValueType ComponentType { get; private set; }

        public AccessorVectorType AccessorType { get; private set; }

        public int Stride => ComponentType.ByteSize() * AccessorType.TypeCount();

        public int Count { get; private set; }

        public int ByteLength => Stride * Count;

        public BufferAccessor(INativeArrayManager arrayManager, NativeArray<byte> bytes, AccessorValueType componentType, AccessorVectorType accessorType, int count)
        {
            ArrayManager = arrayManager;
            Bytes = bytes;
            ComponentType = componentType;
            AccessorType = accessorType;
            Count = count;
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
                case AccessorValueType.UNSIGNED_BYTE:
                    return ArrayManager.Convert(Bytes, (byte x) => (int)x);

                case AccessorValueType.UNSIGNED_SHORT:
                    return ArrayManager.Convert(Bytes.Reinterpret<ushort>(1), (ushort x) => (int)x);

                case AccessorValueType.UNSIGNED_INT:
                    return Bytes.Reinterpret<Int32>(1);

                default:
                    throw new NotImplementedException();
            }
        }

        // BoneWeight用
        public NativeArray<Vector4> GetAsVector4Array()
        {
            if (AccessorType != AccessorVectorType.VEC4)
            {
                throw new InvalidOperationException("not VEC4");
            }
            switch (ComponentType)
            {
                case AccessorValueType.FLOAT:
                    return Bytes.Reinterpret<Vector4>(1);

                case AccessorValueType.UNSIGNED_BYTE:
                    return ArrayManager.Convert(Bytes.Reinterpret<Byte4>(1), (Byte4 b) => new Vector4(b.x / 255.0f, b.y / 255.0f, b.z / 255.0f, b.w / 255.0f));

                case AccessorValueType.UNSIGNED_SHORT:
                    return ArrayManager.Convert(Bytes.Reinterpret<UShort4>(1), (UShort4 b) => new Vector4(b.x / 65535.0f, b.y / 65535.0f, b.z / 65535.0f, b.w / 65535.0f));

                default:
                    throw new NotImplementedException();
            }
        }

        // BoneJoint用
        public NativeArray<SkinJoints> GetAsSkinJointsArray()
        {
            if (AccessorType != AccessorVectorType.VEC4)
            {
                throw new InvalidOperationException("not VEC4");
            }
            switch (ComponentType)
            {
                case AccessorValueType.UNSIGNED_BYTE:
                    return ArrayManager.Convert(Bytes.Reinterpret<Byte4>(1), (Byte4 b) => new SkinJoints(b.x, b.y, b.z, b.w));

                case AccessorValueType.UNSIGNED_SHORT:
                    return Bytes.Reinterpret<SkinJoints>(1);

                default:
                    throw new NotImplementedException();
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

        int AddViewTo(ExportingGltfData data, int bufferIndex,
            int offset = 0, int count = 0)
        {
            var stride = Stride;
            if (count == 0)
            {
                count = Count;
            }
            var slice = Bytes.GetSubArray(offset * stride, count * stride);
            return data.AppendToBuffer(slice);
        }

        glTFAccessor CreateGltfAccessor(int viewIndex, int count = 0, int byteOffset = 0)
        {
            if (count == 0)
            {
                count = Count;
            }
            return new glTFAccessor
            {
                bufferView = viewIndex,
                byteOffset = byteOffset,
                componentType = (glComponentType)ComponentType,
                type = AccessorType.ToString(),
                count = count,
            };
        }

        int AddAccessorTo(ExportingGltfData data, int viewIndex,
            Action<NativeArray<byte>, glTFAccessor> minMax = null,
            int offset = 0, int count = 0)
        {
            var gltf = data.Gltf;
            var accessorIndex = gltf.accessors.Count;
            var accessor = CreateGltfAccessor(viewIndex, count, offset * Stride);
            if (minMax != null)
            {
                minMax(Bytes, accessor);
            }
            gltf.accessors.Add(accessor);
            return accessorIndex;
        }

        public int AddAccessorTo(ExportingGltfData data, int bufferIndex,
            // GltfBufferTargetType targetType,
            bool useSparse,
            Action<NativeArray<byte>, glTFAccessor> minMax = null,
            int offset = 0, int count = 0)
        {
            if (ComponentType == AccessorValueType.FLOAT
            && AccessorType == AccessorVectorType.VEC3
            )
            {
                var values = GetSpan<Vector3>();
                // 巨大ポリゴンのモデル対策にValueTupleの型をushort -> uint へ
                var sparseValuesWithIndex = new List<ValueTuple<int, Vector3>>();
                for (int i = 0; i < values.Length; ++i)
                {
                    var v = values[i];
                    if (v != Vector3.zero)
                    {
                        sparseValuesWithIndex.Add((i, v));
                    }
                }

                //var status = $"{sparseIndices.Count * 14}/{values.Length * 12}";
                if (useSparse
                && sparseValuesWithIndex.Count > 0 // avoid empty sparse
                && sparseValuesWithIndex.Count * 16 < values.Length * 12)
                {
                    // use sparse
                    using (var sparseIndexBin = new NativeArray<byte>(sparseValuesWithIndex.Count * 4, Allocator.Persistent))
                    using (var sparseValueBin = new NativeArray<byte>(sparseValuesWithIndex.Count * 12, Allocator.Persistent))
                    {
                        var sparseIndexSpan = sparseIndexBin.Reinterpret<Int32>(1);
                        var sparseValueSpan = sparseValueBin.Reinterpret<Vector3>(1);

                        for (int i = 0; i < sparseValuesWithIndex.Count; ++i)
                        {
                            var (index, value) = sparseValuesWithIndex[i];
                            sparseIndexSpan[i] = index;
                            sparseValueSpan[i] = value;
                        }

                        var sparseIndexView = data.AppendToBuffer(sparseIndexBin);
                        var sparseValueView = data.AppendToBuffer(sparseValueBin);

                        var accessorIndex = data.Gltf.accessors.Count;
                        var accessor = new glTFAccessor
                        {
                            componentType = (glComponentType)ComponentType,
                            type = AccessorType.ToString(),
                            count = Count,
                            byteOffset = -1,
                            sparse = new glTFSparse
                            {
                                count = sparseValuesWithIndex.Count,
                                indices = new glTFSparseIndices
                                {
                                    componentType = (glComponentType)AccessorValueType.UNSIGNED_INT,
                                    bufferView = sparseIndexView,
                                },
                                values = new glTFSparseValues
                                {
                                    bufferView = sparseValueView,
                                },
                            }
                        };
                        if (minMax != null)
                        {
                            minMax(sparseValueBin, accessor);
                        }
                        data.Gltf.accessors.Add(accessor);
                        return accessorIndex;
                    }
                }
            }

            var viewIndex = AddViewTo(data, bufferIndex, offset, count);
            return AddAccessorTo(data, viewIndex, minMax, 0, count);
        }
    }
}
