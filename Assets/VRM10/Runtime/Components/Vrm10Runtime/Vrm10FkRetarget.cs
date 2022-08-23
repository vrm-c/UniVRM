using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public class Vrm10FkRetarget
    {
        Vrm10BoneWithAxis m_root;

        Dictionary<HumanBodyBones, Vrm10BoneWithAxis> m_boneMap = new Dictionary<HumanBodyBones, Vrm10BoneWithAxis>();

        public readonly float InitialHipsHeight;

        public Vrm10FkRetarget(UniHumanoid.Humanoid humanoid)
        {
            m_root = Vrm10BoneWithAxis.Build(humanoid, m_boneMap);
            InitialHipsHeight = m_root.Target.position.y;
            Debug.Log($"InitialHipsHeight: {InitialHipsHeight}");
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
