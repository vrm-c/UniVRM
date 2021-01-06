using System;
using System.Collections.Generic;

namespace VrmLib
{
    public interface IVrmExporter
    {
        byte[] ToBytes();

        #region GLTF
        void ExportAsset(Model model);
        void Reserve(int bytesLength);
        void ExportImageAndTextures(List<Image> images, List<Texture> textures);
        void ExportMaterialPBR(Material src, PBRMaterial pbr, List<Texture> textures);
        void ExportMaterialUnlit(Material src, UnlitMaterial unlit, List<Texture> textures);
        void ExportMaterialMToon(Material src, MToonMaterial mtoon, List<Texture> textures);
        void ExportMeshes(List<MeshGroup> groups, List<Material> materials, ExportArgs option);
        void ExportNodes(Node root, List<Node> nodes, List<MeshGroup> groups, ExportArgs option);
        void ExportAnimations(List<Animation> animations, List<Node> nodes, ExportArgs option);
        #endregion

        #region VRM
        void ExportVrmMeta(Vrm src, List<Texture> textures);
        void ExportVrmHumanoid(Dictionary<VrmLib.HumanoidBones, Node> map, List<Node> nodes);
        void ExportVrmMaterialProperties(List<Material> materials, List<Texture> textures);
        void ExportVrmExpression(ExpressionManager expression, List<MeshGroup> meshes, List<Material> materials, List<Node> nodes);
        void ExportVrmSpringBone(SpringBoneManager springBone, List<Node> nodes);
        void ExportVrmFirstPersonAndLookAt(FirstPerson firstPerson, LookAt lookat, List<MeshGroup> meshes, List<Node> nodes);
        void ExportVrmEnd();
        #endregion
    }

    public static class IExporterExtensions
    {
        public static byte[] Export(this IVrmExporter exporter, Model m, ExportArgs option)
        {
            exporter.ExportAsset(m);

            ///
            /// 必要な容量を先に確保
            /// (sparseは考慮してないので大きめ)
            ///
            {
                var reserveBytes = 0;
                // image
                foreach (var image in m.Images)
                {
                    reserveBytes += image.Bytes.Count;
                }
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
                exporter.Reserve(reserveBytes);
            }

            exporter.ExportImageAndTextures(m.Images, m.Textures);

            // material
            foreach (var src in m.Materials)
            {
                if (src is MToonMaterial mtoon)
                {
                    exporter.ExportMaterialMToon(src, mtoon, m.Textures);
                }
                else if (src is UnlitMaterial unlit)
                {
                    exporter.ExportMaterialUnlit(src, unlit, m.Textures);
                }
                else if (src is PBRMaterial pbr)
                {
                    exporter.ExportMaterialPBR(src, pbr, m.Textures);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // mesh
            exporter.ExportMeshes(m.MeshGroups, m.Materials, option);

            // node
            exporter.ExportNodes(m.Root, m.Nodes, m.MeshGroups, option);

            // animation
            exporter.ExportAnimations(m.Animations, m.Nodes, option);

            if (option.vrm)
            {
                ExportVrm(exporter, m);
            }

            return exporter.ToBytes();
        }

        static void ExportVrm(IVrmExporter exporter, Model m)
        {
            if (m.Vrm == null)
            {
                return;
            }

            exporter.ExportVrmMeta(m.Vrm, m.Textures);

            exporter.ExportVrmHumanoid(m.GetBoneMap(), m.Nodes);

            exporter.ExportVrmMaterialProperties(m.Materials, m.Textures);

            exporter.ExportVrmExpression(m.Vrm.ExpressionManager, m.MeshGroups, m.Materials, m.Nodes);

            exporter.ExportVrmSpringBone(m.Vrm.SpringBone, m.Nodes);

            exporter.ExportVrmFirstPersonAndLookAt(m.Vrm.FirstPerson, m.Vrm.LookAt, m.MeshGroups, m.Nodes);

            exporter.ExportVrmEnd();
        }
    }
}
