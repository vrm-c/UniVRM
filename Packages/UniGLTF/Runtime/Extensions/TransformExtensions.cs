using UnityEngine;

namespace UniGLTF
{
    public static class TransformExtensions
    {
        public static float UniformedLossyScale(this Transform transform)
        {
            var s = transform.lossyScale;
            return AbsoluteMaxValue(s);
        }

        public static float AbsoluteMaxValue(in Vector3 s)
        {
            var x = Mathf.Abs(s.x);
            var y = Mathf.Abs(s.y);
            var z = Mathf.Abs(s.z);
            return Mathf.Max(Mathf.Max(x, y), z);
        }
    }
}