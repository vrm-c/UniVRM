using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public class CurveMapper : IEquatable<CurveMapper>
    {
        public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);

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

        public void Assign(CurveMapper mapper)
        {
            CurveXRangeDegree = mapper.CurveXRangeDegree;
            CurveYRangeDegree = mapper.CurveYRangeDegree;
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
            // https://github.com/vrm-c/UniVRM/issues/2452
            var t = Mathf.Clamp01(src / MathF.Max(CurveXRangeDegree, 0.001f));
            return Curve.Evaluate(t) * CurveYRangeDegree;
        }

        public bool Equals(CurveMapper other)
        {
            if (CurveXRangeDegree != other.CurveXRangeDegree) return false;
            if (CurveYRangeDegree != other.CurveYRangeDegree) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is CurveMapper mapper)
            {
                return Equals(mapper);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
