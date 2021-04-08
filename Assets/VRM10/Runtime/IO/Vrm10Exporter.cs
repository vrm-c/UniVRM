using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using VrmLib;
using VRMShaders;


namespace UniVRM10
{
    public class Vrm10Exporter
    {
        public readonly Vrm10Storage Storage = new Vrm10Storage();

        public readonly string VrmExtensionName = "VRMC_vrm";

        public Vrm10Exporter()
        {
            Storage.Gltf.extensionsUsed.Add(glTF_KHR_materials_unlit.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(glTF_KHR_texture_transform.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_node_collider.VRMC_node_collider.ExtensionName);
            Storage.Gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_constraints.VRMC_constraints.ExtensionName);
            Storage.Gltf.buffers.Add(new glTFBuffer
            {

            });
        }

        public void ExportAsset(Model model)
        {
            Storage.Gltf.asset = new glTFAssets
            {
            };
            if (!string.IsNullOrEmpty(model.AssetVersion)) Storage.Gltf.asset.version = model.AssetVersion;
            if (!string.IsNullOrEmpty(model.AssetMinVersion)) Storage.Gltf.asset.minVersion = model.AssetMinVersion;

            if (!string.IsNullOrEmpty(model.AssetGenerator)) Storage.Gltf.asset.generator = model.AssetGenerator;

            if (!string.IsNullOrEmpty(model.AssetCopyright)) Storage.Gltf.asset.copyright = model.AssetCopyright;
        }

        public void Reserve(int bytesLength)
        {
            Storage.Reserve(bytesLength);
        }

        public void ExportMeshes(List<MeshGroup> groups, List<object> materials, ExportArgs option)
        {
            foreach (var group in groups)
            {
                var mesh = group.ExportMeshGroup(materials, Storage, option);
                Storage.Gltf.meshes.Add(mesh);
            }
        }

        public void ExportNodes(Node root, List<Node> nodes, List<MeshGroup> groups, ExportArgs option)
        {
            foreach (var x in nodes)
            {
                var node = new glTFNode
                {
                    name = x.Name,
                };

                node.translation = x.LocalTranslation.ToFloat3();
                node.rotation = x.LocalRotation.ToFloat4();
                node.scale = x.LocalScaling.ToFloat3();

                if (x.MeshGroup != null)
                {
                    node.mesh = groups.IndexOfThrow(x.MeshGroup);
                    var skin = x.MeshGroup.Skin;
                    if (skin != null)
                    {
                        var skinIndex = Storage.Gltf.skins.Count;
                        var gltfSkin = new glTFSkin()
                        {
                            joints = skin.Joints.Select(joint => nodes.IndexOfThrow(joint)).ToArray()
                        };
                        if (skin.InverseMatrices == null)
                        {
                            skin.CalcInverseMatrices();
                        }
                        if (skin.InverseMatrices != null)
                        {
                            gltfSkin.inverseBindMatrices = skin.InverseMatrices.AddAccessorTo(Storage, 0, option.sparse);
                        }
                        if (skin.Root != null)
                        {
                            gltfSkin.skeleton = nodes.IndexOf(skin.Root);
                        }
                        Storage.Gltf.skins.Add(gltfSkin);
                        node.skin = skinIndex;
                    }
                }

                node.children = x.Children.Select(child => nodes.IndexOfThrow(child)).ToArray();

                Storage.Gltf.nodes.Add(node);
            }

            Storage.Gltf.scenes.Add(new gltfScene()
            {
                nodes = root.Children.Select(child => nodes.IndexOfThrow(child)).ToArray()
            });
        }

        public void Export(Model m, ExportArgs option, Func<Texture, bool> useAsset)
        {
            ExportAsset(m);

            ///
            /// 必要な容量を先に確保
            /// (sparseは考慮してないので大きめ)
            ///
            {
                var reserveBytes = 0;
                // mesh
                foreach (var g in m.MeshGroups)
                {
                    foreach (var mesh in g.Meshes)
                    {
                        // 頂点バッファ
                        reserveBytes += mesh.IndexBuffer.ByteLength;
                        foreach (var kv in mesh.VertexBuffer)
                        {
                            reserveBytes += kv.Value.ByteLength;
                        }
                        // morph
                        foreach (var morph in mesh.MorphTargets)
                        {
                            foreach (var kv in morph.VertexBuffer)
                            {
                                reserveBytes += kv.Value.ByteLength;
                            }
                        }
                    }
                }
                Reserve(reserveBytes);
            }

            // mesh
            ExportMeshes(m.MeshGroups, m.Materials, option);

            // node
            ExportNodes(m.Root, m.Nodes, m.MeshGroups, option);

            // material
            var textureExporter = new TextureExporter(useAsset);
            var materialExporter = new Vrm10MaterialExporter();
            foreach (Material material in m.Materials)
            {
                var glTFMaterial = materialExporter.ExportMaterial(material, textureExporter);
                Storage.Gltf.materials.Add(glTFMaterial);
            }

            // Extension で Texture が増える場合があるので最後に呼ぶ
            for (int i = 0; i < textureExporter.Exported.Count; ++i)
            {
                var unityTexture = textureExporter.Exported[i];
                Storage.Gltf.PushGltfTexture(0, unityTexture);
            }

            ExportVrm(m);
        }

