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
        [Serializable]
        public class VRM10ControllerImpl
        {
            public enum UpdateTypes
            {
                None,
                Update,
                LateUpdate,
            }

            [SerializeField, Header("UpdateSetting")]
            public UpdateTypes UpdateType = UpdateTypes.LateUpdate;

            [SerializeField, Header("SpringBone")]
            public Transform SpringBoneCenter;
        }

        [SerializeField]
        public VRM10ControllerImpl Controller = new VRM10ControllerImpl();

        [SerializeField]
        public VRM10ControllerMeta Meta = new VRM10ControllerMeta();

        [SerializeField]
        public VRM10ControllerExpression Expression = new VRM10ControllerExpression();

        [SerializeField]
        public VRM10ControllerLookAt LookAt = new VRM10ControllerLookAt();

        [SerializeField]
        public VRM10ControllerFirstPerson FirstPerson = new VRM10ControllerFirstPerson();

        [SerializeField]
        public VRM10ControllerSpringBone SpringBone = new VRM10ControllerSpringBone();

        void OnDestroy()
        {
            if (Expression != null)
            {
                Expression.Restore();
            }
        }

        private void OnValidate()
        {
            if (LookAt != null)
            {
                LookAt.HorizontalInner.OnValidate();
                LookAt.HorizontalOuter.OnValidate();
                LookAt.VerticalUp.OnValidate();
                LookAt.VerticalDown.OnValidate();
            }
        }

        VRM10ControllerRuntime m_runtime;

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
            if (Controller.UpdateType == VRM10ControllerImpl.UpdateTypes.Update)
            {
                GetOrCreate().Process();
            }
        }

        private void LateUpdate()
        {
            if (Controller.UpdateType == VRM10ControllerImpl.UpdateTypes.LateUpdate)
            {
                GetOrCreate().Process();
            }
        }
    }
}
