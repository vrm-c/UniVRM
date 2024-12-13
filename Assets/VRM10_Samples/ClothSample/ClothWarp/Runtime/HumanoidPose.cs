using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10.ClothWarp
{
    public class HumanoidPose
    {
        public Vector3 HipsPosition;
        public List<(HumanBodyBones, Quaternion)> Rotations = new();

        public HumanoidPose(Animator aninmator)
        {
            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }
                var t = aninmator.GetBoneTransform(bone);
                if (t != null)
                {
                    if (bone == HumanBodyBones.Hips)
                    {
                        HipsPosition = t.localPosition;
                    }
                    Rotations.Add((bone, t.localRotation));
                }
            }
        }

        public static void ApplyLerp(Animator a, HumanoidPose start, HumanoidPose end, float t)
        {
            foreach (var (b, sr, er) in Enumerable.Zip(start.Rotations, end.Rotations, (s, e) =>
            {
                var (sb, sr) = s;
                var (eb, er) = e;
                if (sb != eb)
                {
                    throw new Exception();
                }
                return (sb, sr, er);
            }))
            {
                var transform = a.GetBoneTransform(b);
                if (transform == null)
                {
                    throw new Exception();
                }
                transform.localRotation = Quaternion.Slerp(sr, er, t);
                if (b == HumanBodyBones.Hips)
                {
                    transform.localPosition = Vector3.Lerp(start.HipsPosition, end.HipsPosition, t);
                }
            }
        }
    }
}