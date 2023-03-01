using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Stream1用のインターリーブされたメッシュの頂点情報
    /// そのままGPUにアップロードされる
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal readonly struct MeshVertex1
    {
        private readonly Color _color;
        private readonly Vector2 _texCoord0;
        private readonly Vector2 _texCoord1;

        public MeshVertex1(
            Vector2 texCoord0,
            Vector2 texCoord1,
            Color color)
        {
            _texCoord0 = texCoord0;
            _texCoord1 = texCoord1;
            _color = color;
        }
    }
}