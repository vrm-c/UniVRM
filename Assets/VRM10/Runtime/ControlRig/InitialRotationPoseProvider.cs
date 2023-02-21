using System.Collections.Generic;
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

        public Vector3 HipTPoseWorldPosition => throw new System.NotImplementedException();

        public InitRotationPoseProvider(Transform root, UniHumanoid.Humanoid humanoid)
        {
            m_root = root;
            m_hips = m_bones[HumanBodyBones.Hips].Transform;
            foreach (var (t, bone) in humanoid.BoneMap)
            {
                m_bones.Add(bone, new BoneInitialRotation(t));
            }
        }

        public Quaternion GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone)
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

        public Vector3 GetRawHipsPosition()
        {
            if (m_hips.parent == m_root)
            {
                return m_hips.localPosition;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public Quaternion GetBoneWorldRotation(HumanBodyBones bone)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> EnumerateBoneParentPairs()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetBoneWorldPosition(HumanBodyBones bone)
        {
            throw new System.NotImplementedException();
        }
    }
}
