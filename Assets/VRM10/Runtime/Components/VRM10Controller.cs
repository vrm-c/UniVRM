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
        }

        [SerializeField]
        public VRM10ControllerImpl Controller = new VRM10ControllerImpl();

        [SerializeField]
        public VRM10MetaObject Meta;

        [SerializeField]
        public VRM10ControllerExpression Expression = new VRM10ControllerExpression();

        [SerializeField]
        public VRM10ControllerLookAt LookAt = new VRM10ControllerLookAt();

        [SerializeField]
        public VRM10ControllerFirstPerson FirstPerson = new VRM10ControllerFirstPerson();

        [SerializeField]
        public VRM10SpringBoneManager SpringBone = new VRM10SpringBoneManager();

        void OnDestroy()
        {
            if (Expression != null)
            {
                Expression.Restore();
            }
        }

        VRM10Constraint[] m_constraints;

        Transform m_head;
        public Transform Head
        {
            get
            {
                if (m_head == null)
                {
                    m_head = GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                }
                return m_head;
            }
        }

        void Reset()
        {
            var animator = GetComponent<Animator>();
            m_head = animator.GetBoneTransform(HumanBodyBones.Head);
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

        public void Setup()
        {
            var animator = GetComponent<Animator>();
            if (animator == null) return;

            m_head = animator.GetBoneTransform(HumanBodyBones.Head);
            LookAt.Setup(animator, m_head);
            Expression.Setup(transform, LookAt, LookAt.EyeDirectionApplicable);
        }

        /// <summary>
        /// 毎フレーム関連コンポーネントを解決する
        /// 
        /// * Contraint
        /// * Spring
        /// * LookAt
        /// * Expression
        /// 
        /// </summary>
        public void Process()
        {
            // 
            // constraint
            //
            if (m_constraints == null)
            {
                m_constraints = GetComponentsInChildren<VRM10Constraint>();
            }
            foreach (var constraint in m_constraints)
            {
                constraint.Process();
            }

            //
            // spring
            //
            SpringBone.Process();

            //
            // gaze control
            //
            LookAt.Process();

            //
            // expression
            //
            Expression.Process();
        }

        private void OnEnable()
        {
            Setup();
        }

        private void Update()
        {
            if (Controller.UpdateType == VRM10ControllerImpl.UpdateTypes.Update)
            {
                Process();
            }
        }

        private void LateUpdate()
        {
            if (Controller.UpdateType == VRM10ControllerImpl.UpdateTypes.LateUpdate)
            {
                Process();
            }
        }

        private void OnDrawGizmos()
        {
            SpringBone.DrawGizmos();
        }
    }
}
