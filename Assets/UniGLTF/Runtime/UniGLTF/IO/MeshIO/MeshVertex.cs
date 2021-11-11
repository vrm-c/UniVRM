using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// インターリーブされたメッシュの頂点情報を表す構造体
    /// そのままGPUにアップロードされる
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct MeshVertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Color Color;
        public readonly Vector2 Uv;
        public readonly Vector2 Uv2;
        public readonly float BoneWeight0;
        public readonly float BoneWeight1;
        public readonly float BoneWeight2;
        public readonly float BoneWeight3;
        public readonly ushort BoneIndex0;
        public readonly ushort BoneIndex1;
        public readonly ushort BoneIndex2;
        public readonly ushort BoneIndex3;

        public MeshVertex(
            Vector3 position,
            Vector3 normal,
            Vector2 uv,
            Vector2 uv2,
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
            Position = position;
            Normal = normal;
            Uv = uv;
            Uv2 = uv2;
            Color = color;
            BoneIndex0 = boneIndex0;
            BoneIndex1 = boneIndex1;
            BoneIndex2 = boneIndex2;
            BoneIndex3 = boneIndex3;
            BoneWeight0 = boneWeight0;
            BoneWeight1 = boneWeight1;
            BoneWeight2 = boneWeight2;
            BoneWeight3 = boneWeight3;
        }

        public static VertexAttributeDescriptor[] GetVertexAttributeDescriptor() => new[] {
            new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal),
                new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, dimension: 2),
                new VertexAttributeDescriptor(VertexAttribute.BlendWeight, dimension: 4),
                new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt16, 4),
            };
    }
}