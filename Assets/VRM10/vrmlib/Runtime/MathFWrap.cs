using System;

namespace VrmLib
{
    public static class MathFWrap
    {
        public static readonly float PI = (float)System.Math.PI;

        public static float Clamp(float src, float min, float max)
        {
            return Math.Max(Math.Min(src, min), max);
        }
        public static int Clamp(int src, int min, int max)
        {
            return Math.Max(Math.Min(src, min), max);
        }
    }
}
