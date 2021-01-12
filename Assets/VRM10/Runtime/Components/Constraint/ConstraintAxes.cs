using System;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// FreezeAxesで使う。bitマスク
    /// </summary>
    [Flags]
    public enum AxesMask
    {
        X = 1,
        Y = 2,
        Z = 4,
    }

    public static class AxesMaskExtensions
    {
        public static Vector3 Freeze(this AxesMask mask, Vector3 src)
        {
            if (mask.HasFlag(AxesMask.X))
            {
                src.x = 0;
            }
            if (mask.HasFlag(AxesMask.Y))
            {
                src.y = 0;
            }
            if (mask.HasFlag(AxesMask.Z))
            {
                src.z = 0;
            }
            return src;
        }
    }
}
