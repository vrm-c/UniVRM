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
            return data.GetArrayFromAccessor<Vector3>(primitives.attributes.POSITION);
        }

        public static NativeArray<Vector3>? GetNormals(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasNormal(primitives)) return null;
            var result = data.GetArrayFromAccessor<Vector3>(primitives.attributes.NORMAL);
            if (result.Length != positionsLength)
            {
                throw new Exception("different length");
            }

            return result;
        }

        public static NativeArray<Vector2>? GetTexCoords0(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasTexCoord0(primitives)) return null;
            var result = data.GetArrayFromAccessor<Vector2>(primitives.attributes.TEXCOORD_0);
            if (result.Length != positionsLength)
            {
                throw new Exception("different length");
            }

            return result;
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

        public static NativeArray<Color>? GetColors(this glTFPrimitives primitives, GltfData data, int positionsLength)
        {
            if (!HasColor(primitives)) return null;

            switch (data.GLTF.accessors[primitives.attributes.COLOR_0].TypeCount)
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