using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public class Vrm10FkRetarget
    {
        Vrm10BoneWithAxis m_root;

        Dictionary<HumanBodyBones, Vrm10BoneWithAxis> m_boneMap = new Dictionary<HumanBodyBones, Vrm10BoneWithAxis>();

        Vector3 m_initialHipsPosition;

        public Vrm10FkRetarget(UniHumanoid.Humanoid humanoid)
        {
            m_root = Vrm10BoneWithAxis.Build(humanoid, m_boneMap);
            m_initialHipsPosition = m_root.Target.position;
        }

        public void Apply()
        {
            m_root.Target.position = m_root.Normalized.position;
            m_root.ApplyRecursive(Quaternion.identity);
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            if (m_boneMap.TryGetValue(bone, out var value))
            {
                return value.Normalized;
            }
            else
            {
                return null;
            }
        }
    }
}
