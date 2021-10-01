using System;
using System.Collections.Generic;

namespace UniVRM10.FastSpringBones.System
{
    [Serializable]
    public struct FastSpringBoneSpring
    {
        public FastSpringBoneJoint[] Joints;
        public FastSpringBoneCollider[] Colliders;
    }
}