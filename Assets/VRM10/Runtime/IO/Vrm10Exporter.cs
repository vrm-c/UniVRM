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
    }
}
