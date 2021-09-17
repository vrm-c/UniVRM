using System;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// VRM全体を制御するコンポーネント。
    /// 
    /// 各フレームのHumanoidへのモーション適用後に任意のタイミングで
    /// Applyを呼び出してください。
    /// 
    /// ヒエラルキー内への参照のシリアライズ
    /// 
    /// * Humanoid(VRM必須)
    /// * SpringBone の MonoBehaviour でない部分
    ///   * ColliderGroup
    ///   * Springs
    /// 
    /// </summary>
    [AddComponentMenu("VRM10/VRMController")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(11000)]
    public class VRM10Controller : MonoBehaviour
    {
        [SerializeField, Header("VRM1")]
        public VRM10Object Vrm;

        [SerializeField]
        public VRM10ControllerSpringBone SpringBone = new VRM10ControllerSpringBone();

        public enum UpdateTypes
        {
            None,
            Update,
            LateUpdate,
        }

        [SerializeField, Header("Runtime")]
        public UpdateTypes UpdateType = UpdateTypes.LateUpdate;

        [SerializeField]
        public Transform SpringBoneCenter;

        [SerializeField, Header("LookAt")]
        public bool DrawLookAtGizmo = true;

        /// <summay>
        /// LookAtTargetTypes.CalcYawPitchToGaze時の注視点
        /// </summary>
        [SerializeField]
        public Transform Gaze;

        [SerializeField]
        public VRM10ObjectLookAt.LookAtTargetTypes LookAtTargetType;

        VRM10ControllerRuntime m_runtime;

        /// <summary>
        /// delay new VRM10ControllerRuntime
        /// </summary>
        /// <returns></returns>
        VRM10ControllerRuntime GetOrCreate()
        {
            if (m_runtime == null)
            {
                m_runtime = new VRM10ControllerRuntime(this);
            }
            return m_runtime;
        }

        void Start()
        {
            if (LookAtTargetType == VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze)
            {
                if (Gaze == null)
                {
                    LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.SetYawPitch;
                }
            }
        }

        private void Update()
        {
            if (UpdateType == UpdateTypes.Update)
            {
                GetOrCreate().Process();
            }
        }

        private void LateUpdate()
        {
            if (UpdateType == UpdateTypes.LateUpdate)
            {
                GetOrCreate().Process();
            }
        }
    }
}
