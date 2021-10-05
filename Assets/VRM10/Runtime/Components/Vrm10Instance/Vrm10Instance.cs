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
    /// * DefaultExecutionOrder(11000) means calculate springbone after FinalIK( VRIK )
    /// </summary>
    [AddComponentMenu("VRM10/VRMInstance")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(11000)]
    public class Vrm10Instance : MonoBehaviour
    {
        [SerializeField, Header("VRM1")]
        public VRM10Object Vrm;

        [SerializeField]
        public Vrm10InstanceSpringBone SpringBone = new Vrm10InstanceSpringBone();

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

        Vrm10InstanceRuntime m_runtime;

        /// <summary>
        /// delay new Vrm10InstanceRuntime
        /// </summary>
        /// <returns></returns>
        Vrm10InstanceRuntime GetOrCreate()
        {
            if (m_runtime == null)
            {
                m_runtime = new Vrm10InstanceRuntime(this);
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

        private void OnDestroy()
        {
            GetOrCreate().Dispose();
        }
    }
}
