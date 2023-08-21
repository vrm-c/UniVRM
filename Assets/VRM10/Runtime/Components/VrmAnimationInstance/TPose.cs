using System;
using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public class TPose : IVrm10Animation
    {
        public class NoRotation : INormalizedPoseProvider
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

        public class Skeleton : ITPoseProvider
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

        public (INormalizedPoseProvider, ITPoseProvider) m_controlRig;

        public (INormalizedPoseProvider, ITPoseProvider) ControlRig => m_controlRig;

        // empty
        public IReadOnlyDictionary<ExpressionKey, Func<float>> ExpressionMap => new Dictionary<ExpressionKey, Func<float>>();

        public LookAtInput? LookAt => default;

        public TPose(Vector3 hips)
        {
            m_controlRig = (new NoRotation(hips), new Skeleton(hips));
        }

        public void Dispose()
        {
        }

        public void ShowBoxMan(bool enable)
        {
        }
    }
}