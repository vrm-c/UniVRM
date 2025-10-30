using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.InputPorts
{
    [Serializable]
    public struct FastSpringBoneSpring
    {
        public Transform center;
        public FastSpringBoneJoint[] joints;
        public FastSpringBoneCollider[] colliders;
    }
}