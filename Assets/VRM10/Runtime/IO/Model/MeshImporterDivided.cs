using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    public static class MeshImporterDivided
    {
        public static UnityEngine.Mesh LoadDivided(VrmLib.MeshGroup src)
        {
            var vertexCount = src.Meshes.Sum(x => x.VertexBuffer.Count);
            var positions = new List<Vector3>(vertexCount);
            var normals = new List<Vector3>(vertexCount);
            var uv = new List<Vector2>(vertexCount);
            var offset = 0;
            foreach (var mesh in src.Meshes)
            {
                var submesh = mesh.Submeshes[0];
                var end = offset + submesh.DrawCount;
                {
                    positions.AddRange(mesh.VertexBuffer.Positions.GetSpan<Vector3>());
                    normals.AddRange(mesh.VertexBuffer.Normals.GetSpan<Vector3>());
                    uv.AddRange(mesh.VertexBuffer.TexCoords.GetSpan<Vector2>());
                }
                offset = end;
            }

            var dst = new UnityEngine.Mesh();
            dst.name = src.Name;
            dst.vertices = positions.ToArray();
            dst.normals = normals.ToArray();
            dst.uv = uv.ToArray();
            return dst;
        }
    }
}
