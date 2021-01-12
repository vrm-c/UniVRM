using System;
using System.Linq;
using System.Numerics;

namespace VrmLib.Bvh
{
    public static class BvhExtensions
    {
        public static Func<float, float, float, Quaternion> GetEulerToRotation(this BvhNode bvh)
        {
            var order = bvh.Channels.Where(x => x == Channel.Xrotation || x == Channel.Yrotation || x == Channel.Zrotation).ToArray();

            return (x, y, z) =>
            {
                var xRot = Quaternion.CreateFromYawPitchRoll(x, 0, 0);
                var yRot = Quaternion.CreateFromYawPitchRoll(0, y, 0);
                var zRot = Quaternion.CreateFromYawPitchRoll(0, 0, z);

                var r = Quaternion.Identity;
                foreach (var ch in order)
                {
                    switch (ch)
                    {
                        case Channel.Xrotation: r = r * xRot; break;
                        case Channel.Yrotation: r = r * yRot; break;
                        case Channel.Zrotation: r = r * zRot; break;
                        default: throw new BvhException("no rotation");
                    }
                }
                return r;
            };
        }
    }
}
