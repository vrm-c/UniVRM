using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UniJSON;
using VrmLib;

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

        public byte[] ToBytes()
        {
            Storage.Gltf.buffers[0].byteLength = Storage.Buffers[0].Bytes.Count;

            var f = new JsonFormatter();
            UniGLTF.GltfSerializer.Serialize(f, Storage.Gltf);
            var json = f.GetStoreBytes();

            var glb = UniGLTF.Glb.Create(json, Storage.Buffers[0].Bytes);
            return glb.ToBytes();
        }

        public void ExportAsset(Model model)
        {
            Storage.Gltf.asset = new glTFAssets
            {
            };
            if (!string.IsNullOrEmpty(model.AssetVersion)) Storage.Gltf.asset.version = model.AssetVersion;
            if (!string.IsNullOrEmpty(model.AssetMinVersion)) Storage.Gltf.asset.minVersion = model.AssetMinVersion;

            if (!string.IsNullOrEmpty(model.AssetGenerator)) Storage.Gltf.asset.generator = model.AssetGenerator;
            if (model.Vrm != null && !string.IsNullOrEmpty(model.Vrm.ExporterVersion))
            {
                Storage.Gltf.asset.generator = model.Vrm.ExporterVersion;
            }

            if (!string.IsNullOrEmpty(model.AssetCopyright)) Storage.Gltf.asset.copyright = model.AssetCopyright;
        }

        public void Reserve(int bytesLength)
        {
            Storage.Reserve(bytesLength);
        }

        public void ExportImageAndTextures(List<Image> images, List<Texture> textures)
        {
            foreach (var x in images)
            {
                Storage.Gltf.images.Add(x.ToGltf(Storage));
            }
            foreach (var x in textures)
            {
                if (x is ImageTexture imageTexture)
                {
                    var samplerIndex = Storage.Gltf.samplers.Count;
                    Storage.Gltf.samplers.Add(x.Sampler.ToGltf());
                    Storage.Gltf.textures.Add(new glTFTexture
                    {
                        name = x.Name,
                        source = images.IndexOfThrow(imageTexture.Image),
                        sampler = samplerIndex,
                        // extensions
                        // = imageTexture.Image.MimeType.Equals("image/webp") ? new GltfTextureExtensions() { EXT_texture_webp = new EXT_texture_webp() { source = images.IndexOf(imageTexture.Image) } }
                        // : imageTexture.Image.MimeType.Equals("image/vnd-ms.dds") ? new GltfTextureExtensions() { MSFT_texture_dds = new MSFT_texture_dds() { source = images.IndexOf(imageTexture.Image) } }
                        // : null
                    });
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void ExportMaterialPBR(Material src, PBRMaterial pbr, List<Texture> textures)
        {
            var material = pbr.PBRToGltf(textures);
            Storage.Gltf.materials.Add(material);
        }

        public void ExportMaterialUnlit(Material src, UnlitMaterial unlit, List<Texture> textures)
        {
            var material = unlit.UnlitToGltf(textures);
            Storage.Gltf.materials.Add(material);
            if (!Storage.Gltf.extensionsUsed.Contains(UnlitMaterial.ExtensionName))
            {
                Storage.Gltf.extensionsUsed.Add(UnlitMaterial.ExtensionName);
            }
        }

        public void ExportMaterialMToon(Material src, MToonMaterial mtoon, List<Texture> textures)
        {
            if (!Storage.Gltf.extensionsUsed.Contains(UnlitMaterial.ExtensionName))
            {
                Storage.Gltf.extensionsUsed.Add(UnlitMaterial.ExtensionName);
            }

            var material = mtoon.MToonToGltf(textures);
            Storage.Gltf.materials.Add(material);
            if (!Storage.Gltf.extensionsUsed.Contains(MToonMaterial.ExtensionName))
            {
                Storage.Gltf.extensionsUsed.Add(MToonMaterial.ExtensionName);
            }
        }

        public void ExportMeshes(List<MeshGroup> groups, List<Material> materials, ExportArgs option)
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

        public void ExportAnimations(List<Animation> animations, List<Node> nodes, ExportArgs option)
        {
            // throw new System.NotImplementedException();
        }

        public void ExportVrmMeta(Vrm src, List<Texture> textures)
        {
            if (!Storage.Gltf.extensionsUsed.Contains(VrmExtensionName))
            {
                Storage.Gltf.extensionsUsed.Add(VrmExtensionName);
            }

            if (Storage.gltfVrm == null)
            {
                Storage.gltfVrm = new UniGLTF.Extensions.VRMC_vrm.VRMC_vrm();
            }

            Storage.gltfVrm.SpecVersion = src.SpecVersion;
            Storage.gltfVrm.Meta = src.Meta.ToGltf(textures);
        }

        public void ExportVrmHumanoid(Dictionary<HumanoidBones, Node> map, List<Node> nodes)
        {
            Storage.gltfVrm.Humanoid = new UniGLTF.Extensions.VRMC_vrm.Humanoid()
            {
                HumanBones = new UniGLTF.Extensions.VRMC_vrm.HumanBones(),
            };
            foreach (var kv in map.OrderBy(kv => kv.Key))
            {
                var humanoidBone = new UniGLTF.Extensions.VRMC_vrm.HumanBone
                {
                    Node = nodes.IndexOfThrow(kv.Value),
                };

                switch (kv.Key)
                {
                    case HumanoidBones.hips: Storage.gltfVrm.Humanoid.HumanBones.Hips = humanoidBone; break;
                    case HumanoidBones.leftUpperLeg: Storage.gltfVrm.Humanoid.HumanBones.LeftUpperLeg = humanoidBone; break;
                    case HumanoidBones.rightUpperLeg: Storage.gltfVrm.Humanoid.HumanBones.RightUpperLeg = humanoidBone; break;
                    case HumanoidBones.leftLowerLeg: Storage.gltfVrm.Humanoid.HumanBones.LeftLowerLeg = humanoidBone; break;
                    case HumanoidBones.rightLowerLeg: Storage.gltfVrm.Humanoid.HumanBones.RightLowerLeg = humanoidBone; break;
                    case HumanoidBones.leftFoot: Storage.gltfVrm.Humanoid.HumanBones.LeftFoot = humanoidBone; break;
                    case HumanoidBones.rightFoot: Storage.gltfVrm.Humanoid.HumanBones.RightFoot = humanoidBone; break;
                    case HumanoidBones.spine: Storage.gltfVrm.Humanoid.HumanBones.Spine = humanoidBone; break;
                    case HumanoidBones.chest: Storage.gltfVrm.Humanoid.HumanBones.Chest = humanoidBone; break;
                    case HumanoidBones.neck: Storage.gltfVrm.Humanoid.HumanBones.Neck = humanoidBone; break;
                    case HumanoidBones.head: Storage.gltfVrm.Humanoid.HumanBones.Head = humanoidBone; break;
                    case HumanoidBones.leftShoulder: Storage.gltfVrm.Humanoid.HumanBones.LeftShoulder = humanoidBone; break;
                    case HumanoidBones.rightShoulder: Storage.gltfVrm.Humanoid.HumanBones.RightShoulder = humanoidBone; break;
                    case HumanoidBones.leftUpperArm: Storage.gltfVrm.Humanoid.HumanBones.LeftUpperArm = humanoidBone; break;
                    case HumanoidBones.rightUpperArm: Storage.gltfVrm.Humanoid.HumanBones.RightUpperArm = humanoidBone; break;
                    case HumanoidBones.leftLowerArm: Storage.gltfVrm.Humanoid.HumanBones.LeftLowerArm = humanoidBone; break;
                    case HumanoidBones.rightLowerArm: Storage.gltfVrm.Humanoid.HumanBones.RightLowerArm = humanoidBone; break;
                    case HumanoidBones.leftHand: Storage.gltfVrm.Humanoid.HumanBones.LeftHand = humanoidBone; break;
                    case HumanoidBones.rightHand: Storage.gltfVrm.Humanoid.HumanBones.RightHand = humanoidBone; break;
                    case HumanoidBones.leftToes: Storage.gltfVrm.Humanoid.HumanBones.LeftToes = humanoidBone; break;
                    case HumanoidBones.rightToes: Storage.gltfVrm.Humanoid.HumanBones.RightToes = humanoidBone; break;
                    case HumanoidBones.leftEye: Storage.gltfVrm.Humanoid.HumanBones.LeftEye = humanoidBone; break;
                    case HumanoidBones.rightEye: Storage.gltfVrm.Humanoid.HumanBones.RightEye = humanoidBone; break;
                    case HumanoidBones.jaw: Storage.gltfVrm.Humanoid.HumanBones.Jaw = humanoidBone; break;
                    case HumanoidBones.leftThumbProximal: Storage.gltfVrm.Humanoid.HumanBones.LeftThumbProximal = humanoidBone; break;
                    case HumanoidBones.leftThumbIntermediate: Storage.gltfVrm.Humanoid.HumanBones.LeftThumbIntermediate = humanoidBone; break;
                    case HumanoidBones.leftThumbDistal: Storage.gltfVrm.Humanoid.HumanBones.LeftThumbDistal = humanoidBone; break;
                    case HumanoidBones.leftIndexProximal: Storage.gltfVrm.Humanoid.HumanBones.LeftIndexProximal = humanoidBone; break;
                    case HumanoidBones.leftIndexIntermediate: Storage.gltfVrm.Humanoid.HumanBones.LeftIndexIntermediate = humanoidBone; break;
                    case HumanoidBones.leftIndexDistal: Storage.gltfVrm.Humanoid.HumanBones.LeftIndexDistal = humanoidBone; break;
                    case HumanoidBones.leftMiddleProximal: Storage.gltfVrm.Humanoid.HumanBones.LeftMiddleProximal = humanoidBone; break;
                    case HumanoidBones.leftMiddleIntermediate: Storage.gltfVrm.Humanoid.HumanBones.LeftMiddleIntermediate = humanoidBone; break;
                    case HumanoidBones.leftMiddleDistal: Storage.gltfVrm.Humanoid.HumanBones.LeftMiddleDistal = humanoidBone; break;
                    case HumanoidBones.leftRingProximal: Storage.gltfVrm.Humanoid.HumanBones.LeftRingProximal = humanoidBone; break;
                    case HumanoidBones.leftRingIntermediate: Storage.gltfVrm.Humanoid.HumanBones.LeftRingIntermediate = humanoidBone; break;
                    case HumanoidBones.leftRingDistal: Storage.gltfVrm.Humanoid.HumanBones.LeftRingDistal = humanoidBone; break;
                    case HumanoidBones.leftLittleProximal: Storage.gltfVrm.Humanoid.HumanBones.LeftLittleProximal = humanoidBone; break;
                    case HumanoidBones.leftLittleIntermediate: Storage.gltfVrm.Humanoid.HumanBones.LeftLittleIntermediate = humanoidBone; break;
                    case HumanoidBones.leftLittleDistal: Storage.gltfVrm.Humanoid.HumanBones.LeftLittleDistal = humanoidBone; break;
                    case HumanoidBones.rightThumbProximal: Storage.gltfVrm.Humanoid.HumanBones.RightThumbProximal = humanoidBone; break;
                    case HumanoidBones.rightThumbIntermediate: Storage.gltfVrm.Humanoid.HumanBones.RightThumbIntermediate = humanoidBone; break;
                    case HumanoidBones.rightThumbDistal: Storage.gltfVrm.Humanoid.HumanBones.RightThumbDistal = humanoidBone; break;
                    case HumanoidBones.rightIndexProximal: Storage.gltfVrm.Humanoid.HumanBones.RightIndexProximal = humanoidBone; break;
                    case HumanoidBones.rightIndexIntermediate: Storage.gltfVrm.Humanoid.HumanBones.RightIndexIntermediate = humanoidBone; break;
                    case HumanoidBones.rightIndexDistal: Storage.gltfVrm.Humanoid.HumanBones.RightIndexDistal = humanoidBone; break;
                    case HumanoidBones.rightMiddleProximal: Storage.gltfVrm.Humanoid.HumanBones.RightMiddleProximal = humanoidBone; break;
                    case HumanoidBones.rightMiddleIntermediate: Storage.gltfVrm.Humanoid.HumanBones.RightMiddleIntermediate = humanoidBone; break;
                    case HumanoidBones.rightMiddleDistal: Storage.gltfVrm.Humanoid.HumanBones.RightMiddleDistal = humanoidBone; break;
                    case HumanoidBones.rightRingProximal: Storage.gltfVrm.Humanoid.HumanBones.RightRingProximal = humanoidBone; break;
                    case HumanoidBones.rightRingIntermediate: Storage.gltfVrm.Humanoid.HumanBones.RightRingIntermediate = humanoidBone; break;
                    case HumanoidBones.rightRingDistal: Storage.gltfVrm.Humanoid.HumanBones.RightRingDistal = humanoidBone; break;
                    case HumanoidBones.rightLittleProximal: Storage.gltfVrm.Humanoid.HumanBones.RightLittleProximal = humanoidBone; break;
                    case HumanoidBones.rightLittleIntermediate: Storage.gltfVrm.Humanoid.HumanBones.RightLittleIntermediate = humanoidBone; break;
                    case HumanoidBones.rightLittleDistal: Storage.gltfVrm.Humanoid.HumanBones.RightLittleDistal = humanoidBone; break;
                    case HumanoidBones.upperChest: Storage.gltfVrm.Humanoid.HumanBones.UpperChest = humanoidBone; break;
                }

                // gltfVrm.Humanoid.HumanBones.Add(kv.Key.ToString(), humanoidBone);
            }
        }

        public void ExportVrmExpression(ExpressionManager src, List<MeshGroup> _, List<Material> materials, List<Node> nodes)
        {
            if (Storage.gltfVrm.Expressions == null)
            {
                Storage.gltfVrm.Expressions = new List<UniGLTF.Extensions.VRMC_vrm.Expression>();
            }
            foreach (var x in src.ExpressionList)
            {
                Storage.gltfVrm.Expressions.Add(x.ToGltf(nodes, materials));
            }
        }

        public void ExportVrmSpringBone(SpringBoneManager springBone, List<Node> nodes)
        {
            Storage.gltfVrmSpringBone = springBone.ToGltf(nodes, Storage.Gltf.nodes);
        }

        public void ExportVrmFirstPersonAndLookAt(FirstPerson firstPerson, LookAt lookat, List<MeshGroup> meshes, List<Node> nodes)
        {
            Storage.gltfVrm.FirstPerson = firstPerson.ToGltf(nodes);
            Storage.gltfVrm.LookAt = lookat.ToGltf();
        }

        public void ExportVrmMaterialProperties(List<Material> materials, List<Texture> textures)
        {
            // Do nothing
            // see
            // ExportMaterialPBR
            // ExportMaterialUnlit
            // ExportMaterialMToon
        }

        public void ExportVrmEnd()
        {
            UniGLTF.Extensions.VRMC_vrm.GltfSerializer.SerializeTo(ref Storage.Gltf.extensions, Storage.gltfVrm);
            UniGLTF.Extensions.VRMC_springBone.GltfSerializer.SerializeTo(ref Storage.Gltf.extensions, Storage.gltfVrmSpringBone);
        }
    }
}
