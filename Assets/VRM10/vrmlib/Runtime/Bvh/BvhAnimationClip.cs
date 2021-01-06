using System;
using System.Collections.Generic;
using System.Numerics;

namespace VrmLib.Bvh
{
    public static class BvhAnimation
    {
        class CurveSet
        {
            BvhNode Node = default;
            Func<float, float, float, Quaternion> EulerToRotation = default;
            public CurveSet(BvhNode node)
            {
                Node = node;
            }

            public ChannelCurve PositionX = default;
            public ChannelCurve PositionY = default;
            public ChannelCurve PositionZ = default;
            public Vector3 GetPosition(int i)
            {
                return new Vector3(
                    PositionX.Keys[i],
                    PositionY.Keys[i],
                    PositionZ.Keys[i]);
            }

            public ChannelCurve RotationX = default;
            public ChannelCurve RotationY = default;
            public ChannelCurve RotationZ = default;
            public Quaternion GetRotation(int i)
            {
                if (EulerToRotation == null)
                {
                    EulerToRotation = Node.GetEulerToRotation();
                }
                return EulerToRotation(
                    RotationX.Keys[i],
                    RotationY.Keys[i],
                    RotationZ.Keys[i]
                    );
            }
        }
    }
}
