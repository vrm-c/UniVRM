using System.Collections.Generic;
using UniGLTF.Extensions.VRMC_vrm_animation;

namespace UniVRM10
{
    public static class VrmAnimationUtil
    {
        public static VRMC_vrm_animation Create(
                Dictionary<UnityEngine.HumanBodyBones, UnityEngine.Transform> map,
                List<string> names)
        {
            var vrmAnimation = new VRMC_vrm_animation
            {
                Humanoid = new UniGLTF.Extensions.VRMC_vrm_animation.Humanoid
                {
                    HumanBones = new HumanBones(),
                }
            };
            foreach (var kv in map)
            {
                switch (kv.Key)
                {
                    case UnityEngine.HumanBodyBones.Hips: vrmAnimation.Humanoid.HumanBones.Hips = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftUpperLeg: vrmAnimation.Humanoid.HumanBones.LeftUpperLeg = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightUpperLeg: vrmAnimation.Humanoid.HumanBones.RightUpperLeg = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftLowerLeg: vrmAnimation.Humanoid.HumanBones.LeftLowerLeg = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightLowerLeg: vrmAnimation.Humanoid.HumanBones.RightLowerLeg = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftFoot: vrmAnimation.Humanoid.HumanBones.LeftFoot = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightFoot: vrmAnimation.Humanoid.HumanBones.RightFoot = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.Spine: vrmAnimation.Humanoid.HumanBones.Spine = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.Chest: vrmAnimation.Humanoid.HumanBones.Chest = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.UpperChest: vrmAnimation.Humanoid.HumanBones.UpperChest = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.Neck: vrmAnimation.Humanoid.HumanBones.Neck = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.Head: vrmAnimation.Humanoid.HumanBones.Head = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftShoulder: vrmAnimation.Humanoid.HumanBones.LeftShoulder = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightShoulder: vrmAnimation.Humanoid.HumanBones.RightShoulder = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftUpperArm: vrmAnimation.Humanoid.HumanBones.LeftUpperArm = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightUpperArm: vrmAnimation.Humanoid.HumanBones.RightUpperArm = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftLowerArm: vrmAnimation.Humanoid.HumanBones.LeftLowerArm = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightLowerArm: vrmAnimation.Humanoid.HumanBones.RightLowerArm = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftHand: vrmAnimation.Humanoid.HumanBones.LeftHand = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightHand: vrmAnimation.Humanoid.HumanBones.RightHand = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftToes: vrmAnimation.Humanoid.HumanBones.LeftToes = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightToes: vrmAnimation.Humanoid.HumanBones.RightToes = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.Jaw: vrmAnimation.Humanoid.HumanBones.Jaw = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftThumbProximal: vrmAnimation.Humanoid.HumanBones.LeftThumbMetacarpal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftThumbIntermediate: vrmAnimation.Humanoid.HumanBones.LeftThumbProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftThumbDistal: vrmAnimation.Humanoid.HumanBones.LeftThumbDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftIndexProximal: vrmAnimation.Humanoid.HumanBones.LeftIndexProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftIndexIntermediate: vrmAnimation.Humanoid.HumanBones.LeftIndexIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftIndexDistal: vrmAnimation.Humanoid.HumanBones.LeftIndexDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftMiddleProximal: vrmAnimation.Humanoid.HumanBones.LeftMiddleProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftMiddleIntermediate: vrmAnimation.Humanoid.HumanBones.LeftMiddleIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftMiddleDistal: vrmAnimation.Humanoid.HumanBones.LeftMiddleDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftRingProximal: vrmAnimation.Humanoid.HumanBones.LeftRingProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftRingIntermediate: vrmAnimation.Humanoid.HumanBones.LeftRingIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftRingDistal: vrmAnimation.Humanoid.HumanBones.LeftRingDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftLittleProximal: vrmAnimation.Humanoid.HumanBones.LeftLittleProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftLittleIntermediate: vrmAnimation.Humanoid.HumanBones.LeftLittleIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.LeftLittleDistal: vrmAnimation.Humanoid.HumanBones.LeftLittleDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightThumbProximal: vrmAnimation.Humanoid.HumanBones.RightThumbMetacarpal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightThumbIntermediate: vrmAnimation.Humanoid.HumanBones.RightThumbProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightThumbDistal: vrmAnimation.Humanoid.HumanBones.RightThumbDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightIndexProximal: vrmAnimation.Humanoid.HumanBones.RightIndexProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightIndexIntermediate: vrmAnimation.Humanoid.HumanBones.RightIndexIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightIndexDistal: vrmAnimation.Humanoid.HumanBones.RightIndexDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightMiddleProximal: vrmAnimation.Humanoid.HumanBones.RightMiddleProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightMiddleIntermediate: vrmAnimation.Humanoid.HumanBones.RightMiddleIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightMiddleDistal: vrmAnimation.Humanoid.HumanBones.RightMiddleDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightRingProximal: vrmAnimation.Humanoid.HumanBones.RightRingProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightRingIntermediate: vrmAnimation.Humanoid.HumanBones.RightRingIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightRingDistal: vrmAnimation.Humanoid.HumanBones.RightRingDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightLittleProximal: vrmAnimation.Humanoid.HumanBones.RightLittleProximal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightLittleIntermediate: vrmAnimation.Humanoid.HumanBones.RightLittleIntermediate = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                    case UnityEngine.HumanBodyBones.RightLittleDistal: vrmAnimation.Humanoid.HumanBones.RightLittleDistal = new HumanBone { Node = names.IndexOf(kv.Value.name) }; break;
                }
            }

            return vrmAnimation;
        }
    }
}

