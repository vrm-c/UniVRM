using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    public sealed class Vrm10HumanoidBoneSpecification
    {
        /// <summary>
        /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_vrm-1.0/humanoid.md
        /// </summary>
        private readonly List<Vrm10HumanoidBoneAttribute> _specification = new List<Vrm10HumanoidBoneAttribute>
        {
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.Hips, true, null, null, HumanBodyBones.Hips),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.Spine, true, Vrm10HumanoidBones.Hips, null, HumanBodyBones.Spine),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.Chest, false, Vrm10HumanoidBones.Spine, null, HumanBodyBones.Chest),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.UpperChest, false, Vrm10HumanoidBones.Chest, true, HumanBodyBones.UpperChest),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.Neck, false, Vrm10HumanoidBones.UpperChest, false, HumanBodyBones.Neck),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.Head, true, Vrm10HumanoidBones.Neck, false, HumanBodyBones.Head),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftEye, false, Vrm10HumanoidBones.Head, null, HumanBodyBones.LeftEye),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightEye, false, Vrm10HumanoidBones.Head, null, HumanBodyBones.RightEye),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.Jaw, false, Vrm10HumanoidBones.Head, null, HumanBodyBones.Jaw),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftUpperLeg, true, Vrm10HumanoidBones.Hips, null, HumanBodyBones.LeftUpperLeg),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftLowerLeg, true, Vrm10HumanoidBones.LeftUpperLeg, null, HumanBodyBones.LeftLowerLeg),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftFoot, true, Vrm10HumanoidBones.LeftLowerLeg, null, HumanBodyBones.LeftFoot),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftToes, false, Vrm10HumanoidBones.LeftFoot, null, HumanBodyBones.LeftToes),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightUpperLeg, true, Vrm10HumanoidBones.Hips, null, HumanBodyBones.RightUpperLeg),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightLowerLeg, true, Vrm10HumanoidBones.RightUpperLeg, null, HumanBodyBones.RightLowerLeg),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightFoot, true, Vrm10HumanoidBones.RightLowerLeg, null, HumanBodyBones.RightFoot),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightToes, false, Vrm10HumanoidBones.RightFoot, null, HumanBodyBones.RightToes),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftShoulder, false, Vrm10HumanoidBones.UpperChest, false, HumanBodyBones.LeftShoulder),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftUpperArm, true, Vrm10HumanoidBones.LeftShoulder, false, HumanBodyBones.LeftUpperArm),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftLowerArm, true, Vrm10HumanoidBones.LeftUpperArm, null, HumanBodyBones.LeftLowerArm),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftHand, true, Vrm10HumanoidBones.LeftLowerArm, null, HumanBodyBones.LeftHand),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightShoulder, false, Vrm10HumanoidBones.UpperChest, false, HumanBodyBones.RightShoulder),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightUpperArm, true, Vrm10HumanoidBones.RightShoulder, false, HumanBodyBones.RightUpperArm),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightLowerArm, true, Vrm10HumanoidBones.RightUpperArm, null, HumanBodyBones.RightLowerArm),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightHand, true, Vrm10HumanoidBones.RightLowerArm, null, HumanBodyBones.RightHand),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftThumbMetacarpal, false, Vrm10HumanoidBones.LeftHand, null, HumanBodyBones.LeftThumbProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftThumbProximal, false, Vrm10HumanoidBones.LeftThumbMetacarpal, true, HumanBodyBones.LeftThumbIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftThumbDistal, false, Vrm10HumanoidBones.LeftThumbProximal, true, HumanBodyBones.LeftThumbDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftIndexProximal, false, Vrm10HumanoidBones.LeftHand, null, HumanBodyBones.LeftIndexProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftIndexIntermediate, false, Vrm10HumanoidBones.LeftIndexProximal, true, HumanBodyBones.LeftIndexIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftIndexDistal, false, Vrm10HumanoidBones.LeftIndexIntermediate, true, HumanBodyBones.LeftIndexDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftMiddleProximal, false, Vrm10HumanoidBones.LeftHand, null, HumanBodyBones.LeftMiddleProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftMiddleIntermediate, false, Vrm10HumanoidBones.LeftMiddleProximal, true, HumanBodyBones.LeftMiddleIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftMiddleDistal, false, Vrm10HumanoidBones.LeftMiddleIntermediate, true, HumanBodyBones.LeftMiddleDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftRingProximal, false, Vrm10HumanoidBones.LeftHand, null, HumanBodyBones.LeftRingProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftRingIntermediate, false, Vrm10HumanoidBones.LeftRingProximal, true, HumanBodyBones.LeftRingIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftRingDistal, false, Vrm10HumanoidBones.LeftRingIntermediate, true, HumanBodyBones.LeftRingDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftLittleProximal, false, Vrm10HumanoidBones.LeftHand, null, HumanBodyBones.LeftLittleProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftLittleIntermediate, false, Vrm10HumanoidBones.LeftLittleProximal, true, HumanBodyBones.LeftLittleIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.LeftLittleDistal, false, Vrm10HumanoidBones.LeftLittleIntermediate, true, HumanBodyBones.LeftLittleDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightThumbMetacarpal, false, Vrm10HumanoidBones.RightHand, null, HumanBodyBones.RightThumbProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightThumbProximal, false, Vrm10HumanoidBones.RightThumbMetacarpal, true, HumanBodyBones.RightThumbIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightThumbDistal, false, Vrm10HumanoidBones.RightThumbProximal, true, HumanBodyBones.RightThumbDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightIndexProximal, false, Vrm10HumanoidBones.RightHand, null, HumanBodyBones.RightIndexProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightIndexIntermediate, false, Vrm10HumanoidBones.RightIndexProximal, true, HumanBodyBones.RightIndexIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightIndexDistal, false, Vrm10HumanoidBones.RightIndexIntermediate, true, HumanBodyBones.RightIndexDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightMiddleProximal, false, Vrm10HumanoidBones.RightHand, null, HumanBodyBones.RightMiddleProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightMiddleIntermediate, false, Vrm10HumanoidBones.RightMiddleProximal, true, HumanBodyBones.RightMiddleIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightMiddleDistal, false, Vrm10HumanoidBones.RightMiddleIntermediate, true, HumanBodyBones.RightMiddleDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightRingProximal, false, Vrm10HumanoidBones.RightHand, null, HumanBodyBones.RightRingProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightRingIntermediate, false, Vrm10HumanoidBones.RightRingProximal, true, HumanBodyBones.RightRingIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightRingDistal, false, Vrm10HumanoidBones.RightRingIntermediate, true, HumanBodyBones.RightRingDistal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightLittleProximal, false, Vrm10HumanoidBones.RightHand, null, HumanBodyBones.RightLittleProximal),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightLittleIntermediate, false, Vrm10HumanoidBones.RightLittleProximal, true, HumanBodyBones.RightLittleIntermediate),
            new Vrm10HumanoidBoneAttribute(Vrm10HumanoidBones.RightLittleDistal, false, Vrm10HumanoidBones.RightLittleIntermediate, true, HumanBodyBones.RightLittleDistal),
        };

        private readonly Dictionary<Vrm10HumanoidBones, Vrm10HumanoidBoneAttribute> _specDictionary;
        private readonly Dictionary<Vrm10HumanoidBones, HumanBodyBones> _toUnity;
        private readonly Dictionary<HumanBodyBones, Vrm10HumanoidBones> _fromUnity;

        private Vrm10HumanoidBoneSpecification()
        {
            _specDictionary = _specification.ToDictionary(x => x.Bone, x => x);
            _toUnity = _specification.ToDictionary(x => x.Bone, x => x.UnityBone);
            _fromUnity = _specification.ToDictionary(x => x.UnityBone, x => x.Bone);
        }

        private static Vrm10HumanoidBoneSpecification _instance;

        private static Vrm10HumanoidBoneSpecification Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Vrm10HumanoidBoneSpecification();
                }
                return _instance;
            }
        }

        public static Vrm10HumanoidBoneAttribute GetDefine(Vrm10HumanoidBones bone)
        {
            return Instance._specDictionary[bone];
        }

        public static HumanBodyBones ConvertToUnityBone(Vrm10HumanoidBones bone)
        {
            return Instance._toUnity[bone];
        }

        public static Vrm10HumanoidBones ConvertFromUnityBone(HumanBodyBones bone)
        {
            return Instance._fromUnity[bone];
        }
    }
}