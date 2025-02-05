using System;
using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Animator から Pose 供給する。
    /// この Animator は以下の条件を満たすこと。
    /// 
    /// * TPoseであること
    /// * 正規化済みであること
    /// 
    /// </summary>
    public sealed class AnimatorPoseProvider : INormalizedPoseProvider, ITPoseProvider
    {
        Transform m_root;
        Animator m_animator;
        Dictionary<HumanBodyBones, EuclideanTransform> m_posMap = new Dictionary<HumanBodyBones, EuclideanTransform>();

        public AnimatorPoseProvider(Transform root, Animator animator)
        {
            m_root = root;
            m_animator = animator;

            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);

            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }
                var t = animator.GetBoneTransform(bone);
                if (t != null)
                {
                    m_posMap[bone] = new EuclideanTransform(root.worldToLocalMatrix * t.localToWorldMatrix);
                }
            }
        }

        Quaternion INormalizedPoseProvider.GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone)
        {
            if (m_animator.GetBoneTransform(bone) is Transform t)
            {
                // TODO: parentBone relative
                return t.localRotation;
            }
            else
            {
                return Quaternion.identity;
            }
        }

        Vector3 INormalizedPoseProvider.GetRawHipsPosition()
        {
            // TODO: from model root ?
            return m_animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        }

        EuclideanTransform? ITPoseProvider.GetWorldTransform(HumanBodyBones bone)
        {
            if (m_posMap.TryGetValue(bone, out var t))
            {
                return t;
            }
            else
            {
                return default;
            }
        }
    }
}
