using Unity.Collections;
using UnityEngine;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// SpringBoneの各関節を表すデータ型
    /// </summary>
    public struct BlittableJoint
    {
        public float StiffnessForce;
        public float GravityPower;
        public Vector3 GravityDir;
        public float DragForce;
        public float Radius;
    }
}
