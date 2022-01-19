using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// インターリーブされたメッシュの頂点情報のうち、基本的なものを表す構造体
    /// 必要に応じてSkinnedMeshVertexとあわせて
    /// そのままGPUにアップロードされる
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal readonly struct MeshVertex
    {
        private readonly Vector3 _position;
        private readonly Vector3 _normal;
        private readonly Color _color;
        private readonly Vector2 _texCoord0;
        private readonly Vector2 _texCoord1;

        public MeshVertex(
            Vector3 position,
            Vector3 normal,
            Vector2 texCoord0,
            Vector2 texCoord1,
            Color color)
        {
            _position = position;
            _normal = normal;
            _texCoord0 = texCoord0;
            _texCoord1 = texCoord1;
            _color = color;
        }

        public static VertexAttributeDescriptor[] GetVertexAttributeDescriptor() => new[] {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal),
            new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, dimension: 2)
        };
    }
}