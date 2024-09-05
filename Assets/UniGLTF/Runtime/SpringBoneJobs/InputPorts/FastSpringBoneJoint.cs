using System;
using UnityEngine;
using UniGLTF.SpringBoneJobs.Blittables;

namespace UniGLTF.SpringBoneJobs.InputPorts
{
    [Serializable]
    public struct FastSpringBoneJoint
    {
        public Transform Transform;
        public BlittableJoint Joint;
        public Quaternion DefaultLocalRotation;
    }
}