using System.Runtime.InteropServices;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniVRM10
{
    /// <summary>
    /// インターリーブされたメッシュの頂点情報を表す構造体
    /// そのままGPUにアップロードされる
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct MeshVertex
    {
        private readonly Vector3 _position;
        private readonly Vector3 _normal;
        private readonly Color _color;
        private readonly Vector2 _texCoord;
        private readonly float _boneWeight0;
        private readonly float _boneWeight1;
        private readonly float _boneWeight2;
        private readonly float _boneWeight3;
        private readonly ushort _boneIndex0;
        private readonly ushort _boneIndex1;
        private readonly ushort _boneIndex2;
        private readonly ushort _boneIndex3;

        public MeshVertex(
            Vector3 position,
            Vector3 normal,
            Vector2 texCoord,
            Color color,
            ushort boneIndex0,
            ushort boneIndex1,
            ushort boneIndex2,
            ushort boneIndex3,
            float boneWeight0,
            float boneWeight1,
            float boneWeight2,
            float boneWeight3)
        {
            _position = position;
            _normal = normal;
            _texCoord = texCoord;
            _color = color;
            _boneIndex0 = boneIndex0;
            _boneIndex1 = boneIndex1;
            _boneIndex2 = boneIndex2;
            _boneIndex3 = boneIndex3;
            _boneWeight0 = boneWeight0;
            _boneWeight1 = boneWeight1;
            _boneWeight2 = boneWeight2;
            _boneWeight3 = boneWeight3;
        }

        private static readonly VertexAttributeDescriptor[] vertexAttributeDescriptor = {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal),
            new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2),
            new VertexAttributeDescriptor(VertexAttribute.BlendWeight, dimension: 4),
            new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt16, 4),
        };

        public static void SetVertexBufferParamsToMesh(Mesh mesh, int length)
        {
            mesh.SetVertexBufferParams(length, vertexAttributeDescriptor);
        }
    }
}