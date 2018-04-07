using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using UniHumanoid;


namespace VRM
{
    public enum VRMBone
    {
        hips,
        leftUpperLeg,
        rightUpperLeg,
        leftLowerLeg,
        rightLowerLeg,
        leftFoot,
        rightFoot,
        spine,
        chest,
        neck,
        head,
        leftShoulder,
        rightShoulder,
        leftUpperArm,
        rightUpperArm,
        leftLowerArm,
        rightLowerArm,
        leftHand,
        rightHand,
        leftToes,
        rightToes,
        leftEye,
        rightEye,
        jaw,
        leftThumbProximal,
        leftThumbIntermediate,
        leftThumbDistal,
        leftIndexProximal,
        leftIndexIntermediate,
        leftIndexDistal,
        leftMiddleProximal,
        leftMiddleIntermediate,
        leftMiddleDistal,
        leftRingProximal,
        leftRingIntermediate,
        leftRingDistal,
        leftLittleProximal,
        leftLittleIntermediate,
        leftLittleDistal,
        rightThumbProximal,
        rightThumbIntermediate,
        rightThumbDistal,
        rightIndexProximal,
        rightIndexIntermediate,
        rightIndexDistal,
        rightMiddleProximal,
        rightMiddleIntermediate,
        rightMiddleDistal,
        rightRingProximal,
        rightRingIntermediate,
        rightRingDistal,
        rightLittleProximal,
        rightLittleIntermediate,
        rightLittleDistal,
        upperChest,
    }

    public static class VRMBoneExtensions
    {
        public static VRMBone FromHumanBodyBone(this HumanBodyBones human)
        {
            return EnumUtil.TryParseOrDefault<VRMBone>(human.ToString());
        }
        public static HumanBodyBones ToHumanBodyBone(this VRMBone bone)
        {
            return EnumUtil.TryParseOrDefault<HumanBodyBones>(bone.ToString());
        }
    }

    [Serializable]
    public class glTF_VRM_HumanoidBone : JsonSerializableBase
    {
        public string bone;
        public VRMBone vrmBone
        {
            set
            {
                bone = value.ToString();
            }
            get
            {
                return EnumUtil.TryParseOrDefault<VRMBone>(bone);
            }
        }
        public int node = -1;

        public bool useDefaultValues = true;
        public Vector3 min;
        public Vector3 max;
        public Vector3 center;
        public float axisLength;

        protected override void SerializeMembers(JsonFormatter f)
        {
            f.Key("bone"); f.Value((string)bone.ToString());
            f.KeyValue(() => node);
            f.KeyValue(() => useDefaultValues);
            if (!useDefaultValues)
            {
                f.KeyValue(() => min);
                f.KeyValue(() => max);
                f.KeyValue(() => center);
                f.KeyValue(() => axisLength);
            }
        }
    }

    [Serializable]
    public class glTF_VRM_Humanoid : JsonSerializableBase
    {
        public List<glTF_VRM_HumanoidBone> humanBones = new List<glTF_VRM_HumanoidBone>();
        public float armStretch = 0.05f;
        public float legStretch = 0.05f;
        public float upperArmTwist = 0.5f;
        public float lowerArmTwist = 0.5f;
        public float upperLegTwist = 0.5f;
        public float lowerLegTwist = 0.5f;
        public float feetSpacing = 0;
        public bool hasTranslationDoF = false;

        public void SetNodeIndex(HumanBodyBones _key, int node)
        {
            var key = _key.FromHumanBodyBone();
            var index = humanBones.FindIndex(x => x.vrmBone == key);
            if (index == -1 || humanBones[index] == null)
            {
                // not found
                humanBones.Add(new glTF_VRM_HumanoidBone
                {
                    vrmBone = key,
                    node = node
                });
            }
            else
            {
                humanBones[index].node = node;
            }
        }

        protected override void SerializeMembers(JsonFormatter f)
        {
            f.KeyValue(() => humanBones);
            f.KeyValue(() => armStretch);
            f.KeyValue(() => legStretch);
            f.KeyValue(() => upperArmTwist);
            f.KeyValue(() => lowerArmTwist);
            f.KeyValue(() => upperLegTwist);
            f.KeyValue(() => lowerLegTwist);
            f.KeyValue(() => feetSpacing);
            f.KeyValue(() => hasTranslationDoF);
        }

        public void Apply(UniHumanoid.AvatarDescription desc, List<Transform> nodes)
        {
            armStretch = desc.armStretch;
            legStretch = desc.legStretch;
            upperArmTwist = desc.upperArmTwist;
            lowerArmTwist = desc.lowerArmTwist;
            upperLegTwist = desc.upperLegTwist;
            lowerLegTwist = desc.lowerArmTwist;
            feetSpacing = desc.feetSpacing;
            hasTranslationDoF = desc.hasTranslationDoF;

            foreach (var x in desc.human)
            {
                var key = x.humanBone.FromHumanBodyBone();
                var found = humanBones.FirstOrDefault(y => y.vrmBone == key);
                if (found == null)
                {
                    found = new glTF_VRM_HumanoidBone
                    {
                        vrmBone = key
                    };
                    humanBones.Add(found);
                }
                found.node = nodes.FindIndex(y => y.name == x.boneName);

                found.useDefaultValues = x.useDefaultValues;
                found.axisLength = x.axisLength;
                found.center = x.center;
                found.max = x.max;
                found.min = x.min;
            }
        }

        public UniHumanoid.AvatarDescription ToDescription(List<Transform> nodes)
        {
            var description = ScriptableObject.CreateInstance<UniHumanoid.AvatarDescription>();
            description.upperLegTwist = upperLegTwist;
            description.lowerLegTwist = lowerLegTwist;
            description.upperArmTwist = upperArmTwist;
            description.lowerArmTwist = lowerArmTwist;
            description.armStretch = armStretch;
            description.legStretch = legStretch;
            description.hasTranslationDoF = hasTranslationDoF;
            description.human = humanBones.Select(x => new UniHumanoid.BoneLimit
            {
                boneName = nodes[x.node].name,
                useDefaultValues = x.useDefaultValues,
                axisLength = x.axisLength,
                center = x.center,
                min = x.min,
                max = x.max,
                humanBone = x.vrmBone.ToHumanBodyBone(),               
            }).ToArray();
            return description;
        }
    }
}
