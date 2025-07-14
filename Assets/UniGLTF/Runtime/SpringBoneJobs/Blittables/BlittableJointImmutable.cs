using System;
using Unity.Mathematics;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Reconstruct に対して Immutable。
    /// Jointの増減、初期姿勢の変更など構成の変更は Reconstruct が必要。
    /// 変わりにくいスコープ。
    /// </summary>
    [Serializable]
    public readonly struct BlittableJointImmutable
    {
        private readonly float4x3 _data;
        
        public int parentTransformIndex => (int)_data.c2.x;
        public int headTransformIndex => (int)_data.c2.y;
        public int tailTransformIndex => (int)_data.c2.z;
        public float length => _data.c2.w;
        public quaternion localRotation => _data.c0;
        public float3 boneAxis => _data.c1.xyz;

        public BlittableJointImmutable(
            int parentTransformIndex = 0,
            int headTransformIndex = 0,
            int tailTransformIndex = 0,
            float length = 0,
            quaternion localRotation = default,
            float3 boneAxis = default)
        {
            var c0 = localRotation.value;
            var c1 = new float4(boneAxis, 0);
            var c2 = new float4(parentTransformIndex, headTransformIndex, tailTransformIndex, length);
            _data = new float4x3(c0, c1, c2);
        }
        
        public void DrawGizmo(BlittableTransform t, BlittableJointMutable m)
        {
            Gizmos.matrix = t.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Vector3.zero, m.radius);
        }
    }
}