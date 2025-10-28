using UnityEngine;
using UnityEngine.Rendering;

namespace UniVRM10
{
    internal static class MeshVertexUtility
    {
        public static void SetVertexBufferParamsToMesh(Mesh mesh, int length)
        {
            mesh.SetVertexBufferParams(length, new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 0),
                new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4, stream: 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 1),
                new VertexAttributeDescriptor(VertexAttribute.BlendWeight, dimension: 4, stream: 2),
                new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt16, 4, 2),
            });
        }
    }
}