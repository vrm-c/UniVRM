using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRM
{
    public static class EnumExtensions
    {
        public static HumanBodyBones ToUnityBone(this VRMBone val)
        {
            switch (val)
            {
                case VRMBone.hips: return HumanBodyBones.Hips;//"hips"
                case VRMBone.leftUpperLeg: return HumanBodyBones.LeftUpperLeg;//"leftUpperLeg";
                case VRMBone.rightUpperLeg: return HumanBodyBones.RightUpperLeg;//"rightUpperLeg";
                case VRMBone.leftLowerLeg: return HumanBodyBones.LeftLowerLeg;//"leftLowerLeg";
                case VRMBone.rightLowerLeg: return HumanBodyBones.RightLowerLeg;//"rightLowerLeg";
                case VRMBone.leftFoot: return HumanBodyBones.LeftFoot;//"leftFoot";
                case VRMBone.rightFoot: return HumanBodyBones.RightFoot;//"rightFoot";
                case VRMBone.spine: return HumanBodyBones.Spine;//"spine";
                case VRMBone.chest: return HumanBodyBones.Chest;//"chest";
                case VRMBone.neck: return HumanBodyBones.Neck;//"neck";
                case VRMBone.head: return HumanBodyBones.Head;//"head";
                case VRMBone.leftShoulder: return HumanBodyBones.LeftShoulder;//"leftShoulder";
                case VRMBone.rightShoulder: return HumanBodyBones.RightShoulder;//"rightShoulder";
                case VRMBone.leftUpperArm: return HumanBodyBones.LeftUpperArm;//"leftUpperArm";
                case VRMBone.rightUpperArm: return HumanBodyBones.RightUpperArm;//"rightUpperArm";
                case VRMBone.leftLowerArm: return HumanBodyBones.LeftLowerArm;//"leftLowerArm";
                case VRMBone.rightLowerArm: return HumanBodyBones.RightLowerArm;//"rightLowerArm";
                case VRMBone.leftHand: return HumanBodyBones.LeftHand;//"leftHand";
                case VRMBone.rightHand: return HumanBodyBones.RightHand;//"rightHand";
                case VRMBone.leftToes: return HumanBodyBones.LeftToes;//"leftToes";
                case VRMBone.rightToes: return HumanBodyBones.RightToes;//"rightToes";
                case VRMBone.leftEye: return HumanBodyBones.LeftEye;//"leftEye";
                case VRMBone.rightEye: return HumanBodyBones.RightEye;//"rightEye";
                case VRMBone.jaw: return HumanBodyBones.Jaw;//"jaw";
                case VRMBone.leftThumbProximal: return HumanBodyBones.LeftThumbProximal;
                case VRMBone.leftThumbIntermediate: return HumanBodyBones.LeftThumbIntermediate;
                case VRMBone.leftThumbDistal: return HumanBodyBones.LeftThumbDistal;
                case VRMBone.leftIndexProximal: return HumanBodyBones.LeftIndexProximal;
                case VRMBone.leftIndexIntermediate: return HumanBodyBones.LeftIndexIntermediate;
                case VRMBone.leftIndexDistal: return HumanBodyBones.LeftIndexDistal;
                case VRMBone.leftMiddleProximal: return HumanBodyBones.LeftMiddleProximal;
                case VRMBone.leftMiddleIntermediate: return HumanBodyBones.LeftMiddleIntermediate;
                case VRMBone.leftMiddleDistal: return HumanBodyBones.LeftMiddleDistal;
                case VRMBone.leftRingProximal: return HumanBodyBones.LeftRingProximal;
                case VRMBone.leftRingIntermediate: return HumanBodyBones.LeftRingIntermediate;
                case VRMBone.leftRingDistal: return HumanBodyBones.LeftRingDistal;
                case VRMBone.leftLittleProximal: return HumanBodyBones.LeftLittleProximal;
                case VRMBone.leftLittleIntermediate: return HumanBodyBones.LeftLittleIntermediate;
                case VRMBone.leftLittleDistal: return HumanBodyBones.LeftLittleDistal;
                case VRMBone.rightThumbProximal: return HumanBodyBones.RightThumbProximal;
                case VRMBone.rightThumbIntermediate: return HumanBodyBones.RightThumbIntermediate;
                case VRMBone.rightThumbDistal: return HumanBodyBones.RightThumbDistal;
                case VRMBone.rightIndexProximal: return HumanBodyBones.RightIndexProximal;
                case VRMBone.rightIndexIntermediate: return HumanBodyBones.RightIndexIntermediate;
                case VRMBone.rightIndexDistal: return HumanBodyBones.RightIndexDistal;
                case VRMBone.rightMiddleProximal: return HumanBodyBones.RightMiddleProximal;
                case VRMBone.rightMiddleIntermediate: return HumanBodyBones.RightMiddleIntermediate;
                case VRMBone.rightMiddleDistal: return HumanBodyBones.RightMiddleDistal;
                case VRMBone.rightRingProximal: return HumanBodyBones.RightRingProximal;
                case VRMBone.rightRingIntermediate: return HumanBodyBones.RightRingIntermediate;
                case VRMBone.rightRingDistal: return HumanBodyBones.RightRingDistal;
                case VRMBone.rightLittleProximal: return HumanBodyBones.RightLittleProximal;
                case VRMBone.rightLittleIntermediate: return HumanBodyBones.RightLittleIntermediate;
                case VRMBone.rightLittleDistal: return HumanBodyBones.RightLittleDistal;
                case VRMBone.upperChest: return HumanBodyBones.UpperChest;
                default: throw new System.InvalidOperationException();
            }
        }

        
        public static VRMBone ToVrmBone(this HumanBodyBones val)
        {
            switch (val)
            {
                case HumanBodyBones.Hips: return VRMBone.hips;
                case HumanBodyBones.LeftUpperLeg: return VRMBone.leftUpperLeg;
                case HumanBodyBones.RightUpperLeg: return VRMBone.rightUpperLeg;
                case HumanBodyBones.LeftLowerLeg: return VRMBone.leftLowerLeg;
                case HumanBodyBones.RightLowerLeg: return VRMBone.rightLowerLeg;
                case HumanBodyBones.LeftFoot: return VRMBone.leftFoot;
                case HumanBodyBones.RightFoot: return VRMBone.rightFoot;
                case HumanBodyBones.Spine: return VRMBone.spine;
                case HumanBodyBones.Chest: return VRMBone.chest;
                case HumanBodyBones.Neck: return VRMBone.neck;
                case HumanBodyBones.Head: return VRMBone.head;
                case HumanBodyBones.LeftShoulder: return VRMBone.leftShoulder;
                case HumanBodyBones.RightShoulder: return VRMBone.rightShoulder;
                case HumanBodyBones.LeftUpperArm: return VRMBone.leftUpperArm;
                case HumanBodyBones.RightUpperArm: return VRMBone.rightUpperArm;
                case HumanBodyBones.LeftLowerArm: return VRMBone.leftLowerArm;
                case HumanBodyBones.RightLowerArm: return VRMBone.rightLowerArm;
                case HumanBodyBones.LeftHand: return VRMBone.leftHand;
                case HumanBodyBones.RightHand: return VRMBone.rightHand;
                case HumanBodyBones.LeftToes: return VRMBone.leftToes;
                case HumanBodyBones.RightToes: return VRMBone.rightToes;
                case HumanBodyBones.LeftEye: return VRMBone.leftEye;
                case HumanBodyBones.RightEye: return VRMBone.rightEye;
                case HumanBodyBones.Jaw: return VRMBone.jaw;
                case HumanBodyBones.LeftThumbProximal: return VRMBone.leftThumbProximal;
                case HumanBodyBones.LeftThumbIntermediate: return VRMBone.leftThumbIntermediate;
                case HumanBodyBones.LeftThumbDistal: return VRMBone.leftThumbDistal;
                case HumanBodyBones.LeftIndexProximal: return VRMBone.leftIndexProximal;
                case HumanBodyBones.LeftIndexIntermediate: return VRMBone.leftIndexIntermediate;
                case HumanBodyBones.LeftIndexDistal: return VRMBone.leftIndexDistal;
                case HumanBodyBones.LeftMiddleProximal: return VRMBone.leftMiddleProximal;
                case HumanBodyBones.LeftMiddleIntermediate: return VRMBone.leftMiddleIntermediate;
                case HumanBodyBones.LeftMiddleDistal: return VRMBone.leftMiddleDistal;
                case HumanBodyBones.LeftRingProximal: return VRMBone.leftRingProximal;
                case HumanBodyBones.LeftRingIntermediate: return VRMBone.leftRingIntermediate;
                case HumanBodyBones.LeftRingDistal: return VRMBone.leftRingDistal;
                case HumanBodyBones.LeftLittleProximal: return VRMBone.leftLittleProximal;
                case HumanBodyBones.LeftLittleIntermediate: return VRMBone.leftLittleIntermediate;
                case HumanBodyBones.LeftLittleDistal: return VRMBone.leftLittleDistal;
                case HumanBodyBones.RightThumbProximal: return VRMBone.rightThumbProximal;
                case HumanBodyBones.RightThumbIntermediate: return VRMBone.rightThumbIntermediate;
                case HumanBodyBones.RightThumbDistal: return VRMBone.rightThumbDistal;
                case HumanBodyBones.RightIndexProximal: return VRMBone.rightIndexProximal;
                case HumanBodyBones.RightIndexIntermediate: return VRMBone.rightIndexIntermediate;
                case HumanBodyBones.RightIndexDistal: return VRMBone.rightIndexDistal;
                case HumanBodyBones.RightMiddleProximal: return VRMBone.rightMiddleProximal;
                case HumanBodyBones.RightMiddleIntermediate: return VRMBone.rightMiddleIntermediate;
                case HumanBodyBones.RightMiddleDistal: return VRMBone.rightMiddleDistal;
                case HumanBodyBones.RightRingProximal: return VRMBone.rightRingProximal;
                case HumanBodyBones.RightRingIntermediate: return VRMBone.rightRingIntermediate;
                case HumanBodyBones.RightRingDistal: return VRMBone.rightRingDistal;
                case HumanBodyBones.RightLittleProximal: return VRMBone.rightLittleProximal;
                case HumanBodyBones.RightLittleIntermediate: return VRMBone.rightLittleIntermediate;
                case HumanBodyBones.RightLittleDistal: return VRMBone.rightLittleDistal;
                case HumanBodyBones.UpperChest: return VRMBone.upperChest;
                //case HumanBodyBones.LastBone: 
                default: throw new System.InvalidOperationException();
            }
        }
    }
}