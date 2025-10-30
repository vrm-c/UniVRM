using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRM
{
    public static class VRMBoneExtensions
    {
        public static VRMBone FromHumanBodyBone(this HumanBodyBones human)
        {
            return human.ToVrmBone();
        }

        public static HumanBodyBones ToHumanBodyBone(this VRMBone bone)
        {
#if UNITY_5_6_OR_NEWER
#else
            if (bone == VRMBone.upperChest)
            {
                return HumanBodyBones.LastBone;
            }
#endif
            return bone.ToUnityBone();

        }
    }

    public static class VRMHumanoidExtensions
    {
        public static void SetNodeIndex(this glTF_VRM_Humanoid self, HumanBodyBones _key, int node)
        {
            var key = _key.FromHumanBodyBone();
            var index = self.humanBones.FindIndex(x => x.vrmBone == key);
            if (index == -1 || self.humanBones[index] == null)
            {
                // not found
                self.humanBones.Add(new glTF_VRM_HumanoidBone
                {
                    vrmBone = key,
                    node = node
                });
            }
            else
            {
                self.humanBones[index].node = node;
            }
        }

        public static void Apply(this glTF_VRM_Humanoid self, UniHumanoid.AvatarDescription desc, List<Transform> nodes)
        {
            self.armStretch = desc.armStretch;
            self.legStretch = desc.legStretch;
            self.upperArmTwist = desc.upperArmTwist;
            self.lowerArmTwist = desc.lowerArmTwist;
            self.upperLegTwist = desc.upperLegTwist;
            self.lowerLegTwist = desc.lowerArmTwist;
            self.feetSpacing = desc.feetSpacing;
            self.hasTranslationDoF = desc.hasTranslationDoF;

            foreach (var x in desc.human)
            {
                var nodeIndex = nodes.FindIndex(y => y.name == x.boneName);
                if (nodeIndex < 0)
                {
                    continue;
                }
                var key = x.humanBone.FromHumanBodyBone();
                var found = self.humanBones.FirstOrDefault(y => y.vrmBone == key);
                if (found == null)
                {
                    found = new glTF_VRM_HumanoidBone
                    {
                        vrmBone = key
                    };
                    self.humanBones.Add(found);
                }

                found.node = nodeIndex;
                found.useDefaultValues = x.useDefaultValues;
                found.axisLength = x.axisLength;
                found.center = x.center;
                found.max = x.max;
                found.min = x.min;
            }
        }

        public static UniHumanoid.AvatarDescription ToDescription(this glTF_VRM_Humanoid self, List<Transform> nodes)
        {
            var description = ScriptableObject.CreateInstance<UniHumanoid.AvatarDescription>();
            description.upperLegTwist = self.upperLegTwist;
            description.lowerLegTwist = self.lowerLegTwist;
            description.upperArmTwist = self.upperArmTwist;
            description.lowerArmTwist = self.lowerArmTwist;
            description.armStretch = self.armStretch;
            description.legStretch = self.legStretch;
            description.hasTranslationDoF = self.hasTranslationDoF;

            var boneLimits = new UniHumanoid.BoneLimit[self.humanBones.Count];
            int index = 0;
            foreach (var x in self.humanBones)
            {
                boneLimits[index] = new UniHumanoid.BoneLimit
                {
                    useDefaultValues = x.useDefaultValues,
                    axisLength = x.axisLength,
                    center = x.center,
                    min = x.min,
                    max = x.max,
                    humanBone = x.vrmBone.ToHumanBodyBone(),
                };
                if (x.node >= 0 && x.node < nodes.Count)
                {
                    boneLimits[index].boneName = nodes[x.node].name;
                }
                index++;
            }

            description.human = boneLimits;

            return description;
        }
    }
}
