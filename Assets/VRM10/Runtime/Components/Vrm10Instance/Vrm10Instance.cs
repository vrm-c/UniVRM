using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;
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
    [RequireComponent(typeof(UniHumanoid.Humanoid))]
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
        // 中継用。InitializeAtRuntime でもらって MakeRuntime で使う
        private IVrm10SpringBoneRuntime m_springBoneRuntime;
        private IReadOnlyDictionary<Transform, TransformState> m_defaultTransformStates;

        /// <summary>
        /// ControlRig の生成オプション
        /// </summary>
        private bool m_useControlRig;

        public IReadOnlyDictionary<Transform, TransformState> DefaultTransformStates
        {
            get
            {
                if (m_defaultTransformStates == null)
                {
                    if (TryGetComponent<RuntimeGltfInstance>(out var gltfInstance))
                    {
                        // ランタイムインポートならここに到達してゼロコストになる
                        m_defaultTransformStates = gltfInstance.InitialTransformStates;
                    }
                    else
                    {
                        // エディタでプレハブ配置してる奴ならこっちに到達して収集する
                        m_defaultTransformStates = GetComponentsInChildren<Transform>()
                            .ToDictionary(tf => tf, tf => new TransformState(tf));
                    }
                }
                return m_defaultTransformStates;
            }
        }

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
                    m_humanoid = this.GetComponentOrNull<UniHumanoid.Humanoid>();
                }
                return m_humanoid;
            }
        }

        internal Vrm10Runtime MakeRuntime(bool useControlRig)
        {
            if (m_springBoneRuntime == null)
            {
                // springbone が無い => シーン配置モデルが play されたと見做す
                var provider = GetComponent<IVrm10SpringBoneRuntimeProvider>();
                if (provider != null)
                {
                    // 明示的カスタマイズ
                    m_springBoneRuntime = provider.CreateSpringBoneRuntime();
                }

                if (m_springBoneRuntime == null)
                {
                    // シーン配置 play のデフォルトは singletone ではない方
                    m_springBoneRuntime = new Vrm10FastSpringboneRuntimeStandalone();
                }

                m_springBoneRuntime.InitializeAsync(this, new ImmediateCaller());
            }
            else
            {
                // importer 内で InitializeAsync が呼び出し済み
            }
            return new Vrm10Runtime(this, useControlRig, m_springBoneRuntime);
        }

        public Vrm10Runtime Runtime
        {
            get
            {
                if (m_runtime == null)
                {
                    if (this == null) throw new MissingReferenceException("instance was destroyed");
                    m_runtime = MakeRuntime(m_useControlRig);
                }
                return m_runtime;
            }
        }

        internal void InitializeAtRuntime(
            bool useControlRig,
            IVrm10SpringBoneRuntime springBoneRuntime,
            IReadOnlyDictionary<Transform, TransformState> defaultTransformStates = null
            )
        {
            m_useControlRig = useControlRig;
            m_springBoneRuntime = springBoneRuntime;

            if (defaultTransformStates != null)
            {
                m_defaultTransformStates = defaultTransformStates;
            }
            else
            {
                if (TryGetComponent<RuntimeGltfInstance>(out var gltfInstance))
                {
                    // ランタイムインポートならここに到達してゼロコストになる
                    defaultTransformStates = gltfInstance.InitialTransformStates;
                }
                else
                {
                    // エディタでプレハブ配置してる奴ならこっちに到達して収集する
                    defaultTransformStates = GetComponentsInChildren<Transform>()
                        .ToDictionary(tf => tf, tf => new TransformState(tf));
                }
            }
        }

        void Start()
        {
            if (Vrm == null)
            {
                UniGLTFLogger.Error("no VRM10Object");
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
            if (m_runtime != null)
            {
                m_runtime.Dispose();
                m_runtime = null;
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

        private void OnDrawGizmosSelected()
        {
            foreach (var spring in SpringBone.Springs)
            {
                spring.DrawGizmos();
            }

            if (Application.isPlaying)
            {
                Runtime.SpringBone.DrawGizmos();
            }
        }

        #region Obsolete

        [Obsolete]
        public Transform Gaze
        {
            get => LookAtTarget;
            set => LookAtTarget = value;
        }

        #endregion

        public bool TryGetRadiusAsTail(VRM10SpringBoneJoint target, out float? radius)
        {
            foreach (var spring in SpringBone.Springs)
            {
                VRM10SpringBoneJoint prev = default;
                foreach (var joint in spring.Joints)
                {
                    if (joint == target)
                    {
                        if (prev != null)
                        {
                            radius = prev.m_jointRadius;
                            return true;
                        }
                        else
                        {
                            radius = default;
                            return true;
                        }
                    }
                    prev = joint;
                }
            }

            radius = default;
            return false;
        }
    }
}
