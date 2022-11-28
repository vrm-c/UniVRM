using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;
using UniVRM10.FastSpringBones.System;

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
        private readonly Vrm10Instance m_target;
        private readonly Transform m_head;
        private readonly FastSpringBoneService m_fastSpringBoneService;
        private readonly IReadOnlyDictionary<Transform, TransformState> m_defaultTransformStates;

        private FastSpringBoneBuffer m_fastSpringBoneBuffer;
        private BlittableExternalData m_externalData;

        /// <summary>
        /// Control Rig may be null.
        /// Control Rig is generated at loading runtime only.
        /// </summary>
        public Vrm10RuntimeControlRig ControlRig { get; }

        public IVrm10Constraint[] Constraints { get; }
        public Vrm10RuntimeExpression Expression { get; }
        public Vrm10RuntimeLookAt LookAt { get; }

        public Vector3 ExternalForce
        {
            get => m_externalData.ExternalForce;
            set
            {
                m_externalData.ExternalForce = value;
                m_fastSpringBoneBuffer.ExternalData = m_externalData;
            }
        }

        public Vrm10Runtime(Vrm10Instance target, IReadOnlyDictionary<HumanBodyBones, Quaternion> controlRigInitialRotations)
        {
            m_target = target;

            if (!target.TryGetBoneTransform(HumanBodyBones.Head, out m_head))
            {
                throw new Exception();
            }

            if (controlRigInitialRotations != null)
            {
                ControlRig = new Vrm10RuntimeControlRig(target.Humanoid, m_target.transform, controlRigInitialRotations);
            }
            Constraints = target.GetComponentsInChildren<IVrm10Constraint>();
            LookAt = new Vrm10RuntimeLookAt(target.Vrm.LookAt, target.Humanoid, m_head, target.LookAtTargetType, target.Gaze);
            Expression = new Vrm10RuntimeExpression(target, LookAt, LookAt.EyeDirectionApplicable);

            var instance = target.GetComponent<RuntimeGltfInstance>();
            if (instance != null)
            {
                // ランタイムインポートならここに到達してゼロコストになる
                m_defaultTransformStates = instance.InitialTransformStates;
            }
            else
            {
                // エディタでプレハブ配置してる奴ならこっちに到達して収集する
                m_defaultTransformStates = target.GetComponentsInChildren<Transform>()
                    .ToDictionary(tf => tf, tf => new TransformState(tf));
            }

            // NOTE: FastSpringBoneService は UnitTest などでは動作しない
            if (Application.isPlaying)
            {
                m_fastSpringBoneService = FastSpringBoneService.Instance;
                m_fastSpringBoneBuffer = CreateFastSpringBoneBuffer(m_target.SpringBone);
                m_fastSpringBoneService.BufferCombiner.Register(m_fastSpringBoneBuffer);
            }
        }

        public void Dispose()
        {
            ControlRig?.Dispose();
            m_fastSpringBoneService.BufferCombiner.Unregister(m_fastSpringBoneBuffer);
            m_fastSpringBoneBuffer.Dispose();
        }

        /// <summary>
        /// このVRMに紐づくSpringBone関連のバッファを再構築する
        /// ランタイム実行時にSpringBoneに対して変更を行いたいときは、このメソッドを明示的に呼ぶ必要がある
        /// </summary>
        public void ReconstructSpringBone()
        {
            m_fastSpringBoneService.BufferCombiner.Unregister(m_fastSpringBoneBuffer);

            m_fastSpringBoneBuffer.Dispose();
            m_fastSpringBoneBuffer = CreateFastSpringBoneBuffer(m_target.SpringBone);

            m_fastSpringBoneService.BufferCombiner.Register(m_fastSpringBoneBuffer);
        }

        private FastSpringBoneBuffer CreateFastSpringBoneBuffer(Vrm10InstanceSpringBone springBone)
        {
            return new FastSpringBoneBuffer(
                springBone.Springs.Select(spring => new FastSpringBoneSpring
                {
                    center = spring.Center,
                    colliders = spring.ColliderGroups
                    .SelectMany(group => group.Colliders)
                    .Select(collider => new FastSpringBoneCollider
                    {
                        Transform = collider.transform,
                        Collider = new BlittableCollider
                        {
                            offset = collider.Offset,
                            radius = collider.Radius,
                            tail = collider.Tail,
                            colliderType = TranslateColliderType(collider.ColliderType)
                        }
                    }).ToArray(),
                    joints = spring.Joints
                    .Select(joint => new FastSpringBoneJoint
                    {
                        Transform = joint.transform,
                        Joint = new BlittableJoint
                        {
                            radius = joint.m_jointRadius,
                            dragForce = joint.m_dragForce,
                            gravityDir = joint.m_gravityDir,
                            gravityPower = joint.m_gravityPower,
                            stiffnessForce = joint.m_stiffnessForce
                        },
                        DefaultLocalRotation = GetOrAddDefaultTransformState(joint.transform).LocalRotation,
                    }).ToArray(),
                }).ToArray(),
                m_externalData);
        }

        private TransformState GetOrAddDefaultTransformState(Transform tf)
        {
            if (m_defaultTransformStates.TryGetValue(tf, out var defaultTransformState))
            {
                return defaultTransformState;
            }

            Debug.LogWarning($"{tf.name} does not exist on load.");
            return new TransformState(null);
        }

        private static BlittableColliderType TranslateColliderType(VRM10SpringBoneColliderTypes colliderType)
        {
            switch (colliderType)
            {
                case VRM10SpringBoneColliderTypes.Sphere:
                    return BlittableColliderType.Sphere;
                case VRM10SpringBoneColliderTypes.Capsule:
                    return BlittableColliderType.Capsule;
                default:
                    throw new ArgumentOutOfRangeException();
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
        public void Process()
        {
            // 1. Control Rig
            ControlRig?.Process();

            // 2. Constraints
            foreach (var constraint in Constraints)
            {
                constraint.Process();
            }

            // 3. Gaze control
            LookAt.Process(m_target.LookAtTargetType, m_target.Gaze);

            // 4. Expression
            Expression.Process();
        }
    }
}