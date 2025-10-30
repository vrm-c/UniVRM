using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VRM モデルインスタンスを、状態をもって、元の状態から操作・変更するためのクラス。
    /// また、仕様に従ってその操作を行う。
    ///
    /// 操作対象としては以下が挙げられる。
    /// - ControlRig
    /// - Constraint
    /// - LookAt
    /// - Expression
    /// </summary>
    public class Vrm10Runtime : IDisposable
    {
        private readonly Vrm10Instance m_instance;
        private readonly Transform m_head;

        /// <summary>
        /// Control Rig may be null.
        /// Control Rig is generated at loading runtime only.
        /// </summary>
        public Vrm10RuntimeControlRig ControlRig { get; }

        public IVrm10Constraint[] Constraints { get; }
        public Vrm10RuntimeExpression Expression { get; }
        public Vrm10RuntimeLookAt LookAt { get; }
        public IVrm10SpringBoneRuntime SpringBone { get; }
        public IVrm10Animation VrmAnimation { get; set; }

        [Obsolete("use Vrm10Runtime.SpringBone.SetModelLevel")]
        public Vector3 ExternalForce
        {
            get
            {
                throw new NotImplementedException();
                // return SpringBone.ExternalForce;
            }
            set
            {
                // SpringBone.SetModelLevel = value;
                throw new NotImplementedException();
            }
        }

        IReadOnlyDictionary<Transform, TransformState> _initPose;

        public Vrm10Runtime(Vrm10Instance instance, bool useControlRig, IVrm10SpringBoneRuntime springBoneRuntime,
            IReadOnlyDictionary<Transform, TransformState> initPose, bool isPrefabInstance)
        {
            if (!Application.isPlaying)
            {
                UniGLTFLogger.Warning($"{nameof(Vrm10Runtime)} expects runtime behaviour.");
            }

            _initPose = initPose;
            m_instance = instance;
            if (m_instance == null)
            {
                return;
            }

            if (!instance.TryGetBoneTransform(HumanBodyBones.Head, out m_head))
            {
                throw new Exception();
            }

            if (useControlRig)
            {
                ControlRig = new Vrm10RuntimeControlRig(instance.Humanoid, m_instance.transform);
            }
            Constraints = instance.GetComponentsInChildren<IVrm10Constraint>();
            LookAt = new Vrm10RuntimeLookAt(instance, instance.Humanoid, ControlRig);
            Expression = new Vrm10RuntimeExpression(instance, LookAt.EyeDirectionApplicable, isPrefabInstance);
            SpringBone = springBoneRuntime;
        }

        public void Dispose()
        {
            Expression.Dispose();
            ControlRig?.Dispose();
            SpringBone.Dispose();
        }

        [Obsolete("use Vrm10Runtime.SpringBone.ReconstructSpringBone")]
        public void ReconstructSpringBone()
        {
            SpringBone.ReconstructSpringBone();
        }
        /// <summary>
        /// 毎フレーム関連コンポーネントを解決する
        ///
        /// * Update from VrmAnimation
        /// * Constraint
        /// * Spring
        /// * LookAt
        /// * Expression
        ///
        /// </summary>
        public void Process()
        {
            // 1. Update From VrmAnimation
            if (VrmAnimation != null)
            {
                // copy pose
                {
                    Vrm10Retarget.Retarget(VrmAnimation.ControlRig, (ControlRig, ControlRig));
                }

                // update expressions
                foreach (var (k, v) in VrmAnimation.ExpressionMap)
                {
                    Expression.SetWeight(k, v());
                }

                // look at
                if (VrmAnimation.LookAt.HasValue)
                {
                    LookAt.LookAtInput = VrmAnimation.LookAt.Value;
                }
            }

            // 2. Control Rig
            ControlRig?.Process();

            // 3. Constraints
            foreach (var constraint in Constraints)
            {
                if (constraint.ConstraintSource != null)
                {
                    constraint.Process(
                        targetInitState: _initPose[constraint.ConstraintTarget],
                        sourceInitState: _initPose[constraint.ConstraintSource]);
                }
            }

            if (m_instance.LookAtTargetType == VRM10ObjectLookAt.LookAtTargetTypes.SpecifiedTransform
            && m_instance.LookAtTarget != null)
            {
                // Transform 追跡で視線を生成する。
                // 値を上書きします。
                LookAt.LookAtInput = new LookAtInput { WorldPosition = m_instance.LookAtTarget.position };
            }

            // 4. Gaze control
            var eyeDirection = LookAt.Process();

            // 5. Apply Expression
            // LookAt の角度制限などはこちらで処理されます。
            Expression.Process(eyeDirection);

            // 6. SpringBone
            SpringBone.Process(Time.deltaTime);
        }
    }
}