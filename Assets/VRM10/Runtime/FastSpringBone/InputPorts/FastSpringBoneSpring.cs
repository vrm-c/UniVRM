using System;
using UnityEngine;

namespace UniVRM10.FastSpringBones.System
{
    [Serializable]
    public struct FastSpringBoneSpring
    {
        public Transform center;
        public FastSpringBoneJoint[] joints;
        public FastSpringBoneCollider[] colliders;
    }
}