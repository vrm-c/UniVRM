using System.Collections.Generic;
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

        [SerializeField, Header("LookAt")]
        public bool DrawLookAtGizmo = true;

        /// <summay>
        /// LookAtTargetTypes.CalcYawPitchToGaze時の注視点
        /// </summary>
        [SerializeField]
        public Transform Gaze;

        [SerializeField]
        public VRM10ObjectLookAt.LookAtTargetTypes LookAtTargetType;

        private UniHumanoid.Humanoid m_humanoid;
        private Vrm10Runtime m_runtime;

        /// <summary>
        /// ControlRig の生成オプション
        /// 
        /// null: ControlRigGenerationOption.None
        /// empty: ControlRigGenerationOption.Generate = Vrm0XCompatibleRig
        /// other: ControlRigGenerationOption.Vrm0XCompatibleWithXR_EXT_hand_tracking など
        /// </summary>
        private IReadOnlyDictionary<HumanBodyBones, Quaternion> m_controlRigInitialRotations;

        /// <summary>
        /// VRM ファイルに記録された Humanoid ボーンに対応します。
        /// これは、コントロールリグのボーンとは異なります。
        /// </summary>
        public UniHumanoid.Humanoid Humanoid
        {
            get
            {
                if (m_humanoid == null)
                {
                    m_humanoid = GetComponent<UniHumanoid.Humanoid>();
                }
                return m_humanoid;
            }
        }

        /// <summary>
        /// ランタイム情報
        /// </summary>
        public Vrm10Runtime Runtime
        {
            get
            {
                if (m_runtime == null)
                {
                    m_runtime = new Vrm10Runtime(this, m_controlRigInitialRotations);
                }
                return m_runtime;
            }
        }

        internal void InitializeAtRuntime(IReadOnlyDictionary<HumanBodyBones, Quaternion> controlRigInitialRotations)
        {
            m_controlRigInitialRotations = controlRigInitialRotations;
        }

        void Start()
        {
            if (Vrm == null)
            {
                Debug.LogError("no VRM10Object");
                enabled = false;
                return;
            }

            // cause new Vrm10Runtime.
            // init LookAt init rotation.
            var runtime = Runtime;

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

        public bool TryGetBoneTransform(HumanBodyBones bone, out Transform t)
        {
            t = Humanoid.GetBoneTransform(bone);
            if (t == null)
            {
                return false;
            }
            return true;
        }


    }
}
