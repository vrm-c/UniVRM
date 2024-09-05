using System;
using UnityEngine;
using UniGLTF.SpringBoneJobs.Blittables;

namespace UniGLTF.SpringBoneJobs.InputPorts
{
    [Serializable]
    public struct FastSpringBoneCollider
    {
        public Transform Transform;
        public BlittableCollider Collider;
    }
}