using System;
using Unity.Mathematics;
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
    public readonly struct BlittableJointMutable
    {
        private readonly float4x2 _data;
        
        public float stiffnessForce => _data.c0.x;
        public float gravityPower => _data.c0.y;
        public float3 gravityDir => _data.c1.xyz;
        public float dragForce => _data.c0.z;
        public float radius => _data.c0.w;
        
        public BlittableJointMutable(float stiffnessForce = 0,
            float gravityPower = 0,
            float3 gravityDir = default,
            float dragForce = 0,
            float radius = 0)
        {
            var c0 = new float4(stiffnessForce, gravityPower, dragForce, radius);
            var c1 = new float4(gravityDir, 0);
            _data = new float4x2(c0, c1);
        }
    }
}