using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VRM0TPose と異なる初期回転を持ったヒエラルキー
    /// 
    /// - XR_EXT_hand_tracking
    /// - XR_FB_body_tracking
    /// - vrm-animation 
    /// 
    /// から VRM0TPose 互換の NormalizedLocalRotation(VRM0互換)を取り出す。
    /// BoneInitialRotation が値を計算できる。
    /// </summary>
    public sealed class InitRotationPoseProvider : INormalizedPoseProvider, ITPoseProvider
    {
        Transform m_root;
        Transform m_hips;
        private readonly Dictionary<HumanBodyBones, BoneInitialRotation> m_bones = new Dictionary<HumanBodyBones, BoneInitialRotation>();
        private readonly Dictionary<HumanBodyBones, EuclideanTransform> m_worldMap = new Dictionary<HumanBodyBones, EuclideanTransform>();

        public Vector3 HipTPoseWorldPosition => throw new System.NotImplementedException();

        public InitRotationPoseProvider(Transform root, UniHumanoid.Humanoid humanoid)
        {
            m_root = root;
            foreach (var (t, bone) in humanoid.BoneMap)
            {
                m_bones.Add(bone, new BoneInitialRotation(t));
                m_worldMap.Add(bone, new EuclideanTransform(root.localToWorldMatrix * t.localToWorldMatrix));
            }
            m_hips = m_bones[HumanBodyBones.Hips].Transform;
        }

        Quaternion INormalizedPoseProvider.GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone)
        {
            if (m_bones.TryGetValue(bone, out var c))
            {
                return c.NormalizedLocalRotation;
            }
            else
            {
                // Debug.LogWarning($"${bone} not found");
                return Quaternion.identity;
            }
        }

        Vector3 INormalizedPoseProvider.GetRawHipsPosition()
        {
            if (m_hips.parent == m_root)
            {
                return m_hips.localPosition;
            }
            else
            {
                return m_root.worldToLocalMatrix.MultiplyPoint(m_hips.position);
            }
        }

        EuclideanTransform? ITPoseProvider.GetWorldTransform(HumanBodyBones bone)
        {
            if (m_worldMap.TryGetValue(bone, out var t))
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
