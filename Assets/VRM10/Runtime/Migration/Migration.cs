using System;
using System.Collections.Generic;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    /// <summary>
    /// Convert vrm0 binary to vrm1 binary. Json processing
    /// </summary>
    public static class Migration
    {
        static bool TryGet(this UniGLTF.glTFExtensionImport extensions, string key, out ListTreeNode<JsonValue> value)
        {
            foreach (var kv in extensions.ObjectItems())
            {
                if (kv.Key.GetString() == key)
                {
                    value = kv.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

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
        static UniGLTF.Extensions.VRMC_vrm.HumanBone MigrateHumanoidBone(ListTreeNode<JsonValue> vrm0)
        {
            return new HumanBone
            {
                Node = vrm0["node"].GetInt32(),
            };
        }

        static UniGLTF.Extensions.VRMC_vrm.Humanoid MigrateHumanoid(ListTreeNode<JsonValue> vrm0)
        {
            var humanoid = new UniGLTF.Extensions.VRMC_vrm.Humanoid
            {
                HumanBones = new HumanBones()
            };

            foreach (var humanoidBone in vrm0["humanBones"].ArrayItems())
            {
                var boneType = humanoidBone["bone"].GetString();
                switch (boneType)
                {
                    case "hips": humanoid.HumanBones.Hips = MigrateHumanoidBone(humanoidBone); break;
                    case "leftUpperLeg,": break;
                    case "rightUpperLeg,": break;
                    case "leftLowerLeg,": break;
                    case "rightLowerLeg,": break;
                    case "leftFoot,": break;
                    case "rightFoot,": break;
                    case "spine,": break;
                    case "chest,": break;
                    case "neck,": break;
                    case "head,": break;
                    case "leftShoulder,": break;
                    case "rightShoulder,": break;
                    case "leftUpperArm,": break;
                    case "rightUpperArm,": break;
                    case "leftLowerArm,": break;
                    case "rightLowerArm,": break;
                    case "leftHand,": break;
                    case "rightHand,": break;
                    case "leftToes,": break;
                    case "rightToes,": break;
                    case "leftEye,": break;
                    case "rightEye,": break;
                    case "jaw,": break;
                    case "leftThumbProximal,": break;
                    case "leftThumbIntermediate,": break;
                    case "leftThumbDistal,": break;
                    case "leftIndexProximal,": break;
                    case "leftIndexIntermediate,": break;
                    case "leftIndexDistal,": break;
                    case "leftMiddleProximal,": break;
                    case "leftMiddleIntermediate,": break;
                    case "leftMiddleDistal,": break;
                    case "leftRingProximal,": break;
                    case "leftRingIntermediate,": break;
                    case "leftRingDistal,": break;
                    case "leftLittleProximal,": break;
                    case "leftLittleIntermediate,": break;
                    case "leftLittleDistal,": break;
                    case "rightThumbProximal,": break;
                    case "rightThumbIntermediate,": break;
                    case "rightThumbDistal,": break;
                    case "rightIndexProximal,": break;
                    case "rightIndexIntermediate,": break;
                    case "rightIndexDistal,": break;
                    case "rightMiddleProximal,": break;
                    case "rightMiddleIntermediate,": break;
                    case "rightMiddleDistal,": break;
                    case "rightRingProximal,": break;
                    case "rightRingIntermediate,": break;
                    case "rightRingDistal,": break;
                    case "rightLittleProximal,": break;
                    case "rightLittleIntermediate,": break;
                    case "rightLittleDistal,": break;
                    case "upperChest,": break;
                }
            }

            return humanoid;
        }

        public static byte[] Migrate(byte[] src)
        {
            var glb = UniGLTF.Glb.Parse(src);
            var json = glb.Json.Bytes.ParseAsJson();

            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);
            if (!(gltf.extensions is UniGLTF.glTFExtensionImport import))
            {
                throw new Exception("not extensions");
            }
            if (!import.TryGet("VRM", out ListTreeNode<JsonValue> vrm))
            {
                throw new Exception("no vrm");
            }

            {
                var vrm1 = new VRMC_vrm();
                var vrm0 = json["extensions"]["VRM"];
                vrm1.Humanoid = MigrateHumanoid(vrm0["humanoid"]);

                var f = new JsonFormatter();
                GltfSerializer.Serialize(f, vrm1);
                gltf.extensions = new UniGLTF.glTFExtensionExport().Add(VRMC_vrm.ExtensionName, f.GetStoreBytes());
            }

            ArraySegment<byte> vrm1Json = default;
            {
                var f = new JsonFormatter();
                UniGLTF.GltfSerializer.Serialize(f, gltf);
                vrm1Json = f.GetStoreBytes();
            }

            return UniGLTF.Glb.Create(vrm1Json, glb.Binary.Bytes).ToBytes();
        }

        #region for UnitTest
        public static void CheckBone(string bone, ListTreeNode<JsonValue> vrm0, UniGLTF.Extensions.VRMC_vrm.HumanBone vrm1)
        {
            var vrm0NodeIndex = vrm0["node"].GetInt32();
            if (vrm0NodeIndex != vrm1.Node)
            {
                throw new Exception($"different {bone}: {vrm0NodeIndex} != {vrm1.Node}");
            }
        }

        public static void CheckHumanoid(ListTreeNode<JsonValue> vrm0, UniGLTF.Extensions.VRMC_vrm.Humanoid vrm1)
        {
            foreach (var humanoidBone in vrm0["humanBones"].ArrayItems())
            {
                var boneType = humanoidBone["bone"].GetString();
                switch (boneType)
                {
                    case "hips": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Hips); break;
                    case "leftUpperLeg,": break;
                    case "rightUpperLeg,": break;
                    case "leftLowerLeg,": break;
                    case "rightLowerLeg,": break;
                    case "leftFoot,": break;
                    case "rightFoot,": break;
                    case "spine,": break;
                    case "chest,": break;
                    case "neck,": break;
                    case "head,": break;
                    case "leftShoulder,": break;
                    case "rightShoulder,": break;
                    case "leftUpperArm,": break;
                    case "rightUpperArm,": break;
                    case "leftLowerArm,": break;
                    case "rightLowerArm,": break;
                    case "leftHand,": break;
                    case "rightHand,": break;
                    case "leftToes,": break;
                    case "rightToes,": break;
                    case "leftEye,": break;
                    case "rightEye,": break;
                    case "jaw,": break;
                    case "leftThumbProximal,": break;
                    case "leftThumbIntermediate,": break;
                    case "leftThumbDistal,": break;
                    case "leftIndexProximal,": break;
                    case "leftIndexIntermediate,": break;
                    case "leftIndexDistal,": break;
                    case "leftMiddleProximal,": break;
                    case "leftMiddleIntermediate,": break;
                    case "leftMiddleDistal,": break;
                    case "leftRingProximal,": break;
                    case "leftRingIntermediate,": break;
                    case "leftRingDistal,": break;
                    case "leftLittleProximal,": break;
                    case "leftLittleIntermediate,": break;
                    case "leftLittleDistal,": break;
                    case "rightThumbProximal,": break;
                    case "rightThumbIntermediate,": break;
                    case "rightThumbDistal,": break;
                    case "rightIndexProximal,": break;
                    case "rightIndexIntermediate,": break;
                    case "rightIndexDistal,": break;
                    case "rightMiddleProximal,": break;
                    case "rightMiddleIntermediate,": break;
                    case "rightMiddleDistal,": break;
                    case "rightRingProximal,": break;
                    case "rightRingIntermediate,": break;
                    case "rightRingDistal,": break;
                    case "rightLittleProximal,": break;
                    case "rightLittleIntermediate,": break;
                    case "rightLittleDistal,": break;
                    case "upperChest,": break;
                }
            }
        }
        #endregion
    }
}
