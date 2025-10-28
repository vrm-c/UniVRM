using System;
using UnityEngine;


namespace UniVRM10
{
    [Serializable]
    public class CurveMapper
    {
        /// <summary>
        /// v0.128.3 VRM10ObjectLookAtEditor
        /// 
        /// DegreeINput 0-90
        /// </summary>
        public float CurveXRangeDegree;

        /// <summary>
        /// v0.128.3 VRM10ObjectLookAtEditor
        /// 
        /// EyeBoneDegree 0-90
        /// or
        /// ExpressionWeight 0-1.0
        /// </summary>
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