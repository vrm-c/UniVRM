using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Reconstruct に対して Mutable。
    /// Reconstruct より軽量な JointReconfigure(仮) で変更できるようにする予定。
    /// おもに Editor play で設定を変更しながら動作を見る用途を想定している。
    /// JointReconfigure を呼ばなければ以前と同じで不変となる。
    /// </summary>
    [Serializable]
    public struct BlittableJointMutable
    {
        public float stiffnessForce;
        public float gravityPower;
        public Vector3 gravityDir;
        public float dragForce;
        public float radius;
    }
}