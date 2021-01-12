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

        [Serializable]
        public class VRM10ControllerImpl
        {
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
        public ModelAsset ModelAsset;

        void OnDestroy()
        {
            Expression.Dispose();

            if (ModelAsset != null)
            {
#if UNITY_EDITOR
                ModelAsset.DisposeEditor();
#else
                ModelAsset.Dispose();
#endif
            }
        }

        VRMConstraint[] m_constraints;
        VRMSpringBone[] m_springs;

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

        private void Start()
        {
            Expression.OnStart(transform);

            // get lookat origin
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                m_head = animator.GetBoneTransform(HumanBodyBones.Head);
                LookAt.Setup(animator, m_head);
            }
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
        public void Apply()
        {
            // 
            // constraint
            //
            if (m_constraints == null)
            {
                m_constraints = GetComponentsInChildren<VRMConstraint>();
            }
            foreach (var constraint in m_constraints)
            {
                constraint.Process();
            }

            //
            // spring
            //
            if (m_springs == null)
            {
                m_springs = GetComponentsInChildren<VRMSpringBone>();
            }
            foreach (var spring in m_springs)
            {
                spring.Process();
            }

            //
            // expression
            //
            var validateState = Expression.Begin();

            //
            // gaze control
            //
            LookAt.Process(Head, Expression.SetPresetValue, validateState.ignoreLookAt);

            //
            // expression
            //
            Expression.End(validateState);
        }

        private void Update()
        {
            if (Controller.UpdateType == UpdateTypes.Update)
            {
                Apply();
            }
        }

        private void LateUpdate()
        {
            if (Controller.UpdateType == UpdateTypes.LateUpdate)
            {
                Apply();
            }
        }
    }
}
