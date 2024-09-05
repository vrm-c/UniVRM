using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
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
        // capsule tail or plane normal
        public Vector3 tailOrNormal;
        public int transformIndex;
    }
}