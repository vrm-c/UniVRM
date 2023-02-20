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
    public class InitRotationRigGetter : IControlRigGetter
    {
        Transform m_root;
        private readonly Dictionary<HumanBodyBones, BoneInitialRotation> m_bones = new Dictionary<HumanBodyBones, BoneInitialRotation>();

        /// <param name="tpose">TPoseのヒエラルキー</param>
        public InitRotationRigGetter(Transform root, UniHumanoid.Humanoid humanoid)
        {
            m_root = root;
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

        public Vector3 GetRootPosition()
        {
            // TODO: from m_root relative ? scaling ?
            return m_bones[HumanBodyBones.Hips].Transform.localPosition;
        }
    }
}
