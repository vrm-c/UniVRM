using System;
using Unity.Mathematics;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Blittableなコライダ
    /// </summary>
    [Serializable]
    public readonly struct BlittableCollider
    {
        private readonly float3x3 _data;

        public BlittableColliderType colliderType => (BlittableColliderType)(int)_data.c2.y;
        public float3 offset => _data.c0;
        public float radius => _data.c2.x;
        // capsule tail or plane normal
        public float3 tailOrNormal => _data.c1;
        public int transformIndex => (int)_data.c2.z;

        public BlittableCollider(
            float3 offset = default,
            float radius = 0,
            float3 tailOrNormal = default,
            BlittableColliderType colliderType = default, 
            int colliderTransformIndex = 0)
        {
            var c0 = offset;
            var c1 = tailOrNormal;
            var c2 = new float3(radius,(int)colliderType, colliderTransformIndex);
            _data = new float3x3(c0, c1, c2);
        }

        public BlittableCollider SetTransformIndex(int index)
        {
            return new BlittableCollider(offset, radius, tailOrNormal, colliderType, index);
        }
        
        public void DrawGizmo(BlittableTransform t)
        {
            Gizmos.matrix = t.localToWorldMatrix;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(offset, radius);
        }
    }
}