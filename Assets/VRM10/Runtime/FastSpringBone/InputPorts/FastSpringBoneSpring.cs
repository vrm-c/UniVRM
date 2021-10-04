using System;

namespace UniVRM10.FastSpringBones.System
{
    [Serializable]
    public struct FastSpringBoneSpring
    {
        public FastSpringBoneJoint[] joints;
        public FastSpringBoneCollider[] colliders;
    }
}