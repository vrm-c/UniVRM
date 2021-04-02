using System;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// FreezeAxesで使う。bitマスク
    /// </summary>
    [Flags]
    public enum AxisMask
    {
        X = 1,
        Y = 2,
        Z = 4,
    }

    public static class AxesMaskExtensions
    {
        public static Vector3 Freeze(this AxisMask mask, Vector3 src)
        {
            if (mask.HasFlag(AxisMask.X))
            {
                src.x = 0;
            }
            if (mask.HasFlag(AxisMask.Y))
            {
                src.y = 0;
            }
            if (mask.HasFlag(AxisMask.Z))
            {
                src.z = 0;
            }
            return src;
        }
    }
}
