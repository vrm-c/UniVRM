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

            // mesh
            exporter.ExportMeshes(m.MeshGroups, m.Materials, option);

            // node
            exporter.ExportNodes(m.Root, m.Nodes, m.MeshGroups, option);

            return exporter.ToBytes();
        }
    }
}
