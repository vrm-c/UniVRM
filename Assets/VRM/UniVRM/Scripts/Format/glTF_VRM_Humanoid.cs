using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;


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

        unknown,
    }

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

    [Serializable]
    [JsonSchema(Title = "vrm.humanoid.bone")]
    public class glTF_VRM_HumanoidBone : JsonSerializableBase
    {
        [JsonSchema(Description = "Human bone name.", EnumValues = new object[]
        {
            "hips",
            "leftUpperLeg",
            "rightUpperLeg",
            "leftLowerLeg",
            "rightLowerLeg",
            "leftFoot",
            "rightFoot",
            "spine",
            "chest",
            "neck",
            "head",
            "leftShoulder",
            "rightShoulder",
            "leftUpperArm",
            "rightUpperArm",
            "leftLowerArm",
            "rightLowerArm",
            "leftHand",
            "rightHand",
            "leftToes",
            "rightToes",
            "leftEye",
            "rightEye",
            "jaw",
            "leftThumbProximal",
            "leftThumbIntermediate",
            "leftThumbDistal",
            "leftIndexProximal",
            "leftIndexIntermediate",
            "leftIndexDistal",
            "leftMiddleProximal",
            "leftMiddleIntermediate",
            "leftMiddleDistal",
            "leftRingProximal",
            "leftRingIntermediate",
            "leftRingDistal",
            "leftLittleProximal",
            "leftLittleIntermediate",
            "leftLittleDistal",
            "rightThumbProximal",
            "rightThumbIntermediate",
            "rightThumbDistal",
            "rightIndexProximal",
            "rightIndexIntermediate",
            "rightIndexDistal",
            "rightMiddleProximal",
            "rightMiddleIntermediate",
            "rightMiddleDistal",
            "rightRingProximal",
            "rightRingIntermediate",
            "rightRingDistal",
            "rightLittleProximal",
            "rightLittleIntermediate",
            "rightLittleDistal",
            "upperChest",
        }, EnumSerializationType = EnumSerializationType.AsString)]
        public string bone;

        public VRMBone vrmBone
        {
            set
            {
                bone = value.ToString();
            }
            get
            {
                return CacheEnum.Parse<VRMBone>(bone, true);
            }
        }

        // When the value is -1, it means that no node is found.
        [JsonSchema(Description = "Reference node index", Minimum = 0)]
        public int node = -1;

        [JsonSchema(Description = "Unity's HumanLimit.useDefaultValues")]
        public bool useDefaultValues = true;

        [JsonSchema(Description = "Unity's HumanLimit.min")]
        public Vector3 min;

        [JsonSchema(Description = "Unity's HumanLimit.max")]
        public Vector3 max;

        [JsonSchema(Description = "Unity's HumanLimit.center")]
        public Vector3 center;

        [JsonSchema(Description = "Unity's HumanLimit.axisLength")]
        public float axisLength;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.Key("bone"); f.Value((string) bone.ToString());
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
    [JsonSchema(Title = "vrm.humanoid")]
    public class glTF_VRM_Humanoid : JsonSerializableBase
    {
        public List<glTF_VRM_HumanoidBone> humanBones = new List<glTF_VRM_HumanoidBone>();

        [JsonSchema(Description = "Unity's HumanDescription.armStretch")]
        public float armStretch = 0.05f;

        [JsonSchema(Description = "Unity's HumanDescription.legStretch")]
        public float legStretch = 0.05f;

        [JsonSchema(Description = "Unity's HumanDescription.upperArmTwist")]
        public float upperArmTwist = 0.5f;

        [JsonSchema(Description = "Unity's HumanDescription.lowerArmTwist")]
        public float lowerArmTwist = 0.5f;

        [JsonSchema(Description = "Unity's HumanDescription.upperLegTwist")]
        public float upperLegTwist = 0.5f;

        [JsonSchema(Description = "Unity's HumanDescription.lowerLegTwist")]
        public float lowerLegTwist = 0.5f;

        [JsonSchema(Description = "Unity's HumanDescription.feetSpacing")]
        public float feetSpacing = 0;

        [JsonSchema(Description = "Unity's HumanDescription.hasTranslationDoF")]
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

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.Key("humanBones"); f.GLTFValue(humanBones);
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

            var boneLimits = new UniHumanoid.BoneLimit[humanBones.Count];
            int index = 0;
            foreach (var x in humanBones)
            {
                if (x.node < 0 || x.node >= nodes.Count) continue;
                boneLimits[index] = new UniHumanoid.BoneLimit
                {
                    boneName = nodes[x.node].name,
                    useDefaultValues = x.useDefaultValues,
                    axisLength = x.axisLength,
                    center = x.center,
                    min = x.min,
                    max = x.max,
                    humanBone = x.vrmBone.ToHumanBodyBone(),
                };
                index++;
            }

            description.human = boneLimits;

            return description;
        }
    }
}