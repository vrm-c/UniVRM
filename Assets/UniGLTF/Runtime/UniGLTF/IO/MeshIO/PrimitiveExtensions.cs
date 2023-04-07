using System;
using Unity.Collections;
using UnityEngine;

namespace UniGLTF
{
    internal static class PrimitiveExtensions
    {
        public static bool HasNormal(this glTFPrimitives primitives) => primitives.attributes.NORMAL != -1;
        public static bool HasTexCoord0(this glTFPrimitives primitives) => primitives.attributes.TEXCOORD_0 != -1;
        public static bool HasTexCoord1(this glTFPrimitives primitives) => primitives.attributes.TEXCOORD_1 != -1;
        public static bool HasSkin(this glTFPrimitives primitives) => primitives.attributes.JOINTS_0 != -1 && primitives.attributes.WEIGHTS_0 != -1;
        public static bool HasColor(this glTFPrimitives primitives) => primitives.attributes.COLOR_0 != -1;

        public static NativeArray<Vector3> GetPositions(this glTFPrimitives primitives, GltfData data)
        {
            var accessor = data.GLTF.accessors[primitives.attributes.POSITION];
            if (accessor.type != "VEC3")
            {
                throw new ArgumentException($"unknown POSITION type: {accessor.componentType}:{accessor.type}");
            }

            if (accessor.componentType == glComponentType.FLOAT)
            {
                return data.GetArrayFromAccessor<Vector3>(primitives.attributes.POSITION);
            }
            else if (accessor.componentType == glComponentType.UNSIGNED_SHORT)
            {
                // KHR_mesh_quantization
                // not UShort3 for 4byte alignment !
                var src = data.GetArrayFromAccessor<UShort4>(primitives.attributes.POSITION);
                var array = data.NativeArrayManager.CreateNativeArray<Vector3>(src.Length);
                for (int i = 0; i < src.Length; ++i)
                {
                    var v = src[i];
                    array[i] = new Vector3(v.x, v.y, v.z);
                }
                return array;
            }
            else
            {
                throw new NotImplementedException($"unknown POSITION type: {accessor.componentType}:{accessor.type}");
            }
        }

        public static NativeArray<Vector3>? GetNormals(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasNormal(primitives)) return null;

            var accessor = data.GLTF.accessors[primitives.attributes.NORMAL];
            if (accessor.type != "VEC3")
            {
                throw new ArgumentException($"unknown NORMAL type: {accessor.componentType}:{accessor.type}");
            }

            if (accessor.componentType == glComponentType.FLOAT)
            {
                var result = data.GetArrayFromAccessor<Vector3>(primitives.attributes.NORMAL);
                if (result.Length != positionsLength)
                {
                    throw new Exception("NORMAL is different in length from POSITION");
                }
                return result;
            }
            else if (accessor.componentType == glComponentType.BYTE)
            {
                // KHR_mesh_quantization
                // not Byte3 for 4byte alignment !
                var src = data.GetArrayFromAccessor<SByte4>(primitives.attributes.NORMAL);
                var array = data.NativeArrayManager.CreateNativeArray<Vector3>(src.Length);
                if (accessor.normalized)
                {
                    var factor = 1.0f / 127.0f;
                    for (int i = 0; i < src.Length; ++i)
                    {
                        var v = src[i];
                        array[i] = new Vector3(
                            v.x * factor,
                            v.y * factor,
                            v.z * factor);
                    }
                }
                else
                {
                    for (int i = 0; i < src.Length; ++i)
                    {
                        var v = src[i];
                        array[i] = new Vector3(v.x, v.y, v.z);
                    }
                }
                return array;
            }
            else
            {
                throw new NotImplementedException($"unknown NORMAL type: {accessor.componentType}:{accessor.type}");
            }
        }

        public static NativeArray<Vector2>? GetTexCoords0(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasTexCoord0(primitives)) return null;

            var accessor = data.GLTF.accessors[primitives.attributes.TEXCOORD_0];
            if (accessor.type != "VEC2")
            {
                throw new ArgumentException($"unknown TEXCOORD_0 type: {accessor.componentType}:{accessor.type}");
            }

