using System;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;

namespace UniVRM10
{
    [Serializable]
    public class VRM10ObjectLookAt
    {
        public enum LookAtTargetTypes
        {
            /// <summary>
            /// Vrm10Instance に設定した Transform を見ます.
            /// </summary>
            SpecifiedTransform = 0,
            /// <summary>
            /// Vrm10RuntimeLookAt に設定した Yaw/Pitch 値に従います.
            /// </summary>
            YawPitchValue = 1,

            [Obsolete] CalcYawPitchToGaze = 0,
            [Obsolete] SetYawPitch = 1,
        }

        [SerializeField]
        public Vector3 OffsetFromHead = new Vector3(0, 0.06f, 0);

        [SerializeField]
        public LookAtType LookAtType;

        [SerializeField]
        public CurveMapper HorizontalOuter = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper HorizontalInner = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalDown = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalUp = new CurveMapper(90.0f, 10.0f);
    }
}
