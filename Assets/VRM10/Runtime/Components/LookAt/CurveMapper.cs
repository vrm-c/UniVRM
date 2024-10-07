using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniVRM10
{
    [Serializable]
    public class CurveMapper
    {
        private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);

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
                return 0;
                // or CurveYRangeDegree ?
            }

            if (src < 0)
            {
                src = 0;
            }
            else if (src > CurveXRangeDegree)
            {
                src = CurveXRangeDegree;
            }
            return _curve.Evaluate(src / CurveXRangeDegree) * CurveYRangeDegree;
        }
    }
}
