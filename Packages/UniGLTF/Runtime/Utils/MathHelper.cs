using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace UniGLTF.Runtime.Utils
{
    public static class MathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 MultiplyPoint3x4(float4x4 matrix, float3 point)
        {
            return math.mul(matrix, new float4(point, 1)).xyz;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 MultiplyPoint(float4x4 matrix, float3 point)
        {
            var v = math.mul(matrix, new float4(point, 1.0f));
            return v.xyz / v.w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 MultiplyVector(float4x4 matrix, float3 vector)
        {
            return math.mul(matrix, new float4(vector, 0)).xyz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion FromToRotation(in float3 fromVector, in float3 toVector)
        {
            if (math.lengthsq(fromVector) == 0 || math.lengthsq(toVector) == 0)
            {
                return quaternion.identity;
            }

            float3 from = math.normalize(fromVector);
            float3 to = math.normalize(toVector);

            var dot = math.dot(from, to);
            switch(dot)
            {
                case >= 1.0f:
                    return quaternion.identity; 
                case <= -1.0f:
                {
                    var axis = math.cross(from, new float3(1, 0, 0));
                    if (math.lengthsq(axis) < 0.0001f)
                    {
                        axis = math.cross(from, new float3(0, 1, 0));
                    }
                    return quaternion.AxisAngle(math.normalize(axis), math.PI);
                }
                default:
                {
                    var angle = math.acos(dot);
                    var axis = math.cross(from, to);
                    return quaternion.AxisAngle(math.normalize(axis), angle);
                }
            }
        }

        /// <summary>
        /// <see cref="UnityEngine.Mathf.Approximately"/> を Unity.Mathematics パッケージ向けに再実装した関数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Approximately(float a, float b)
        {
            return math.abs(b - a) < math.max(1E-06f * math.max(math.abs(a), math.abs(b)), math.EPSILON * 8f);
        }

        /// <inheritdoc cref="MathHelper.Approximately(float, float)"/>
        public static bool Approximately(float3 a, float3 b)
        {
            return Approximately(a.x, b.x) &&
                Approximately(a.y, b.y) &&
                Approximately(a.z, b.z);
        }

        /// <inheritdoc cref="MathHelper.Approximately(float, float)"/>
        public static bool Approximately(quaternion a, quaternion b)
        {
            return Approximately(a.value.x, b.value.x) &&
                Approximately(a.value.y, b.value.y) &&
                Approximately(a.value.z, b.value.z) &&
                Approximately(a.value.w, b.value.w);
        }
    }
}
