using System;
using System.Collections.Generic;
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

        public void Export(Model m, ExportArgs option)
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
        }
    }
}
