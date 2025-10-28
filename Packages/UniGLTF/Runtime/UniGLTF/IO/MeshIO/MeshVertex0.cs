using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Stream0用のインターリーブされたメッシュの頂点情報
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