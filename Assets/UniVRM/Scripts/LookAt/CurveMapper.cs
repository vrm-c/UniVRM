using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public class CurveMapper
    {
        public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);

        [Range(20.0f, 90.0f)]
        public float CurveXRangeDegree;

        [Range(0, 90.0f)]
        public float CurveYRangeDegree;

        /*
        public CurveMapper() : this(90.0f, 10.0f)
        {
        }
        */

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

        public void Apply(glTF_VRM_DegreeMap degreeMap)
        {
            CurveXRangeDegree = degreeMap.xRange;
            CurveYRangeDegree = degreeMap.yRange;
            if (degreeMap.curve != null)
            {
                Curve = new AnimationCurve(ToKeys(degreeMap.curve).ToArray());
            }
            else
            {
                Curve = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);
            }
        }

        IEnumerable<Keyframe> ToKeys(float[] values)
        {
            for (int i = 0; i < values.Length; i += 4)
            {
                yield return new Keyframe(values[i], values[i + 1], values[i + 2], values[i + 3]);
            }
        }

        public float Map(float src)
        {
            if (src < 0)
            {
                src = 0;
            }
            else if (src > CurveXRangeDegree)
            {
                src = CurveXRangeDegree;
            }
            return Curve.Evaluate(src / CurveXRangeDegree) * CurveYRangeDegree;
        }
    }
}
