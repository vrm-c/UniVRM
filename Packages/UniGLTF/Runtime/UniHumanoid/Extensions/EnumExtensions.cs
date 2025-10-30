using UnityEngine;

namespace UniHumanoid
{
    public static class EnumExtensions
    {
        public static string ToStringFromEnum(this HumanBodyBones val, bool compareBoneTrait = false)
        {
            switch (val)
            {
                case HumanBodyBones.Hips: return "Hips";
                case HumanBodyBones.LeftUpperLeg: return "LeftUpperLeg";
                case HumanBodyBones.RightUpperLeg: return "RightUpperLeg";
                case HumanBodyBones.LeftLowerLeg: return "LeftLowerLeg";
                case HumanBodyBones.RightLowerLeg: return "RightLowerLeg";
                case HumanBodyBones.LeftFoot: return "LeftFoot";
                case HumanBodyBones.RightFoot: return "RightFoot";
                case HumanBodyBones.Spine: return "Spine";
                case HumanBodyBones.Chest: return "Chest";
                case HumanBodyBones.Neck: return "Neck";
                case HumanBodyBones.Head: return "Head";
                case HumanBodyBones.LeftShoulder: return "LeftShoulder";
                case HumanBodyBones.RightShoulder: return "RightShoulder";
                case HumanBodyBones.LeftUpperArm: return "LeftUpperArm";
                case HumanBodyBones.RightUpperArm: return "RightUpperArm";
                case HumanBodyBones.LeftLowerArm: return "LeftLowerArm";
                case HumanBodyBones.RightLowerArm: return "RightLowerArm";
                case HumanBodyBones.LeftHand: return "LeftHand";
                case HumanBodyBones.RightHand: return "RightHand";
                case HumanBodyBones.LeftToes: return "LeftToes";
                case HumanBodyBones.RightToes: return "RightToes";
                case HumanBodyBones.LeftEye: return "LeftEye";
                case HumanBodyBones.RightEye: return "RightEye";
                case HumanBodyBones.Jaw: return "Jaw";
                case HumanBodyBones.LeftThumbProximal: return compareBoneTrait ? "Left Thumb Proximal" : "LeftThumbProximal";
                case HumanBodyBones.LeftThumbIntermediate: return compareBoneTrait ? "Left Thumb Intermediate" : "LeftThumbIntermediate";
                case HumanBodyBones.LeftThumbDistal: return compareBoneTrait ? "Left Thumb Distal" : "LeftThumbDistal";
                case HumanBodyBones.LeftIndexProximal: return compareBoneTrait ? "Left Index Proximal" : "LeftIndexProximal";
                case HumanBodyBones.LeftIndexIntermediate: return compareBoneTrait ? "Left Index Intermediate" : "LeftIndexIntermediate";
                case HumanBodyBones.LeftIndexDistal: return compareBoneTrait ? "Left Index Distal" : "LeftIndexDistal";
                case HumanBodyBones.LeftMiddleProximal: return compareBoneTrait ? "Left Middle Proximal" : "LeftMiddleProximal";
                case HumanBodyBones.LeftMiddleIntermediate: return compareBoneTrait ? "Left Middle Intermediate" : "LeftMiddleIntermediate";
                case HumanBodyBones.LeftMiddleDistal: return compareBoneTrait ? "Left Middle Distal" : "LeftMiddleDistal";
                case HumanBodyBones.LeftRingProximal: return compareBoneTrait ? "Left Ring Proximal" : "LeftRingProximal";
                case HumanBodyBones.LeftRingIntermediate: return compareBoneTrait ? "Left Ring Intermediate" : "LeftRingIntermediate";
                case HumanBodyBones.LeftRingDistal: return compareBoneTrait ? "Left Ring Distal" : "LeftRingDistal";
                case HumanBodyBones.LeftLittleProximal: return compareBoneTrait ? "Left Little Proximal" : "LeftLittleProximal";
                case HumanBodyBones.LeftLittleIntermediate: return compareBoneTrait ? "Left Little Intermediate" : "LeftLittleIntermediate";
                case HumanBodyBones.LeftLittleDistal: return compareBoneTrait ? "Left Little Distal" : "LeftLittleDistal";
                case HumanBodyBones.RightThumbProximal: return compareBoneTrait ? "Right Thumb Proximal" : "RightThumbProximal";
                case HumanBodyBones.RightThumbIntermediate: return compareBoneTrait ? "Right Thumb Intermediate" : "RightThumbIntermediate";
                case HumanBodyBones.RightThumbDistal: return compareBoneTrait ? "Right Thumb Distal" : "RightThumbDistal";
                case HumanBodyBones.RightIndexProximal: return compareBoneTrait ? "Right Index Proximal" : "RightIndexProximal";
                case HumanBodyBones.RightIndexIntermediate: return compareBoneTrait ? "Right Index Intermediate" : "RightIndexIntermediate";
                case HumanBodyBones.RightIndexDistal: return compareBoneTrait ? "Right Index Distal" : "RightIndexDistal";
                case HumanBodyBones.RightMiddleProximal: return compareBoneTrait ? "Right Middle Proximal" : "RightMiddleProximal";
                case HumanBodyBones.RightMiddleIntermediate: return compareBoneTrait ? "Right Middle Intermediate" : "RightMiddleIntermediate";
                case HumanBodyBones.RightMiddleDistal: return compareBoneTrait ? "Right Middle Distal" : "RightMiddleDistal";
                case HumanBodyBones.RightRingProximal: return compareBoneTrait ? "Right Ring Proximal" : "RightRingProximal";
                case HumanBodyBones.RightRingIntermediate: return compareBoneTrait ? "Right Ring Intermediate" : "RightRingIntermediate";
                case HumanBodyBones.RightRingDistal: return compareBoneTrait ? "Right Ring Distal" : "RightRingDistal";
                case HumanBodyBones.RightLittleProximal: return compareBoneTrait ? "Right Little Proximal" : "RightLittleProximal";
                case HumanBodyBones.RightLittleIntermediate: return compareBoneTrait ? "Right Little Intermediate" : "RightLittleIntermediate";
                case HumanBodyBones.RightLittleDistal: return compareBoneTrait ? "Right Little Distal" : "RightLittleDistal";
                case HumanBodyBones.UpperChest: return "UpperChest";
                case HumanBodyBones.LastBone: return "LastBone";
                default: throw new System.InvalidOperationException();
            }
        }
    }
}