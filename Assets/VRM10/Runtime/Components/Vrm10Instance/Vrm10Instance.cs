using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


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
        /// The model looks at position of the Transform specified in this field.
        /// That behaviour is available only when LookAtTargetType is SpecifiedTransform.
        ///
        /// モデルはここで指定した Transform の位置の方向に目を向けます。
        /// LookAtTargetType を SpecifiedTransform に設定したときのみ有効です。
        /// </summary>
        [SerializeField, FormerlySerializedAs("Gaze")]
        public Transform LookAtTarget;

        /// <summary>
        /// Specify "LookAt" behaviour at runtime.
        ///
        /// 実行時の目の動かし方を指定します。
        /// </summary>
        [SerializeField]
        public VRM10ObjectLookAt.LookAtTargetTypes LookAtTargetType;

        private UniHumanoid.Humanoid m_humanoid;
        private Vrm10Runtime m_runtime;

        /// <summary>
        /// ControlRig の生成オプション
        /// </summary>
        private bool m_useControlRig;

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
                    m_runtime = new Vrm10Runtime(this, m_useControlRig);
                }
                return m_runtime;
            }
        }

        internal void InitializeAtRuntime(bool useControlRig)
        {
            m_useControlRig = useControlRig;
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
            var _ = Runtime;
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
            if (Humanoid == null)
            {
                t = null;
                return false;
            }
            t = Humanoid.GetBoneTransform(bone);
            if (t == null)
            {
                return false;
            }
            return true;
        }

        #region Obsolete

        [Obsolete]
        public Transform Gaze
        {
            get => LookAtTarget;
            set => LookAtTarget = value;
        }

        #endregion
    }
}
