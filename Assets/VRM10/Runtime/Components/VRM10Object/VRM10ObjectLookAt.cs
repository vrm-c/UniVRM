using System;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [Serializable]
    public class VRM10ObjectLookAt
    {
        public enum LookAtTargetTypes
        {
            CalcYawPitchToGaze,
            SetYawPitch,
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
