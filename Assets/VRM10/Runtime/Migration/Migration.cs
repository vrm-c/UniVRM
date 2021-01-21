using System;
using System.Collections.Generic;
using System.Linq;
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
            return new UniGLTF.Extensions.VRMC_vrm.HumanBone
            {
                Node = vrm0["node"].GetInt32(),
            };
        }

        static UniGLTF.Extensions.VRMC_vrm.Humanoid MigrateHumanoid(ListTreeNode<JsonValue> vrm0)
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

        ///
        /// 互換性の無いところ
        /// 
        /// * きつくなる方向は許す
        /// * 緩くなる方向は不許可(throw)
        /// 
        // "meta": {
        //   "title": "Alicia Solid",
        //   "version": "1.10",
        //   "author": "© DWANGO Co., Ltd.",
        //   "contactInformation": "https://3d.nicovideo.jp/alicia/",
        //   "reference": "",
        //   "texture": 7,
        //   "allowedUserName": "Everyone",
        //   "violentUssageName": "Disallow",
        //   "sexualUssageName": "Disallow",
        //   "commercialUssageName": "Allow",
        //   "otherPermissionUrl": "https://3d.nicovideo.jp/alicia/rule.html",
        //   "licenseName": "Other",
        //   "otherLicenseUrl": "https://3d.nicovideo.jp/alicia/rule.html"
        // },
        static UniGLTF.Extensions.VRMC_vrm.Meta MigrateMeta(ListTreeNode<JsonValue> vrm0)
        {
            var meta = new UniGLTF.Extensions.VRMC_vrm.Meta
            {
                AllowPoliticalOrReligiousUsage = false,
                AllowExcessivelySexualUsage = false,
                AllowExcessivelyViolentUsage = false,
                AllowRedistribution = false,
                AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.onlyAuthor,
                CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit,
                CreditNotation = UniGLTF.Extensions.VRMC_vrm.CreditNotationType.required,
                Modification = UniGLTF.Extensions.VRMC_vrm.ModificationType.prohibited,
            };

            foreach (var kv in vrm0.ObjectItems())
            {
                var key = kv.Key.GetString();
                switch (key)
                {
                    case "title": meta.Name = kv.Value.GetString(); break;
                    case "version": meta.Version = kv.Value.GetString(); break;
                    case "author": meta.Authors = new List<string>() { kv.Value.GetString() }; break;
                    case "contactInformation": meta.ContactInformation = kv.Value.GetString(); break;
                    case "reference": meta.References = new List<string>() { kv.Value.GetString() }; break;
                    case "texture": meta.ThumbnailImage = kv.Value.GetInt32(); break;

                    case "allowedUserName":
                        {
                            var allowdUserType = kv.Value.GetString();
                            switch (allowdUserType)
                            {
                                case "Everyone": meta.AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.everyone; break;
                                default: throw new NotImplementedException($"allowedUser: {allowdUserType}");
                            }
                        }
                        break;

                    case "violentUssageName": meta.AllowExcessivelyViolentUsage = kv.Value.GetString().ToLower() == "allow"; break;
                    case "sexualUssageName": meta.AllowExcessivelySexualUsage = kv.Value.GetString().ToLower() == "allow"; break;
                    case "commercialUssageName":
                        {
                            var commercialUssageType = kv.Value.GetString();
                            switch (commercialUssageType)
                            {
                                case "Allow": meta.CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalProfit; break;
                                default: meta.CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit; break;
                            }
                        }
                        break;

                    case "otherPermissionUrl":
                        {
                            // TODO
                            // var url = kv.Value.GetString();
                            // if (!String.IsNullOrWhiteSpace(url))
                            // {
                            //     throw new NotImplementedException("otherPermissionUrl not allowd");
                            // }
                        }
                        break;

                    case "otherLicenseUrl": meta.OtherLicenseUrl = kv.Value.GetString(); break;

                    case "licenseName":
                        {
                            // TODO
                            // CreditNotation = CreditNotationType.required,
                        }
                        break;

                    default:
                        throw new NotImplementedException(key);
                }
            }

            return meta;
        }

        static string GetLicenseUrl(ListTreeNode<JsonValue> vrm0)
        {
            string l0 = default;
            string l1 = default;
            foreach (var kv in vrm0.ObjectItems())
            {
                switch (kv.Key.GetString())
                {
                    case "otherLicenseUrl":
                        l0 = kv.Value.GetString();
                        break;

                    case "otherPermissionUrl":
                        l1 = kv.Value.GetString();
                        break;
                }
            }
            if (!string.IsNullOrWhiteSpace(l0))
            {
                return l0;
            }
            if (!string.IsNullOrWhiteSpace(l1))
            {
                return l1;
            }
            return "";
        }

        static float[] ReverseZ(ListTreeNode<JsonValue> xyz)
        {
            return new float[]{
                xyz["x"].GetSingle(),
                xyz["y"].GetSingle(),
                -xyz["z"].GetSingle(),
            };
        }

        static IEnumerable<UniGLTF.glTFNode> EnumJoint(List<UniGLTF.glTFNode> nodes, UniGLTF.glTFNode node)
        {
            yield return node;

            if (node.children != null && node.children.Length > 0)
            {
                foreach (var x in EnumJoint(nodes, nodes[node.children[0]]))
                {
                    yield return x;
                }
            }
        }

        static UniGLTF.Extensions.VRMC_springBone.VRMC_springBone MigrateSpringBone(UniGLTF.glTF gltf, ListTreeNode<JsonValue> sa)
        {
            var colliderNodes = new List<int>();

            foreach (var x in sa["colliderGroups"].ArrayItems())
            {
                var node = x["node"].GetInt32();
                colliderNodes.Add(node);
                var gltfNode = gltf.nodes[node];

                var collider = new UniGLTF.Extensions.VRMC_node_collider.VRMC_node_collider()
                {
                    Shapes = new List<UniGLTF.Extensions.VRMC_node_collider.ColliderShape>(),
                };

                // {
                //   "node": 14,
                //   "colliders": [
                //     {
                //       "offset": {
                //         "x": 0.025884293,
                //         "y": -0.120000005,
                //         "z": 0
                //       },
                //       "radius": 0.05
                //     },
                //     {
                //       "offset": {
                //         "x": -0.02588429,
                //         "y": -0.120000005,
                //         "z": 0
                //       },
                //       "radius": 0.05
                //     },
                //     {
                //       "offset": {
                //         "x": 0,
                //         "y": -0.0220816135,
                //         "z": 0
                //       },
                //       "radius": 0.08
                //     }
                //   ]
                // },
                foreach (var y in x["colliders"].ArrayItems())
                {
                    collider.Shapes.Add(new UniGLTF.Extensions.VRMC_node_collider.ColliderShape
                    {
                        Sphere = new UniGLTF.Extensions.VRMC_node_collider.ColliderShapeSphere
                        {
                            Offset = ReverseZ(y["offset"]),
                            Radius = y["radius"].GetSingle()
                        }
                    });
                }

                if (!(gltfNode.extensions is UniGLTF.glTFExtensionExport extensions))
                {
                    extensions = new UniGLTF.glTFExtensionExport();
                    gltfNode.extensions = extensions;
                }

                var f = new JsonFormatter();
                UniGLTF.Extensions.VRMC_node_collider.GltfSerializer.Serialize(f, collider);
                extensions.Add(UniGLTF.Extensions.VRMC_node_collider.VRMC_node_collider.ExtensionName, f.GetStoreBytes());
            }

            var springBone = new UniGLTF.Extensions.VRMC_springBone.VRMC_springBone
            {
                Springs = new List<UniGLTF.Extensions.VRMC_springBone.Spring>(),
            };
            foreach (var x in sa["boneGroups"].ArrayItems())
            {
                // {
                //   "comment": "",
                //   "stiffiness": 2,
                //   "gravityPower": 0,
                //   "gravityDir": {
                //     "x": 0,
                //     "y": -1,
                //     "z": 0
                //   },
                //   "dragForce": 0.7,
                //   "center": -1,
                //   "hitRadius": 0.02,
                //   "bones": [
                //     97,
                //     99,
                //     101,
                //     113,
                //     114
                //   ],
                //   "colliderGroups": [
                //     3,
                //     4,
                //     5
                //   ]
                // },
                foreach (var y in x["bones"].ArrayItems())
                {
                    var spring = new UniGLTF.Extensions.VRMC_springBone.Spring
                    {
                        Name = x["comment"].GetString(),
                        Colliders = x["colliderGroups"].ArrayItems().Select(z => colliderNodes[z.GetInt32()]).ToArray(),
                        Joints = new List<UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint>(),
                    };

                    foreach (var z in EnumJoint(gltf.nodes, gltf.nodes[y.GetInt32()]))
                    {
                        spring.Joints.Add(new UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint
                        {
                            Node = gltf.nodes.IndexOf(z),
                            DragForce = x["dragForce"].GetSingle(),
                            GravityDir = ReverseZ(x["gravityDir"]),
                            GravityPower = x["gravityPower"].GetSingle(),
                            HitRadius = x["hitRadius"].GetSingle(),
                            Stiffness = x["stiffiness"].GetSingle(),
                        });
                    }

                    springBone.Springs.Add(spring);
                }
            }

            return springBone;
        }

        public static byte[] Migrate(byte[] src)
        {
            var glb = UniGLTF.Glb.Parse(src);
            var json = glb.Json.Bytes.ParseAsJson();
            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);

            var extensions = new UniGLTF.glTFExtensionExport();
            {
                var vrm0 = json["extensions"]["VRM"];

                {
                    // vrm
                    var vrm1 = new UniGLTF.Extensions.VRMC_vrm.VRMC_vrm();
                    vrm1.Meta = MigrateMeta(vrm0["meta"]);
                    vrm1.Humanoid = MigrateHumanoid(vrm0["humanoid"]);

                    var f = new JsonFormatter();
                    UniGLTF.Extensions.VRMC_vrm.GltfSerializer.Serialize(f, vrm1);
                    extensions.Add(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionName, f.GetStoreBytes());
                }
                {
                    // springBone & collider
                    var vrm1 = MigrateSpringBone(gltf, json["extensions"]["VRM"]["secondaryAnimation"]);

                    var f = new JsonFormatter();
                    UniGLTF.Extensions.VRMC_springBone.GltfSerializer.Serialize(f, vrm1);
                    extensions.Add(UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionName, f.GetStoreBytes());
                }
                {
                    // MToon
                }
                {
                    // constraint
                }
            }

            ArraySegment<byte> vrm1Json = default;
            {
                gltf.extensions = extensions;

                var f = new JsonFormatter();
                UniGLTF.GltfSerializer.Serialize(f, gltf);
                vrm1Json = f.GetStoreBytes();
            }

            return UniGLTF.Glb.Create(vrm1Json, glb.Binary.Bytes).ToBytes();
        }

        #region for UnitTest
        public class MigrationException : Exception
        {
            public MigrationException(string key, string value) : base($"{key}: {value}")
            {
            }
        }

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
                    case "leftThumbProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbProximal); break;
                    case "leftThumbIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbIntermediate); break;
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
                    case "rightThumbProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbProximal); break;
                    case "rightThumbIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbIntermediate); break;
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
                    default: throw new MigrationException("humanonoid.humanBones[*].bone", boneType);
                }
            }
        }

        static bool IsSingleList(string key, string lhs, List<string> rhs)
        {
            if (rhs.Count != 1) throw new MigrationException(key, $"{rhs.Count}");
            return lhs == rhs[0];
        }

        static string AvatarPermission(string key, UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType x)
        {
            switch (x)
            {
                case UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.everyone: return "Everyone";
                    // case AvatarPermissionType.onlyAuthor: return "OnlyAuthor";
                    // case AvatarPermissionType.explicitlyLicensedPerson: return "Explicited";
            }
            throw new MigrationException(key, $"{x}");
        }

        public static void CheckMeta(ListTreeNode<JsonValue> vrm0, UniGLTF.Extensions.VRMC_vrm.Meta vrm1)
        {
            if (vrm0["title"].GetString() != vrm1.Name) throw new MigrationException("meta.title", vrm1.Name);
            if (vrm0["version"].GetString() != vrm1.Version) throw new MigrationException("meta.version", vrm1.Version);
            if (!IsSingleList("meta.author", vrm0["author"].GetString(), vrm1.Authors)) throw new MigrationException("meta.author", $"{vrm1.Authors}");
            if (vrm0["contactInformation"].GetString() != vrm1.ContactInformation) throw new MigrationException("meta.contactInformation", vrm1.ContactInformation);
            if (!IsSingleList("meta.reference", vrm0["reference"].GetString(), vrm1.References)) throw new MigrationException("meta.reference", $"{vrm1.References}");
            if (vrm0["texture"].GetInt32() != vrm1.ThumbnailImage) throw new MigrationException("meta.texture", $"{vrm1.ThumbnailImage}");

            if (vrm0["allowedUserName"].GetString() != AvatarPermission("meta.allowedUserName", vrm1.AvatarPermission)) throw new MigrationException("meta.allowedUserName", $"{vrm1.AvatarPermission}");
            if (vrm0["violentUssageName"].GetString() == "Allow" != vrm1.AllowExcessivelyViolentUsage) throw new MigrationException("meta.violentUssageName", $"{vrm1.AllowExcessivelyViolentUsage}");
            if (vrm0["sexualUssageName"].GetString() == "Allow" != vrm1.AllowExcessivelySexualUsage) throw new MigrationException("meta.sexualUssageName", $"{vrm1.AllowExcessivelyViolentUsage}");

            if (vrm0["commercialUssageName"].GetString() == "Allow")
            {
                if (vrm1.CommercialUsage == UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit)
                {
                    throw new MigrationException("meta.commercialUssageName", $"{vrm1.CommercialUsage}");
                }
            }
            else
            {
                if (vrm1.CommercialUsage == UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.corporation
                || vrm1.CommercialUsage == UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalProfit)
                {
                    throw new MigrationException("meta.commercialUssageName", $"{vrm1.CommercialUsage}");
                }
            }

            if (GetLicenseUrl(vrm0) != vrm1.OtherLicenseUrl) throw new MigrationException("meta.otherLicenseUrl", vrm1.OtherLicenseUrl);

            switch (vrm0["licenseName"].GetString())
            {
                case "Other":
                    {
                        if (vrm1.Modification != UniGLTF.Extensions.VRMC_vrm.ModificationType.prohibited) throw new MigrationException("meta.licenceName", $"{vrm1.Modification}");
                        if (vrm1.AllowRedistribution.Value) throw new MigrationException("meta.liceneName", $"{vrm1.Modification}");
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public static void Check(ListTreeNode<JsonValue> vrm0, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm1)
        {
            Migration.CheckMeta(vrm0["meta"], vrm1.Meta);
            Migration.CheckHumanoid(vrm0["humanoid"], vrm1.Humanoid);
        }

        public static void Check(ListTreeNode<JsonValue> vrm0, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone vrm1, List<UniGLTF.glTFNode> nodes)
        {
            var a = 0;
            // Migration.CheckSpringBone(vrm0["secondaryAnimation"], vrm1.sp)
        }
        #endregion
    }
}
