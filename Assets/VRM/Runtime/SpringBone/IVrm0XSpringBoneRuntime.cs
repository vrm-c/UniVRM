using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;

namespace VRM
{
    public interface IVrm0XSpringBoneRuntime
    {
        public Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller);

        /// <summary>
        /// SpringBone の構成変更を反映して再構築する。
        /// </summary>
        public void ReconstructSpringBone();

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
    }
}