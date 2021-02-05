using System;
using UniJSON;

namespace UniVRM10
{
    // {
    //   "bone": "hips",
    //   "node": 14,
    //   "useDefaultValues": true,
    //   "min": {
    //       "x": 0,
    //       "y": 0,
    //       "z": 0
    //   },
    //   "max": {
    //       "x": 0,
    //       "y": 0,
    //       "z": 0
    //   },
    //   "center": {
    //       "x": 0,
    //       "y": 0,
    //       "z": 0
    //   },
    //   "axisLength": 0
    // },
    public static class MigrationVrmHumanoid
    {
        static UniGLTF.Extensions.VRMC_vrm.HumanBone MigrateHumanoidBone(JsonNode vrm0)
        {
            return new UniGLTF.Extensions.VRMC_vrm.HumanBone
            {
                Node = vrm0["node"].GetInt32(),
            };
        }

        public static UniGLTF.Extensions.VRMC_vrm.Humanoid Migrate(JsonNode vrm0)
        {
            var humanoid = new UniGLTF.Extensions.VRMC_vrm.Humanoid
            {
                HumanBones = new UniGLTF.Extensions.VRMC_vrm.HumanBones()
            };

            foreach (var humanoidBone in vrm0["humanBones"].ArrayItems())
            {
                var boneType = humanoidBone["bone"].GetString();
                switch (boneType)
                {
                    case "hips": humanoid.HumanBones.Hips = MigrateHumanoidBone(humanoidBone); break;
                    case "leftUpperLeg": humanoid.HumanBones.LeftUpperLeg = MigrateHumanoidBone(humanoidBone); break;
                    case "rightUpperLeg": humanoid.HumanBones.RightUpperLeg = MigrateHumanoidBone(humanoidBone); break;
                    case "leftLowerLeg": humanoid.HumanBones.LeftLowerLeg = MigrateHumanoidBone(humanoidBone); break;
                    case "rightLowerLeg": humanoid.HumanBones.RightLowerLeg = MigrateHumanoidBone(humanoidBone); break;
                    case "leftFoot": humanoid.HumanBones.LeftFoot = MigrateHumanoidBone(humanoidBone); break;
                    case "rightFoot": humanoid.HumanBones.RightFoot = MigrateHumanoidBone(humanoidBone); break;
                    case "spine": humanoid.HumanBones.Spine = MigrateHumanoidBone(humanoidBone); break;
                    case "chest": humanoid.HumanBones.Chest = MigrateHumanoidBone(humanoidBone); break;
                    case "neck": humanoid.HumanBones.Neck = MigrateHumanoidBone(humanoidBone); break;
                    case "head": humanoid.HumanBones.Head = MigrateHumanoidBone(humanoidBone); break;
                    case "leftShoulder": humanoid.HumanBones.LeftShoulder = MigrateHumanoidBone(humanoidBone); break;
                    case "rightShoulder": humanoid.HumanBones.RightShoulder = MigrateHumanoidBone(humanoidBone); break;
                    case "leftUpperArm": humanoid.HumanBones.LeftUpperArm = MigrateHumanoidBone(humanoidBone); break;
                    case "rightUpperArm": humanoid.HumanBones.RightUpperArm = MigrateHumanoidBone(humanoidBone); break;
                    case "leftLowerArm": humanoid.HumanBones.LeftLowerArm = MigrateHumanoidBone(humanoidBone); break;
                    case "rightLowerArm": humanoid.HumanBones.RightLowerArm = MigrateHumanoidBone(humanoidBone); break;
                    case "leftHand": humanoid.HumanBones.LeftHand = MigrateHumanoidBone(humanoidBone); break;
                    case "rightHand": humanoid.HumanBones.RightHand = MigrateHumanoidBone(humanoidBone); break;
                    case "leftToes": humanoid.HumanBones.LeftToes = MigrateHumanoidBone(humanoidBone); break;
                    case "rightToes": humanoid.HumanBones.RightToes = MigrateHumanoidBone(humanoidBone); break;
                    case "leftEye": humanoid.HumanBones.LeftEye = MigrateHumanoidBone(humanoidBone); break;
                    case "rightEye": humanoid.HumanBones.RightEye = MigrateHumanoidBone(humanoidBone); break;
                    case "jaw": humanoid.HumanBones.Jaw = MigrateHumanoidBone(humanoidBone); break;
                    case "leftThumbProximal": humanoid.HumanBones.LeftThumbProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftThumbIntermediate": humanoid.HumanBones.LeftThumbIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "leftThumbDistal": humanoid.HumanBones.LeftThumbDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftIndexProximal": humanoid.HumanBones.LeftIndexProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftIndexIntermediate": humanoid.HumanBones.LeftIndexIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "leftIndexDistal": humanoid.HumanBones.LeftIndexDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftMiddleProximal": humanoid.HumanBones.LeftMiddleProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftMiddleIntermediate": humanoid.HumanBones.LeftMiddleIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "leftMiddleDistal": humanoid.HumanBones.LeftMiddleDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftRingProximal": humanoid.HumanBones.LeftRingProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftRingIntermediate": humanoid.HumanBones.LeftRingIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "leftRingDistal": humanoid.HumanBones.LeftRingDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftLittleProximal": humanoid.HumanBones.LeftLittleProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftLittleIntermediate": humanoid.HumanBones.LeftLittleIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "leftLittleDistal": humanoid.HumanBones.LeftLittleDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightThumbProximal": humanoid.HumanBones.RightThumbProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightThumbIntermediate": humanoid.HumanBones.RightThumbIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "rightThumbDistal": humanoid.HumanBones.RightThumbDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightIndexProximal": humanoid.HumanBones.RightIndexProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightIndexIntermediate": humanoid.HumanBones.RightIndexIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "rightIndexDistal": humanoid.HumanBones.RightIndexDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightMiddleProximal": humanoid.HumanBones.RightMiddleProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightMiddleIntermediate": humanoid.HumanBones.RightMiddleIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "rightMiddleDistal": humanoid.HumanBones.RightMiddleDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightRingProximal": humanoid.HumanBones.RightRingProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightRingIntermediate": humanoid.HumanBones.RightRingIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "rightRingDistal": humanoid.HumanBones.RightRingDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightLittleProximal": humanoid.HumanBones.RightLittleProximal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightLittleIntermediate": humanoid.HumanBones.RightLittleIntermediate = MigrateHumanoidBone(humanoidBone); break;
                    case "rightLittleDistal": humanoid.HumanBones.RightLittleDistal = MigrateHumanoidBone(humanoidBone); break;
                    case "upperChest": humanoid.HumanBones.UpperChest = MigrateHumanoidBone(humanoidBone); break;
                    default: throw new NotImplementedException($"unknown bone: {boneType}");
                }
            }

            return humanoid;
        }
    }
}
