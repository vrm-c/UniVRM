using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct UShort4
    {
        public ushort x;
        public ushort y;
        public ushort z;
        public ushort w;

        public UShort4(ushort _x, ushort _y, ushort _z, ushort _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
    }

    public static class glTFExtensions
    {
        struct ComponentVec
        {
            public glComponentType ComponentType;
            public int ElementCount;

            public ComponentVec(glComponentType componentType, int elementCount)
            {
                ComponentType = componentType;
                ElementCount = elementCount;
            }
        }

        static Dictionary<Type, ComponentVec> ComponentTypeMap = new Dictionary<Type, ComponentVec>
        {
            { typeof(Vector2), new ComponentVec(glComponentType.FLOAT, 2) },
            { typeof(Vector3), new ComponentVec(glComponentType.FLOAT, 3) },
            { typeof(Vector4), new ComponentVec(glComponentType.FLOAT, 4) },
            { typeof(UShort4), new ComponentVec(glComponentType.UNSIGNED_SHORT, 4) },
            { typeof(Matrix4x4), new ComponentVec(glComponentType.FLOAT, 16) },
            { typeof(Color), new ComponentVec(glComponentType.FLOAT, 4) },
        };

        static glComponentType GetComponentType<T>()
        {
            var cv = default(ComponentVec);
            if (ComponentTypeMap.TryGetValue(typeof(T), out cv))
            {
                return cv.ComponentType;
            }
            else if (typeof(T) == typeof(uint))
            {
                return glComponentType.UNSIGNED_INT;
            }
            else if (typeof(T) == typeof(float))
            {
                return glComponentType.FLOAT;
            }
            else
            {
                throw new NotImplementedException(typeof(T).Name);
            }
        }

        static string GetAccessorType<T>()
        {
            var cv = default(ComponentVec);
            if (ComponentTypeMap.TryGetValue(typeof(T), out cv))
            {
                switch (cv.ElementCount)
                {
                    case 2: return "VEC2";
                    case 3: return "VEC3";
                    case 4: return "VEC4";
                    case 16: return "MAT4";
                    default: throw new Exception();
                }
            }
            else
            {
                return "SCALAR";
            }
        }

        static int GetAccessorElementCount<T>()
        {
            var cv = default(ComponentVec);
            if (ComponentTypeMap.TryGetValue(typeof(T), out cv))
            {
                return cv.ElementCount;
            }
            else
            {
                return 1;
            }
        }

        public static int ExtendBufferAndGetAccessorIndex<T>(this glTF gltf, int bufferIndex, T[] array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            return gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, new ArraySegment<T>(array), target);
        }

        public static int ExtendBufferAndGetAccessorIndex<T>(this glTF gltf, int bufferIndex,
            ArraySegment<T> array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            if (array.Count == 0)
            {
                return -1;
            }
            var viewIndex = ExtendBufferAndGetViewIndex(gltf, bufferIndex, array, target);

            // index buffer's byteStride is unnecessary
            gltf.bufferViews[viewIndex].byteStride = 0;

            var accessorIndex = gltf.accessors.Count;
            gltf.accessors.Add(new glTFAccessor
            {
                bufferView = viewIndex,
                byteOffset = 0,
                componentType = GetComponentType<T>(),
                type = GetAccessorType<T>(),
                count = array.Count,
            });
            return accessorIndex;
        }

        public static int ExtendBufferAndGetViewIndex<T>(this glTF gltf, int bufferIndex,
            T[] array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            return ExtendBufferAndGetViewIndex(gltf, bufferIndex, new ArraySegment<T>(array), target);
        }

        public static int ExtendBufferAndGetViewIndex<T>(this glTF gltf, int bufferIndex,
            ArraySegment<T> array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            if (array.Count == 0)
            {
                return -1;
            }
            var view = gltf.buffers[bufferIndex].Append(array, target);
            var viewIndex = gltf.bufferViews.Count;
            gltf.bufferViews.Add(view);
            return viewIndex;
        }

        public static int ExtendSparseBufferAndGetAccessorIndex<T>(this glTF gltf, int bufferIndex,
            int accessorCount,
            T[] sparseValues, int[] sparseIndices, int sparseViewIndex,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            return ExtendSparseBufferAndGetAccessorIndex(gltf, bufferIndex, 
                accessorCount,
                new ArraySegment<T>(sparseValues), sparseIndices, sparseViewIndex,
                target);
        }

        public static int ExtendSparseBufferAndGetAccessorIndex<T>(this glTF gltf, int bufferIndex,
            int accessorCount,
            ArraySegment<T> sparseValues, int[] sparseIndices, int sparseIndicesViewIndex,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            if (sparseValues.Count == 0)
            {
                return -1;
            }
            var sparseValuesViewIndex = ExtendBufferAndGetViewIndex(gltf, bufferIndex, sparseValues, target);
            var accessorIndex = gltf.accessors.Count;
            gltf.accessors.Add(new glTFAccessor
            {
                byteOffset = 0,
                componentType = GetComponentType<T>(),
                type = GetAccessorType<T>(),
                count = accessorCount,

                sparse = new glTFSparse
                {
                    count=sparseIndices.Length,
                    indices = new glTFSparseIndices
                    {
                        bufferView = sparseIndicesViewIndex,
                        componentType = glComponentType.UNSIGNED_INT
                    },
                    values = new glTFSparseValues
                    {
                        bufferView = sparseValuesViewIndex,                       
                    }
                }
            });
            return accessorIndex;
        }
    }
}
