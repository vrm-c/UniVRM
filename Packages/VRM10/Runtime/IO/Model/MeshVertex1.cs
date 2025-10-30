using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Stream1用のインターリーブされたメッシュの頂点情報を表す構造体
    /// そのままGPUにアップロードされる
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal readonly struct MeshVertex1
    {
        private readonly Color _color;
        private readonly Vector2 _texCoord;

        public MeshVertex1(
            Vector2 texCoord,
            Color color)
        {
            _texCoord = texCoord;
            _color = color;
        }
    }
}