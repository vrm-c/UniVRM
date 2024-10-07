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

        // zero 除算などを回避する閾値
        public const float MIMIMUM_INPUT_MAX_VALUE = 1e-5f;

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
            if (CurveXRangeDegree < MIMIMUM_INPUT_MAX_VALUE)
            {
                // https://github.com/vrm-c/UniVRM/issues/2452
                return src <= 0 ? 0 : CurveXRangeDegree;
            }
            else
            {
                if (src < 0)
                {
                    src = 0;
                }
                else if (src > CurveXRangeDegree)
                {
                    src = CurveXRangeDegree;
                }
                return src / CurveXRangeDegree * CurveYRangeDegree;
            }
        }
    }
}