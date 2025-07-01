using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    public readonly struct BlittableModelLevel
    {
        private readonly float4 _data;
        
        /// <summary>
        /// World 座標系の追加の力。風など。
        /// </summary>
        public float3 ExternalForce => _data.xyz;

        /// <summary>
        /// 処理結果の Transform への書き戻しを停止する。
        /// </summary>
        public bool StopSpringBoneWriteback => ((int)_data.w & 1) != 0;

        /// <summary>
        /// スケール値に連動して SpringBone のパラメータを自動調整する。
        /// (見た目の角速度が同じになるようにする)
        /// </summary>
        public bool SupportsScalingAtRuntime => ((int)_data.w & 2) != 0;
        
        public BlittableModelLevel(float3 externalForce = default,
            bool stopSpringBoneWriteback = false,
            bool supportsScalingAtRuntime = false)
        {
            var w = (stopSpringBoneWriteback ? 1 : 0) | ((supportsScalingAtRuntime ? 1 : 0) << 1);
            _data = new float4(externalForce, w);
        }
    }
}