using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    //
    // extensions.VRMC_vrm_animation.extras.UNIVRM_pose
    //
    // no jsonscheme.
    //
    // extensions: {
    //   VRMC_vrm_animation: {
    //     humanoid : {
    //       humanBones: {}
    //     },
    //     extras: {
    //       UNIVRM_pose: {
    //         humanoid: {
    //           translation: [
    //             0,
    //             1,
    //             0
    //           ],
    //           rotations: {
    //             hips: [
    //               0,
    //               0.707,
    //               0,
    //               0.707
    //             ],
    //             spine: [
    //               0,
    //               0.707,
    //               0,
    //               0.707
    //             ],
    //             // ...
    //           }
    //         },
    //         expressions: {
    //           preset: {
    //             happy: 1.0,
    //           },
    //         },
    //         lookAt: {
    //           position: [
    //             4,
    //             5,
    //             6
    //           ],
    //           // yawPitchDegrees: [20, 30],
    //         }
    //       }
    //     }
    //   }
    // }
    public static class Vrm10PoseLoader
    {
        static Vector3 ToVec3(JsonNode j)
        {
            return new Vector3(-j[0].GetSingle(), j[1].GetSingle(), j[2].GetSingle());
        }

        static Quaternion ToQuat(JsonNode j)
        {
            return new Quaternion(j[0].GetSingle(), -j[1].GetSingle(), -j[2].GetSingle(), j[3].GetSingle());
        }

        static (Vector3, Dictionary<HumanBodyBones, Quaternion>) GetPose(JsonNode humanoid)
        {
            Vector3 root = default;
            var map = new Dictionary<HumanBodyBones, Quaternion>();

            if (humanoid.TryGet("translation", out var translation))
            {
                root = ToVec3(translation);
            }
            if (humanoid.TryGet("rotations", out var rotations))
            {
                foreach (var kv in rotations.ObjectItems())
                {
                    switch (kv.Key.GetString())
                    {
                        case "hips": map.Add(HumanBodyBones.Hips, ToQuat(kv.Value)); break;
                        case "spine": map.Add(HumanBodyBones.Spine, ToQuat(kv.Value)); break;
                        case "chest": map.Add(HumanBodyBones.Chest, ToQuat(kv.Value)); break;
                        case "upperChest": map.Add(HumanBodyBones.UpperChest, ToQuat(kv.Value)); break;
                        case "neck": map.Add(HumanBodyBones.Neck, ToQuat(kv.Value)); break;
                        case "head": map.Add(HumanBodyBones.Head, ToQuat(kv.Value)); break;
                        // case "leftEye": map.Add(HumanBodyBones.leftEye, ToQuat(kv.Value)); break;
                        // case "rightEye": map.Add(HumanBodyBones.rightEye, ToQuat(kv.Value)); break;
                        case "jaw": map.Add(HumanBodyBones.Jaw, ToQuat(kv.Value)); break;
                        case "leftShoulder": map.Add(HumanBodyBones.LeftShoulder, ToQuat(kv.Value)); break;
                        case "leftUpperArm": map.Add(HumanBodyBones.LeftUpperArm, ToQuat(kv.Value)); break;
                        case "leftLowerArm": map.Add(HumanBodyBones.LeftLowerArm, ToQuat(kv.Value)); break;
                        case "leftHand": map.Add(HumanBodyBones.LeftHand, ToQuat(kv.Value)); break;
                        case "rightShoulder": map.Add(HumanBodyBones.RightShoulder, ToQuat(kv.Value)); break;
                        case "rightUpperArm": map.Add(HumanBodyBones.RightUpperArm, ToQuat(kv.Value)); break;
                        case "rightLowerArm": map.Add(HumanBodyBones.RightLowerArm, ToQuat(kv.Value)); break;
                        case "rightHand": map.Add(HumanBodyBones.RightHand, ToQuat(kv.Value)); break;
                        case "leftUpperLeg": map.Add(HumanBodyBones.LeftUpperLeg, ToQuat(kv.Value)); break;
                        case "leftLowerLeg": map.Add(HumanBodyBones.LeftLowerLeg, ToQuat(kv.Value)); break;
                        case "leftFoot": map.Add(HumanBodyBones.LeftFoot, ToQuat(kv.Value)); break;
                        case "leftToes": map.Add(HumanBodyBones.LeftToes, ToQuat(kv.Value)); break;
                        case "rightUpperLeg": map.Add(HumanBodyBones.RightUpperLeg, ToQuat(kv.Value)); break;
                        case "rightLowerLeg": map.Add(HumanBodyBones.RightLowerLeg, ToQuat(kv.Value)); break;
                        case "rightFoot": map.Add(HumanBodyBones.RightFoot, ToQuat(kv.Value)); break;
                        case "rightToes": map.Add(HumanBodyBones.RightToes, ToQuat(kv.Value)); break;
                        case "leftThumbMetacarpal": map.Add(HumanBodyBones.LeftThumbProximal, ToQuat(kv.Value)); break;
                        case "leftThumbProximal": map.Add(HumanBodyBones.LeftThumbIntermediate, ToQuat(kv.Value)); break;
                        case "leftThumbDistal": map.Add(HumanBodyBones.LeftThumbDistal, ToQuat(kv.Value)); break;
                        case "leftIndexProximal": map.Add(HumanBodyBones.LeftIndexProximal, ToQuat(kv.Value)); break;
                        case "leftIndexIntermediate": map.Add(HumanBodyBones.LeftIndexIntermediate, ToQuat(kv.Value)); break;
                        case "leftIndexDistal": map.Add(HumanBodyBones.LeftIndexDistal, ToQuat(kv.Value)); break;
                        case "leftMiddleProximal": map.Add(HumanBodyBones.LeftMiddleProximal, ToQuat(kv.Value)); break;
                        case "leftMiddleIntermediate": map.Add(HumanBodyBones.LeftMiddleIntermediate, ToQuat(kv.Value)); break;
                        case "leftMiddleDistal": map.Add(HumanBodyBones.LeftMiddleDistal, ToQuat(kv.Value)); break;
                        case "leftRingProximal": map.Add(HumanBodyBones.LeftRingProximal, ToQuat(kv.Value)); break;
                        case "leftRingIntermediate": map.Add(HumanBodyBones.LeftRingIntermediate, ToQuat(kv.Value)); break;
                        case "leftRingDistal": map.Add(HumanBodyBones.LeftRingDistal, ToQuat(kv.Value)); break;
                        case "leftLittleProximal": map.Add(HumanBodyBones.LeftLittleProximal, ToQuat(kv.Value)); break;
                        case "leftLittleIntermediate": map.Add(HumanBodyBones.LeftLittleIntermediate, ToQuat(kv.Value)); break;
                        case "leftLittleDistal": map.Add(HumanBodyBones.LeftLittleDistal, ToQuat(kv.Value)); break;
                        case "rightThumbMetacarpal": map.Add(HumanBodyBones.RightThumbProximal, ToQuat(kv.Value)); break;
                        case "rightThumbProximal": map.Add(HumanBodyBones.RightThumbIntermediate, ToQuat(kv.Value)); break;
                        case "rightThumbDistal": map.Add(HumanBodyBones.RightThumbDistal, ToQuat(kv.Value)); break;
                        case "rightIndexProximal": map.Add(HumanBodyBones.RightIndexProximal, ToQuat(kv.Value)); break;
                        case "rightIndexIntermediate": map.Add(HumanBodyBones.RightIndexIntermediate, ToQuat(kv.Value)); break;
                        case "rightIndexDistal": map.Add(HumanBodyBones.RightIndexDistal, ToQuat(kv.Value)); break;
                        case "rightMiddleProximal": map.Add(HumanBodyBones.RightMiddleProximal, ToQuat(kv.Value)); break;
                        case "rightMiddleIntermediate": map.Add(HumanBodyBones.RightMiddleIntermediate, ToQuat(kv.Value)); break;
                        case "rightMiddleDistal": map.Add(HumanBodyBones.RightMiddleDistal, ToQuat(kv.Value)); break;
                        case "rightRingProximal": map.Add(HumanBodyBones.RightRingProximal, ToQuat(kv.Value)); break;
                        case "rightRingIntermediate": map.Add(HumanBodyBones.RightRingIntermediate, ToQuat(kv.Value)); break;
                        case "rightRingDistal": map.Add(HumanBodyBones.RightRingDistal, ToQuat(kv.Value)); break;
                        case "rightLittleProximal": map.Add(HumanBodyBones.RightLittleProximal, ToQuat(kv.Value)); break;
                        case "rightLittleIntermediate": map.Add(HumanBodyBones.RightLittleIntermediate, ToQuat(kv.Value)); break;
                        case "rightLittleDistal": map.Add(HumanBodyBones.RightLittleDistal, ToQuat(kv.Value)); break;
                    }
                }
            }

            return (root, map);
        }

        static void LoadHumanPose(Vrm10AnimationInstance instance,
                UniJSON.JsonNode humanoid)
        {
            var (hips, map) = GetPose(humanoid);

            var animator = instance.GetComponentOrThrow<Animator>();

            // update src ControlRig
            animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = hips;

            foreach (var kv in map)
            {
                var t = animator.GetBoneTransform(kv.Key);
                if (t != null)
                {
                    t.localRotation = kv.Value;
                }
            }
        }

        static void LoadExpressions(Vrm10AnimationInstance instance,
                UniJSON.JsonNode expressions)
        {
            if (expressions.TryGet("preset", out var preset))
            {
                foreach (var kv in preset.ObjectItems())
                {
                    switch (kv.Key.GetString())
                    {
                        case "happy": instance.preset_happy = kv.Value.GetSingle(); break;
                        case "angry": instance.preset_angry = kv.Value.GetSingle(); break;
                        case "sad": instance.preset_sad = kv.Value.GetSingle(); break;
                        case "relaxed": instance.preset_relaxed = kv.Value.GetSingle(); break;
                        case "surprised": instance.preset_surprised = kv.Value.GetSingle(); break;
                        case "aa": instance.preset_aa = kv.Value.GetSingle(); break;
                        case "ih": instance.preset_ih = kv.Value.GetSingle(); break;
                        case "ou": instance.preset_ou = kv.Value.GetSingle(); break;
                        case "ee": instance.preset_ee = kv.Value.GetSingle(); break;
                        case "oh": instance.preset_oh = kv.Value.GetSingle(); break;
                        case "blink": instance.preset_blink = kv.Value.GetSingle(); break;
                        case "blinkLeft": instance.preset_blinkleft = kv.Value.GetSingle(); break;
                        case "blinkRight": instance.preset_blinkright = kv.Value.GetSingle(); break;
                        // case "lookUp": instance.preset_lookUp = kv.Value.GetSingle(); break;
                        // case "lookDown": instance.preset_lookDown = kv.Value.GetSingle(); break;
                        // case "lookLeft": instance.preset_lookLeft = kv.Value.GetSingle(); break;
                        // case "lookRight": instance.preset_lookRight = kv.Value.GetSingle(); break;
                        case "neutral": instance.preset_neutral = kv.Value.GetSingle(); break;
                    }
                }
            }
            if (expressions.TryGet("custom", out var custom))
            {
                foreach (var kv in preset.ObjectItems())
                {
                    if (instance.ExpressionSetterMap.TryGetValue(ExpressionKey.CreateCustom(kv.Key.GetString()), out var setter))
                    {
                        setter(kv.Key.GetSingle());
                    }
                }
            }
        }

        static void LoadLookAt(Vrm10AnimationInstance instance,
                UniJSON.JsonNode lookAt)
        {
            if (lookAt.TryGet("position", out var position))
            {
                // 注視点. 座標系?
            }
        }

        public static async Task<Vrm10AnimationInstance> LoadVrmAnimationPose(string text)
        {
            using GltfData data = GlbLowLevelParser.ParseGltf(
                "tmp.vrma",
                text,
                new List<GlbChunk>(), // .gltf file has no chunks.
                new FileSystemStorage("_dummy_root_"), // .gltf file has resource path at file system.
                new MigrationFlags()
            );
            using var loader = new VrmAnimationImporter(data);
            var gltfInstance = await loader.LoadAsync(new ImmediateCaller());
            var instance = gltfInstance.GetComponentOrThrow<Vrm10AnimationInstance>();

            if (data.GLTF.extensions is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetString() == "VRMC_vrm_animation")
                    {
                        if (kv.Value.TryGet("extras", out var extras))
                        {
                            if (extras.TryGet("UNIVRM_pose", out var pose))
                            {
                                if (pose.TryGet("humanoid", out var humanoid))
                                {
                                    LoadHumanPose(instance, humanoid);
                                }

                                if (extras.TryGet("expressions", out var expressions))
                                {
                                    LoadExpressions(instance, expressions);
                                }

                                if (extras.TryGet("lookAt", out var lookAt))
                                {
                                    LoadLookAt(instance, lookAt);
                                }
                            }
                        }
                    }
                }
            }

            return instance;
        }
    }
}
