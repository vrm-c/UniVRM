using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Stream0用のインターリーブされたメッシュの頂点情報を表す構造体
    /// そのままGPUにアップロードされる
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal readonly struct MeshVertex0
    {
        private readonly Vector3 _position;
        private readonly Vector3 _normal;

        public MeshVertex0(
            Vector3 position,
            Vector3 normal)
        {
            _position = position;
            _normal = normal;
        }
    }
}