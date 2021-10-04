using System;
using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// SpringBoneの各関節を表すデータ型
    /// </summary>
    [Serializable]
    public struct BlittableJoint
    {
        public float stiffnessForce;
        public float gravityPower;
        public Vector3 gravityDir;
        public float dragForce;
        public float radius;
    }
}
