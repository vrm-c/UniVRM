using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10SpringBoneRuntime : IDisposable
    {
        public void Initialize(Vrm10Instance instance);

        /// <summary>
        /// 主に singleton のバッチング更新。
        /// 
        /// 再構築。initialTransform を使って再構築？
        /// Joint の増減、T-Pose の変更などある？
        /// </summary>
        public void ReconstructSpringBone();

        /// <summary>
        /// initialTransform 状態に復帰。verlet の速度 も 0 に。
        /// </summary>
        public void RestoreInitialTransform();

        public Vector3 ExternalForce { get; set; }

        public bool IsSpringBoneEnabled { get; set; }
    }
}