using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// Blittableなコライダ
    /// </summary>
    [Serializable]
    public struct BlittableCollider
    {   
        public BlittableColliderType colliderType;
        public Vector3 offset;
        public float radius;
        public Vector3 tail;
        public int transformIndex;
    }
}