            if (accessor.componentType == glComponentType.FLOAT)
            {
                var result = data.GetArrayFromAccessor<Vector2>(primitives.attributes.TEXCOORD_0);
                if (result.Length != positionsLength)
                {
                    throw new Exception("different length");
                }

                return result;
            }
            if (accessor.componentType == glComponentType.UNSIGNED_SHORT)
            {
                // KHR_mesh_quantization
                var src = data.GetArrayFromAccessor<UShort2>(primitives.attributes.TEXCOORD_0);
                var array = data.NativeArrayManager.CreateNativeArray<Vector2>(src.Length);
                for (int i = 0; i < src.Length; ++i)
                {
                    var v = src[i];
                    array[i] = new Vector2(v.x, v.y);
                }
                return array;
            }
            else
            {
                throw new ArgumentException($"unknown TEXCOORD_0 type: {accessor.componentType}:{accessor.type}");
            }
        }

        public static NativeArray<Vector2>? GetTexCoords1(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasTexCoord1(primitives)) return null;
            var result = data.GetArrayFromAccessor<Vector2>(primitives.attributes.TEXCOORD_1);
            if (result.Length != positionsLength)
            {
                throw new Exception("different length");
            }

            return result;
        }

        /// <summary>
        /// glTF 仕様では VEC3/VEC4 x float/unsigned byte normalized/unsigned short normalized
        /// がある。
        /// </summary>
        public static NativeArray<Color>? GetColors(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasColor(primitives)) return null;

            var accessor = data.GLTF.accessors[primitives.attributes.COLOR_0];
            switch (accessor.componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    return GetColorsUInt8(primitives, data, positionsLength);
                case glComponentType.UNSIGNED_SHORT:
                    return GetColorsUInt16(primitives, data, positionsLength);
                case glComponentType.FLOAT:
                    return GetColorsFloat(primitives, data, positionsLength);
                default:
                    throw new NotImplementedException(
                        $"unknown color type {accessor.componentType}");
            }
        }

        /// unsigned byte normalized => f = c / 255.0
        public static NativeArray<Color>? GetColorsUInt8(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasColor(primitives)) return null;

            var accessor = data.GLTF.accessors[primitives.attributes.COLOR_0];
            switch (accessor.TypeCount)
            {
                case 3:
                    {
                        var vec3Color = data.GetArrayFromAccessor<Byte3>(primitives.attributes.COLOR_0);
                        if (vec3Color.Length != positionsLength)
                        {
                            throw new Exception("different length");
                        }
                        var colors = data.NativeArrayManager.CreateNativeArray<Color>(vec3Color.Length);

                        for (var index = 0; index < vec3Color.Length; index++)
                        {
                            var color = vec3Color[index];
                            colors[index] = new Color(color.x / 255.0f, color.y / 255.0f, color.z / 255.0f);
                        }
                        return colors;
                    }

                case 4:
                    {
                        var vec4Color = data.GetArrayFromAccessor<Byte4>(primitives.attributes.COLOR_0);
                        if (vec4Color.Length != positionsLength)
                        {
                            throw new Exception("different length");
                        }
                        var colors = data.NativeArrayManager.CreateNativeArray<Color>(vec4Color.Length);

                        for (var index = 0; index < vec4Color.Length; index++)
                        {
                            var color = vec4Color[index];
                            colors[index] = new Color(color.x / 255.0f, color.y / 255.0f, color.z / 255.0f, color.w / 255.0f);
                        }
                        return colors;
                    }

                default:
                    throw new NotImplementedException($"unknown color type {accessor.type}");
            }
        }

        /// unsigned short normalized => c = round(f * 65535.0)
        public static NativeArray<Color>? GetColorsUInt16(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasColor(primitives)) return null;

            var accessor = data.GLTF.accessors[primitives.attributes.COLOR_0];
            switch (accessor.TypeCount)
            {
                case 3:
                    {
                        var vec3Color = data.GetArrayFromAccessor<UShort3>(primitives.attributes.COLOR_0);
                        if (vec3Color.Length != positionsLength)
                        {
                            throw new Exception("different length");
                        }
                        var colors = data.NativeArrayManager.CreateNativeArray<Color>(vec3Color.Length);

                        for (var index = 0; index < vec3Color.Length; index++)
                        {
                            var color = vec3Color[index];
                            colors[index] = new Color(color.x / 65535.0f, color.y / 65535.0f, color.z / 65535.0f);
                        }
                        return colors;
                    }

                case 4:
                    {
                        var vec4Color = data.GetArrayFromAccessor<UShort4>(primitives.attributes.COLOR_0);
                        if (vec4Color.Length != positionsLength)
                        {
                            throw new Exception("different length");
                        }
                        var colors = data.NativeArrayManager.CreateNativeArray<Color>(vec4Color.Length);

                        for (var index = 0; index < vec4Color.Length; index++)
                        {
                            var color = vec4Color[index];
                            colors[index] = new Color(color.x / 65535.0f, color.y / 65535.0f, color.z / 65535.0f, color.w / 65535.0f);
                        }
                        return colors;
                    }

                default:
                    throw new NotImplementedException($"unknown color type {accessor.type}");
            }
        }

        public static NativeArray<Color>? GetColorsFloat(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasColor(primitives)) return null;

            var accessor = data.GLTF.accessors[primitives.attributes.COLOR_0];
            switch (accessor.TypeCount)
            {
                case 3:
                    {
                        var vec3Color = data.GetArrayFromAccessor<Vector3>(primitives.attributes.COLOR_0);
                        if (vec3Color.Length != positionsLength)
                        {
                            throw new Exception("different length");
                        }
                        var colors = data.NativeArrayManager.CreateNativeArray<Color>(vec3Color.Length);

                        for (var index = 0; index < vec3Color.Length; index++)
                        {
                            var color = vec3Color[index];
                            colors[index] = new Color(color.x, color.y, color.z);
                        }
                        return colors;
                    }

                case 4:
                    var result = data.GetArrayFromAccessor<Color>(primitives.attributes.COLOR_0);
                    if (result.Length != positionsLength)
                    {
                        throw new Exception("different length");
                    }
                    return result;

                default:
                    throw new NotImplementedException(
                        $"unknown color type {data.GLTF.accessors[primitives.attributes.COLOR_0].type}");
            }
        }

        public static JointsAccessor.Getter GetJoints(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            // skin
            if (!HasSkin(primitives)) return null;
            var (getter, length) = JointsAccessor.GetAccessor(data, primitives.attributes.JOINTS_0);
            if (length != positionsLength)
            {
                throw new Exception("different length");
            }

            return getter;

        }

        public static WeightsAccessor.Getter GetWeights(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            // skin
            if (!HasSkin(primitives)) return null;
            var (getter, length) = WeightsAccessor.GetAccessor(data, primitives.attributes.WEIGHTS_0);
            if (length != positionsLength)
            {
                throw new Exception("different length");
            }

            return getter;
        }
    }
}