using System;
using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10SpringBoneRuntime : IDisposable
    {
        public void Initialize(Vrm10Instance instance);

        /// <summary>
        /// 主に singleton のバッチング更新。
        /// </summary>
        public void ReconstructSpringBone();

        /// <summary>
        /// initialTransform 状態に復帰。verlet の速度 も 0 に。
        /// </summary>
        public void RestoreInitialTransform();

        /// <summary>
        /// deltaTime のカスタマイズポイント。通常は Time.dletaTime
        /// </summary>
        public float DeltaTime { get; }

        /// <summary>
        /// 風などの追加の外力を設定する
        /// </summary>
        public Vector3 ExternalForce { get; set; }

        /// <summary>
        /// SpringBone を一時停止する
        /// </summary>
        public bool IsSpringBoneEnabled { get; set; }

        /// <summary>
        /// 毎フレーム Vrm10Runtime.Process から呼ばれる。
        /// </summary>
        public void Process();
    }
}