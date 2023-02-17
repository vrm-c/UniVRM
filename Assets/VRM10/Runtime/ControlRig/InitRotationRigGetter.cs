using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 初期回転を持っているヒエラルキーから、NormalizedLocalRotation(VRM0互換)を取り出す
    /// </summary>
    public class InitRotationGetter : IControlRigGetter
    {
        Transform m_root;
        private readonly Dictionary<HumanBodyBones, Vrm10ControlBone> m_bones = new Dictionary<HumanBodyBones, Vrm10ControlBone>();

        /// <param name="tpose">TPoseのヒエラルキー</param>
        public InitRotationGetter(Transform root, UniHumanoid.Humanoid humanoid)
        {
            m_root = root;
            foreach (var (t, bone) in humanoid.BoneMap)
            {
                m_bones.Add(bone, new Vrm10ControlBone(t));
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
