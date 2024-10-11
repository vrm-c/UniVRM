using System;
using UnityEngine;


namespace UniVRM10
{
    [Serializable]
    public class CurveMapper
    {
        [Range(20.0f, 90.0f)]
        public float CurveXRangeDegree;

        [Range(0, 90.0f)]
        public float CurveYRangeDegree;

        public CurveMapper(float xRange, float yRange)
        {
            CurveXRangeDegree = xRange;
            CurveYRangeDegree = yRange;
        }

        public void OnValidate()
        {
            if (CurveXRangeDegree == 0)
            {
                CurveXRangeDegree = 90.0f;
            }
        }

        public float Map(float src)
        {
            // https://github.com/vrm-c/UniVRM/issues/2452
            var t = Mathf.Clamp01(src / Mathf.Max(0.001f, CurveXRangeDegree));
            return t * CurveYRangeDegree;
        }
    }
}