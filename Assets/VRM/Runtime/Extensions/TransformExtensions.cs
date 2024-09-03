using UnityEngine;

namespace VRM
{
    public static class TransformExtensions
    {
        public static float UniformedLossyScale(this Transform transform)
        {
            // Mathf.Max(a, b, c) は GC どうなんだろう
            var s = transform.lossyScale;
            var x = Mathf.Abs(s.x);
            var y = Mathf.Abs(s.y);
            var z = Mathf.Abs(s.z);
            if (x < y)
            {
                if (y < z)
                {
                    return z;
                }
                else
                {
                    return y;
                }
            }
            else
            {
                if (x < z)
                {
                    return z;
                }
                else
                {
                    return x;
                }
            }
        }
    }
}