        void ExportVrm(Model model)
        {
            var vrm = new UniGLTF.Extensions.VRMC_vrm.VRMC_vrm
            {
                Humanoid = new UniGLTF.Extensions.VRMC_vrm.Humanoid
                {
                    HumanBones = new UniGLTF.Extensions.VRMC_vrm.HumanBones
                    {
                    },
                },
                Meta = new UniGLTF.Extensions.VRMC_vrm.Meta
                {
                    AllowExcessivelySexualUsage = false,
                    AllowExcessivelyViolentUsage = false,
                    AllowPoliticalOrReligiousUsage = false,
                    AllowRedistribution = false,
                },
            };

            ExportHumanoid(vrm, model);

            // meta

            // expression

            // lookAt

            // firstPerson

            UniGLTF.Extensions.VRMC_vrm.GltfSerializer.SerializeTo(ref Storage.Gltf.extensions, vrm);
        }

        void ExportHumanoid(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, Model model)
        {
            // humanoid
            for (int i = 0; i < model.Nodes.Count; ++i)
            {
                var bone = model.Nodes[i];
                switch (bone.HumanoidBone)
                {
                    case HumanoidBones.hips: vrm.Humanoid.HumanBones.Hips = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                    case HumanoidBones.spine: vrm.Humanoid.HumanBones.Spine = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.chest: vrm.Humanoid.HumanBones.Chest = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.upperChest: vrm.Humanoid.HumanBones.UpperChest = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.neck: vrm.Humanoid.HumanBones.Neck = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.head: vrm.Humanoid.HumanBones.Head = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftEye: vrm.Humanoid.HumanBones.LeftEye = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightEye: vrm.Humanoid.HumanBones.RightEye = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.jaw: vrm.Humanoid.HumanBones.Jaw = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftUpperLeg: vrm.Humanoid.HumanBones.LeftUpperLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftLowerLeg: vrm.Humanoid.HumanBones.LeftLowerLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftFoot: vrm.Humanoid.HumanBones.LeftFoot = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftToes: vrm.Humanoid.HumanBones.LeftToes = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightUpperLeg: vrm.Humanoid.HumanBones.RightUpperLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightLowerLeg: vrm.Humanoid.HumanBones.RightLowerLeg = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightFoot: vrm.Humanoid.HumanBones.RightFoot = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightToes: vrm.Humanoid.HumanBones.RightToes = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftShoulder: vrm.Humanoid.HumanBones.LeftShoulder = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftUpperArm: vrm.Humanoid.HumanBones.LeftUpperArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftLowerArm: vrm.Humanoid.HumanBones.LeftLowerArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftHand: vrm.Humanoid.HumanBones.LeftHand = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightShoulder: vrm.Humanoid.HumanBones.RightShoulder = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightUpperArm: vrm.Humanoid.HumanBones.RightUpperArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightLowerArm: vrm.Humanoid.HumanBones.RightLowerArm = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightHand: vrm.Humanoid.HumanBones.RightHand = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftThumbProximal: vrm.Humanoid.HumanBones.LeftThumbProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftThumbIntermediate: vrm.Humanoid.HumanBones.LeftThumbIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftThumbDistal: vrm.Humanoid.HumanBones.LeftThumbDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftIndexProximal: vrm.Humanoid.HumanBones.LeftIndexProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftIndexIntermediate: vrm.Humanoid.HumanBones.LeftIndexIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftIndexDistal: vrm.Humanoid.HumanBones.LeftIndexDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftMiddleProximal: vrm.Humanoid.HumanBones.LeftMiddleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftMiddleIntermediate: vrm.Humanoid.HumanBones.LeftMiddleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftMiddleDistal: vrm.Humanoid.HumanBones.LeftMiddleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftRingProximal: vrm.Humanoid.HumanBones.LeftRingProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftRingIntermediate: vrm.Humanoid.HumanBones.LeftRingIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftRingDistal: vrm.Humanoid.HumanBones.LeftRingDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftLittleProximal: vrm.Humanoid.HumanBones.LeftLittleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftLittleIntermediate: vrm.Humanoid.HumanBones.LeftLittleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.leftLittleDistal: vrm.Humanoid.HumanBones.LeftLittleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightThumbProximal: vrm.Humanoid.HumanBones.RightThumbProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightThumbIntermediate: vrm.Humanoid.HumanBones.RightThumbIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightThumbDistal: vrm.Humanoid.HumanBones.RightThumbDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightIndexProximal: vrm.Humanoid.HumanBones.RightIndexProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightIndexIntermediate: vrm.Humanoid.HumanBones.RightIndexIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightIndexDistal: vrm.Humanoid.HumanBones.RightIndexDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightMiddleProximal: vrm.Humanoid.HumanBones.RightMiddleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightMiddleIntermediate: vrm.Humanoid.HumanBones.RightMiddleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightMiddleDistal: vrm.Humanoid.HumanBones.RightMiddleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightRingProximal: vrm.Humanoid.HumanBones.RightRingProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightRingIntermediate: vrm.Humanoid.HumanBones.RightRingIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightRingDistal: vrm.Humanoid.HumanBones.RightRingDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightLittleProximal: vrm.Humanoid.HumanBones.RightLittleProximal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightLittleIntermediate: vrm.Humanoid.HumanBones.RightLittleIntermediate = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;

                    case HumanoidBones.rightLittleDistal: vrm.Humanoid.HumanBones.RightLittleDistal = new UniGLTF.Extensions.VRMC_vrm.HumanBone { Node = i }; break;
                }
            }
        }
    }
}
