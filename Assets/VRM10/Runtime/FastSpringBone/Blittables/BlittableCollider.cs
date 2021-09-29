using UnityEngine;

namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// VRMSpringBoneのSphereColliderをBlittableにしたもの
    /// 位置情報は親であるColliderGroupが持つ
    /// </summary>
    public readonly struct BlittableCollider
    {
        public Vector3 Offset { get; }
        public float Radius { get; }
        
        public BlittableCollider(Vector3 offset, float radius)
        {
            Offset = offset;
            Radius = radius;
        }
    }
}