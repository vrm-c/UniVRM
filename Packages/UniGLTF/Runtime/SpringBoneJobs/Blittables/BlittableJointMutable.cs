using System;
using Unity.Mathematics;

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
        /// <summary>
        /// |x       |y       |z       |w       |
        /// |--------|--------|--------|--------|
        /// |stiff   |g-power |drag    |radius  |
        /// |g-dir.x |g-dir.y |g-dir.z |        |
        /// |al.type |limit1  |limit2  |  
        /// |offset.x|offset.y|offset.z|offset.w|
        /// 
        /// g-power = gravityPower
        /// g-dir = gravityDir
        /// al = anglelimit
        /// offset = anglelimitOffset
        /// </summary>
        private readonly float4x4 _data;

        public float stiffnessForce => _data.c0.x;
        public float gravityPower => _data.c0.y;
        public float3 gravityDir => _data.c1.xyz;
        public float dragForce => _data.c0.z;
        public float radius => _data.c0.w;

        public AnglelimitTypes anglelimitType => (AnglelimitTypes)_data.c2.x;
        public float anglelimit1 => _data.c2.y;
        public float anglelimit2 => _data.c2.z;
        public quaternion anglelimitOffset => _data.c3.xyzw;

        public BlittableJointMutable(float stiffnessForce = 0,
            float gravityPower = 0,
            float3 gravityDir = default,
            float dragForce = 0,
            float radius = 0,
            // v0.129.4 anglelimit
            float angleLimitType = 0,
            float angleLimit1 = 0,
            float angleLimit2 = 0,
            quaternion angleLimitOffset = default)
        {
            var c0 = new float4(stiffnessForce, gravityPower, dragForce, radius);
            var c1 = new float4(gravityDir, 0);
            var c2 = new float4(angleLimitType, angleLimit1, angleLimit2, 0);
            var c3 = angleLimitOffset.value;
            _data = new float4x4(c0, c1, c2, c3);
        }
    }
}