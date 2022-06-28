using System;
using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    [Serializable]
    public struct FastSpringBoneJoint
    {
        public Transform Transform;
        public BlittableJoint Joint;
        public Quaternion DefaultLocalRotation;
    }
}