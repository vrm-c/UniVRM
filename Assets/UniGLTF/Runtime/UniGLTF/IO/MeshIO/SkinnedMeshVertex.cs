using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// インターリーブされたメッシュの頂点情報のうち、SkinnedMeshに関連した情報を表す構造体
    /// そのままGPUにアップロードされる
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal readonly struct SkinnedMeshVertex
    {
        private readonly float _boneWeight0;
        private readonly float _boneWeight1;
        private readonly float _boneWeight2;
        private readonly float _boneWeight3;
        private readonly ushort _boneIndex0;
        private readonly ushort _boneIndex1;
        private readonly ushort _boneIndex2;
        private readonly ushort _boneIndex3;

        public SkinnedMeshVertex(
            ushort boneIndex0,
            ushort boneIndex1,
            ushort boneIndex2,
            ushort boneIndex3,
            float boneWeight0,
            float boneWeight1,
            float boneWeight2,
            float boneWeight3)
        {
            _boneIndex0 = boneIndex0;
            _boneIndex1 = boneIndex1;
            _boneIndex2 = boneIndex2;
            _boneIndex3 = boneIndex3;
            _boneWeight0 = boneWeight0;
            _boneWeight1 = boneWeight1;
            _boneWeight2 = boneWeight2;
            _boneWeight3 = boneWeight3;
        }

        public static VertexAttributeDescriptor[] GetVertexAttributeDescriptor() => new[] {
                new VertexAttributeDescriptor(VertexAttribute.BlendWeight, dimension: 4),
                new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt16, 4),
            };
    }
}