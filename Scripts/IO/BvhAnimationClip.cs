using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniHumanoid
{
    public static class BvhAnimation
    {
        class CurveSet
        {
            BvhNode Node;
            Func<float, float, float, Quaternion> EulerToRotation;
            public CurveSet(BvhNode node)
            {
                Node = node;
            }

            public ChannelCurve PositionX;
            public ChannelCurve PositionY;
            public ChannelCurve PositionZ;
            public Vector3 GetPosition(int i)
            {
                return new Vector3(
                    PositionX.Keys[i],
                    PositionY.Keys[i],
                    PositionZ.Keys[i]);
            }

            public ChannelCurve RotationX;
            public ChannelCurve RotationY;
            public ChannelCurve RotationZ;
            public Quaternion GetRotation(int i)
            {
                if (EulerToRotation == null)
                {
                    EulerToRotation = Node.GetEulerToRotation();
                }
                return EulerToRotation(
                    RotationX.Keys[i],
                    RotationY.Keys[i],
                    RotationZ.Keys[i]
                    );
            }

            static void AddCurve(Bvh bvh, AnimationClip clip, ChannelCurve ch, float scaling)
            {
                if (ch == null) return;
                var pathWithProp = default(Bvh.PathWithProperty);
                bvh.TryGetPathWithPropertyFromChannel(ch, out pathWithProp);
                var curve = new AnimationCurve();
                for (int i = 0; i < bvh.FrameCount; ++i)
                {
                    var time = (float)(i * bvh.FrameTime.TotalSeconds);
                    var value = ch.Keys[i] * scaling;
                    curve.AddKey(time, value);
                }
                clip.SetCurve(pathWithProp.Path, typeof(Transform), pathWithProp.Property, curve);
            }

            public void AddCurves(Bvh bvh, AnimationClip clip, float scaling)
            {
                AddCurve(bvh, clip, PositionX, -scaling);
                AddCurve(bvh, clip, PositionY, scaling);
                AddCurve(bvh, clip, PositionZ, scaling);

                var pathWithProp = default(Bvh.PathWithProperty);
                bvh.TryGetPathWithPropertyFromChannel(RotationX, out pathWithProp);

                // rotation
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                for (int i = 0; i < bvh.FrameCount; ++i)
                {
                    var time = (float)(i * bvh.FrameTime.TotalSeconds);
                    var q = GetRotation(i).ReverseX();
                    curveX.AddKey(time, q.x);
                    curveY.AddKey(time, q.y);
                    curveZ.AddKey(time, q.z);
                    curveW.AddKey(time, q.w);
                }
                clip.SetCurve(pathWithProp.Path, typeof(Transform), "localRotation.x", curveX);
                clip.SetCurve(pathWithProp.Path, typeof(Transform), "localRotation.y", curveY);
                clip.SetCurve(pathWithProp.Path, typeof(Transform), "localRotation.z", curveZ);
                clip.SetCurve(pathWithProp.Path, typeof(Transform), "localRotation.w", curveW);
            }
        }

        public static AnimationClip CreateAnimationClip(Bvh bvh, float scaling)
        {
            var clip = new AnimationClip();
            clip.legacy = true;

            var curveMap = new Dictionary<BvhNode, CurveSet>();

            int j = 0;
            foreach (var node in bvh.Root.Traverse())
            {
                var set = new CurveSet(node);
                curveMap[node] = set;

                for (int i = 0; i < node.Channels.Length; ++i, ++j)
                {
                    var curve = bvh.Channels[j];
                    switch (node.Channels[i])
                    {
                        case Channel.Xposition: set.PositionX = curve; break;
                        case Channel.Yposition: set.PositionY = curve; break;
                        case Channel.Zposition: set.PositionZ = curve; break;
                        case Channel.Xrotation: set.RotationX = curve; break;
                        case Channel.Yrotation: set.RotationY = curve; break;
                        case Channel.Zrotation: set.RotationZ = curve; break;
                        default: throw new Exception();
                    }
                }
            }

            foreach (var set in curveMap)
            {
                set.Value.AddCurves(bvh, clip, scaling);
            }

            clip.EnsureQuaternionContinuity();

            return clip;
        }

    }
}
