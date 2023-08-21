using System;
using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public class Vrm10TPose : IVrm10Animation
    {
        class NoRotation : INormalizedPoseProvider
        {
            readonly Vector3 m_hips;

            public NoRotation(Vector3 hips)
            {
                m_hips = hips;
            }

            public Quaternion GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone)
            {
                return new Quaternion(0, 0, 0, 1);
            }

            public Vector3 GetRawHipsPosition()
            {
                return m_hips;
            }
        }

        class Skeleton : ITPoseProvider
        {
            Vector3 m_hips;
            public Skeleton(Vector3 hips)
            {
                m_hips = hips;
            }

            public EuclideanTransform? GetWorldTransform(HumanBodyBones bone)
            {
                if (bone == HumanBodyBones.Hips)
                {
                    return new EuclideanTransform(m_hips);
                }
                return default;
            }
        }

        public (INormalizedPoseProvider, ITPoseProvider) ControlRig { get; }

        public IReadOnlyDictionary<ExpressionKey, Func<float>> ExpressionMap { get; }

        public LookAtInput? LookAt { get; }

        public Vrm10TPose(Vector3 hips)
        {
            ControlRig = (new NoRotation(hips), new Skeleton(hips));
            ExpressionMap = new Dictionary<ExpressionKey, Func<float>>();
            LookAt = default;
        }

        public void Dispose()
        {
        }

        public void ShowBoxMan(bool enable)
        {
        }
    }
}