using System;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
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
        /// Joint レベルの可変情報をセットする
        /// stiffness, 
        /// </summary>
        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings);

        /// <summary>
        /// Model レベルの可変情報をセットする
        /// 風, pause, scaling
        /// </summary>
        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings);

        /// <summary>
        /// System レベルの可変情報
        /// TODO: デザイン検討中。
        /// deltaTime のカスタマイズポイント。通常は Time.dletaTime
        /// </summary>
        // public float DeltaTime { get; }

        /// <summary>
        /// 毎フレーム Vrm10Runtime.Process から呼ばれる。
        /// </summary>
        public void Process(float deltaTime);

        /// <summary>
        /// from Vrm10Instance.OnDrawGizmosSelected
        /// </summary>
        public void DrawGizmos();
    }
}