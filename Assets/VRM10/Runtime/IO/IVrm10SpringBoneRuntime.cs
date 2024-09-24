using System;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10SpringBoneRuntime : IDisposable
    {
        public Task InitializeAsync(Vrm10Instance instance, IAwaitCaller awaitCaller);

        /// <summary>
        /// SpringBone の構成変更を反映して再構築する。
        /// </summary>
        public bool ReconstructSpringBone();

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
        // SpringBone のランタイムの動作状態を設定する。
        // SpringBone の動きを一時停止したいときは false にする。
        /// </summary>
        public bool IsSpringBoneEnabled { get; set; }

        /// <summary>
        /// 毎フレーム Vrm10Runtime.Process から呼ばれる。
        /// </summary>
        public void Process();
    }
}