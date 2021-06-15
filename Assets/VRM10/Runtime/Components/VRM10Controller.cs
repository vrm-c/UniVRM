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
    /// </summary>
    [AddComponentMenu("VRM10/VRMController")]
    [DisallowMultipleComponent]
    public class VRM10Controller : MonoBehaviour
    {
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

        [SerializeField, Header("VRM1")]
        public VRM10Object Vrm;

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
