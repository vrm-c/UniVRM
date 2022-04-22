using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// VRM全体を制御するRoot
    /// 
    /// Importer(scripted importer) -> Prefab(editor/asset) -> Instance(scene/MonoBehavior) -> Runtime(play時)
    /// 
    /// * DefaultExecutionOrder(11000) means calculate springbone after FinalIK( VRIK )
    /// </summary>
    [AddComponentMenu("VRM10/VRMInstance")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(11000)]
    public class Vrm10Instance : MonoBehaviour
    {
        /// <summary>
        /// シリアライズ情報
        /// </summary>
        [SerializeField, Header("VRM1")]
        public VRM10Object Vrm;

        /// <summary>
        /// SpringBone のシリアライズ情報
        /// </summary>
        /// <returns></returns>
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

        Vrm10Runtime m_runtime;

        /// <summary>
        /// ランタイム情報
        /// </summary>
        public Vrm10Runtime Runtime
        {
            get
            {
                if (m_runtime == null)
                {
                    m_runtime = new Vrm10Runtime(this);
                }
                return m_runtime;
            }
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
                Runtime.Process();
            }
        }

        private void LateUpdate()
        {
            if (UpdateType == UpdateTypes.LateUpdate)
            {
                Runtime.Process();
            }
        }

        private void OnDestroy()
        {
            Runtime.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach (var spring in SpringBone.Springs)
            {
                foreach (var (head, tail) in spring.EnumHeadTail())
                {
                    Gizmos.DrawLine(head.transform.position, tail.transform.position);
                    Gizmos.DrawWireSphere(tail.transform.position, head.m_jointRadius);
                }
            }
        }
    }
}
