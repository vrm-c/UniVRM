using System;
using VrmLib;

namespace UniVRM10
{
    public static class IExporterExtensions
    {
        public static byte[] Export(this Vrm10Exporter exporter, Model m, ExportArgs option)
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

            return exporter.ToBytes();
        }
    }
}
