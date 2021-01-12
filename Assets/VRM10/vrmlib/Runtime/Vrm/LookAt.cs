

using System.Numerics;

namespace VrmLib
{
    public enum LookAtType
    {
        Bone,
        Expression,
    }

    public class LookAtRangeMap
    {
        /// <summary>
        /// Yaw, Pitch 各を 0 ~ InputMaxValue で clamp する
        /// </summary>
        public float InputMaxValue = 90.0f;

        /// <summary>
        /// 0 ~ InputMaxValue を 0 ~ 1 に map してから乗算する
        /// </summary>
        public float OutputScaling = 10.0f;

        /// 4つでひとつのキー。最低8
        public float[] Curve;
    }

    public class LookAt
    {
        public Vector3 OffsetFromHeadBone;

        public LookAtType LookAtType;

        public LookAtRangeMap HorizontalInner = new LookAtRangeMap();
        public LookAtRangeMap HorizontalOuter = new LookAtRangeMap();
        public LookAtRangeMap VerticalUp = new LookAtRangeMap();
        public LookAtRangeMap VerticalDown = new LookAtRangeMap();

    }
}