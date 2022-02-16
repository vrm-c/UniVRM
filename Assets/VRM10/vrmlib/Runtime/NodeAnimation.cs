using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VrmLib
{
    public enum AnimationPathType
    {
        Translation,
        Rotation,
        Scale,
        Weights,
    }

    public class CurveSampler
    {
        public BufferAccessor In;
        public BufferAccessor Out;

        public float LastTime
        {
            get
            {
                if (In.ComponentType == AccessorValueType.FLOAT)
                {
                    var times =  In.Bytes.Reinterpret<Single>(1);
                    return times[times.Length - 1];
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public CurveSampler()
        {
        }

        ValueTuple<int, int, float> GetRange(float seconds)
        {
            var keys = In.GetSpan<float>();

            if (seconds <= keys[0])
            {
                return (0, 0, 0);
            }
            else if (seconds >= keys[keys.Length - 1])
            {
                return (keys.Length - 1, keys.Length - 1, 0);
            }

            // search range
            float begin = keys[0];
            float end;
            for (int i = 1; i < keys.Length; ++i)
            {
                end = keys[i];

                if (seconds == end)
                {
                    return (i, i, 0);
                }
                else if (seconds < end)
                {
                    var ratio = (seconds - begin) / (end - begin);
                    return (i - 1, i, ratio);
                }

                begin = end;
            }

            throw new Exception("not found");
        }

        public Vector3 GetVector3(TimeSpan elapsed)
        {
            var (begin, end, ratio) = GetRange((float)elapsed.TotalSeconds);
            var values = Out.GetSpan<Vector3>();
            if (begin == end)
            {
                return values[begin];
            }
            else
            {
                // TODO: curve interpolation
                return Vector3.Lerp(values[begin], values[end], ratio);
            }
        }

        public Quaternion GetQuaternion(TimeSpan elapsed)
        {
            var (begin, end, ratio) = GetRange((float)elapsed.TotalSeconds);
            var values = Out.GetSpan<Quaternion>();
            if (begin == end)
            {
                return values[begin];
            }
            else
            {
                // TODO: curve interpolation
                return Quaternion.Lerp(values[begin], values[end], ratio);
            }
        }

        public void SkipFrame(int skipFrames)
        {
            In = In.Skip(skipFrames);
            Out = Out.Skip(skipFrames);
        }
    }

    public class NodeAnimation
    {
        public TimeSpan Duration
        {
            get
            {
                var lastTime = float.NegativeInfinity;
                foreach (var kv in Curves)
                {
                    if (kv.Value.LastTime > lastTime)
                    {
                        lastTime = kv.Value.LastTime;
                    }
                }
                return TimeSpan.FromSeconds(lastTime);
            }
        }

        public Dictionary<AnimationPathType, CurveSampler> Curves = new Dictionary<AnimationPathType, CurveSampler>();
    }
}
