using System;
using VrmLib;

namespace UniVRM10
{
    public static class LookAtAdapter
    {
        public static LookAtRangeMap FromGltf(this UniGLTF.Extensions.VRMC_vrm.LookAtRangeMap map)
        {
            return new LookAtRangeMap
            {
                InputMaxValue = map.InputMaxValue.Value,
                OutputScaling = map.OutputScale.Value,
            };
        }

        public static LookAtType FromGltf(this UniGLTF.Extensions.VRMC_vrm.LookAtType src)
        {
            switch (src)
            {
                case UniGLTF.Extensions.VRMC_vrm.LookAtType.bone: return LookAtType.Bone;
                case UniGLTF.Extensions.VRMC_vrm.LookAtType.expression: return LookAtType.Expression;
            }

            throw new NotImplementedException();
        }

        public static LookAt FromGltf(this UniGLTF.Extensions.VRMC_vrm.LookAt src)
        {
            return new LookAt
            {
                OffsetFromHeadBone = src.OffsetFromHeadBone.ToVector3(),
                LookAtType = src.LookAtType.FromGltf(),
                HorizontalInner = src.LookAtHorizontalInner.FromGltf(),
                HorizontalOuter = src.LookAtHorizontalOuter.FromGltf(),
                VerticalUp = src.LookAtVerticalUp.FromGltf(),
                VerticalDown = src.LookAtVerticalDown.FromGltf(),
            };
        }

        public static UniGLTF.Extensions.VRMC_vrm.LookAtRangeMap ToGltf(this LookAtRangeMap map)
        {
            return new UniGLTF.Extensions.VRMC_vrm.LookAtRangeMap
            {
                InputMaxValue = map.InputMaxValue,
                OutputScale = map.OutputScaling,
            };
        }

        public static UniGLTF.Extensions.VRMC_vrm.LookAt ToGltf(this LookAt lookAt)
        {
            var dst = new UniGLTF.Extensions.VRMC_vrm.LookAt
            {
                LookAtType = (UniGLTF.Extensions.VRMC_vrm.LookAtType)lookAt.LookAtType,
                LookAtHorizontalInner = lookAt.HorizontalInner.ToGltf(),
                LookAtHorizontalOuter = lookAt.HorizontalOuter.ToGltf(),
                LookAtVerticalUp = lookAt.VerticalUp.ToGltf(),
                LookAtVerticalDown = lookAt.VerticalDown.ToGltf(),
                OffsetFromHeadBone = lookAt.OffsetFromHeadBone.ToFloat3(),
            };
            return dst;
        }
    }
}