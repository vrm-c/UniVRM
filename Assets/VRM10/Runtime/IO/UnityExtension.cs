using UnityEngine;

namespace UniVRM10
{
    public static class UnityExtension
    {
        public static float[] ToFloat3(this Vector3 value)
        {
            return new[] { value.x, value.y, value.z };
        }

        public static float[] ToFloat4(this Quaternion value)
        {
            return new float[] { value.x, value.y, value.z, value.w };
        }
    }
}
