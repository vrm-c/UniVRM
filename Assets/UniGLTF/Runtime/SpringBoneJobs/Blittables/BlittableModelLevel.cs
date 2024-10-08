using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    public struct BlittableModelLevel
    {
        /// <summary>
        /// World 座標系の追加の力。風など。
        /// </summary>
        public Vector3 ExternalForce;

        /// <summary>
        /// 処理結果の Transform への書き戻しを停止する。
        /// </summary>
        public bool StopSpringBoneWriteback;

        /// <summary>
        /// スケール値に連動して SpringBone のパラメータを自動調整する。
        /// (見た目の角速度が同じになるようにする)
        /// </summary>
        public bool SupportsScalingAtRuntime;
    }
}