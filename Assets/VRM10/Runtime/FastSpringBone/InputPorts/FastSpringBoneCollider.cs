using System;
using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    [Serializable]
    public struct FastSpringBoneCollider
    {
        public Transform Transform;
        public BlittableCollider Collider;
    }
}