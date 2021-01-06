using System;
using UnityEngine;

namespace UniVRM10
{
    public enum SpringBoneColliderTypes
    {
        Sphere,
        Capsule,
    }

    [Serializable]
    public class SpringBoneCollider
    {
        public SpringBoneColliderTypes ColliderType;

        /// <summary>bone local position</summary>
        public Vector3 Offset;

        [Range(0, 1.0f)]
        public float Radius;

        /// <summary>bone local position</summary>
        public Vector3 Tail;
    }
}
