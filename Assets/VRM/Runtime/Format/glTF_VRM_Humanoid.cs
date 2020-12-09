using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
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

    [Serializable]
    [JsonSchema(Title = "vrm.humanoid.bone")]
    public class glTF_VRM_HumanoidBone
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

        [JsonSchema(Description = "Unity's HumanLimit.min", SerializationConditions = new string[] { "value.min!=Vector3.zero" })]
        public Vector3 min;

        [JsonSchema(Description = "Unity's HumanLimit.max", SerializationConditions = new string[] { "value.min!=Vector3.zero" })]
        public Vector3 max;

        [JsonSchema(Description = "Unity's HumanLimit.center", SerializationConditions = new string[] { "value.min!=Vector3.zero" })]
        public Vector3 center;

        [JsonSchema(Description = "Unity's HumanLimit.axisLength", Minimum = 0, ExclusiveMinimum = true)]
        public float axisLength;
    }

    [Serializable]
    [JsonSchema(Title = "vrm.humanoid")]
    public class glTF_VRM_Humanoid
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
    }
}
