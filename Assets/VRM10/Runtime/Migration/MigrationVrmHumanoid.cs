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
                    case "leftThumbProximal": humanoid.HumanBones.LeftThumbMetacarpal = MigrateHumanoidBone(humanoidBone); break;
                    case "leftThumbIntermediate": humanoid.HumanBones.LeftThumbProximal = MigrateHumanoidBone(humanoidBone); break;
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
                    case "rightThumbProximal": humanoid.HumanBones.RightThumbMetacarpal = MigrateHumanoidBone(humanoidBone); break;
                    case "rightThumbIntermediate": humanoid.HumanBones.RightThumbProximal = MigrateHumanoidBone(humanoidBone); break;
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

        public static void CheckBone(string bone, JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.HumanBone vrm1)
        {
            var vrm0NodeIndex = vrm0["node"].GetInt32();
            if (vrm0NodeIndex != vrm1.Node)
            {
                throw new Exception($"different {bone}: {vrm0NodeIndex} != {vrm1.Node}");
            }
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.Humanoid vrm1)
        {
            foreach (var humanoidBone in vrm0["humanBones"].ArrayItems())
            {
                var boneType = humanoidBone["bone"].GetString();
                switch (boneType)
                {
                    case "hips": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Hips); break;
                    case "leftUpperLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftUpperLeg); break;
                    case "rightUpperLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightUpperLeg); break;
                    case "leftLowerLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLowerLeg); break;
                    case "rightLowerLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLowerLeg); break;
                    case "leftFoot": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftFoot); break;
                    case "rightFoot": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightFoot); break;
                    case "spine": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Spine); break;
                    case "chest": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Chest); break;
                    case "neck": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Neck); break;
                    case "head": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Head); break;
                    case "leftShoulder": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftShoulder); break;
                    case "rightShoulder": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightShoulder); break;
                    case "leftUpperArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftUpperArm); break;
                    case "rightUpperArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightUpperArm); break;
                    case "leftLowerArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLowerArm); break;
                    case "rightLowerArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLowerArm); break;
                    case "leftHand": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftHand); break;
                    case "rightHand": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightHand); break;
                    case "leftToes": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftToes); break;
                    case "rightToes": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightToes); break;
                    case "leftEye": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftEye); break;
                    case "rightEye": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightEye); break;
                    case "jaw": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Jaw); break;
                    case "leftThumbProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbMetacarpal); break;
                    case "leftThumbIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbProximal); break;
                    case "leftThumbDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbDistal); break;
                    case "leftIndexProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftIndexProximal); break;
                    case "leftIndexIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftIndexIntermediate); break;
                    case "leftIndexDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftIndexDistal); break;
                    case "leftMiddleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftMiddleProximal); break;
                    case "leftMiddleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftMiddleIntermediate); break;
                    case "leftMiddleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftMiddleDistal); break;
                    case "leftRingProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftRingProximal); break;
                    case "leftRingIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftRingIntermediate); break;
                    case "leftRingDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftRingDistal); break;
                    case "leftLittleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLittleProximal); break;
                    case "leftLittleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLittleIntermediate); break;
                    case "leftLittleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLittleDistal); break;
                    case "rightThumbProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbMetacarpal); break;
                    case "rightThumbIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbProximal); break;
                    case "rightThumbDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbDistal); break;
                    case "rightIndexProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightIndexProximal); break;
                    case "rightIndexIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightIndexIntermediate); break;
                    case "rightIndexDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightIndexDistal); break;
                    case "rightMiddleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightMiddleProximal); break;
                    case "rightMiddleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightMiddleIntermediate); break;
                    case "rightMiddleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightMiddleDistal); break;
                    case "rightRingProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightRingProximal); break;
                    case "rightRingIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightRingIntermediate); break;
                    case "rightRingDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightRingDistal); break;
                    case "rightLittleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLittleProximal); break;
                    case "rightLittleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLittleIntermediate); break;
                    case "rightLittleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLittleDistal); break;
                    case "upperChest": CheckBone(boneType, humanoidBone, vrm1.HumanBones.UpperChest); break;
                    default: throw new MigrationException("humanoid.humanBones[*].bone", boneType);
                }
            }
        }
    }
}
