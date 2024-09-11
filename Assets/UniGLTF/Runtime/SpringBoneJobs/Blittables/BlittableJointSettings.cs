using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// - 毎フレームの変化を許可する
    /// </summary>
    [Serializable]
    public struct BlittableJointSettings
    {
        public float stiffnessForce;
        public float gravityPower;
        public Vector3 gravityDir;
        public float dragForce;
        public float radius;
    }
}