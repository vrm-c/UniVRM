using System.Numerics;

namespace UniVRM10
{
    public static class ArrayExtensions
    {
        public static Vector3 ToVector3(this float[] src, Vector3 defaultValue = default)
        {
            if (src.Length != 3) return defaultValue;

            var v = new Vector3();
            v.X = src[0];
            v.Y = src[1];
            v.Z = src[2];
            return v;
        }

        public static Quaternion ToQuaternion(this float[] src)
        {
            if (src.Length != 4) return Quaternion.Identity;

            var v = new Quaternion(src[0], src[1], src[2], src[3]);
            return v;
        }
    }
}
