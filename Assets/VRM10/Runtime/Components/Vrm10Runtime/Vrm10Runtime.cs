using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;
using UniVRM10.FastSpringBones.System;

namespace UniVRM10
{
    /// <summary>
    /// Play時 と Editorからの参照情報置き場
    /// </summary>
    public class Vrm10Runtime : IDisposable
    {
        private readonly Vrm10Instance m_target;
        private readonly IVrm10Constraint[] m_constraints;
        private readonly Transform m_head;
        private readonly FastSpringBoneService m_fastSpringBoneService;
        private readonly Dictionary<Transform, (Vector3 position, Quaternion rotation)> m_defaultTransformStates;

        private FastSpringBoneBuffer m_fastSpringBoneBuffer;

        private Vrm10RuntimeExpression m_expression;
        public Vrm10RuntimeExpression Expression => m_expression;

        private Vrm10RuntimeLookAt m_lookat;
        public Vrm10RuntimeLookAt LookAt => m_lookat;

        public Vrm10Runtime(Vrm10Instance target)
        {
            m_target = target;
            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                throw new Exception();
            }

            m_head = animator.GetBoneTransform(HumanBodyBones.Head);
            m_lookat = new Vrm10RuntimeLookAt(target.Vrm.LookAt, animator, m_head, target.LookAtTargetType, target.Gaze);
            m_expression = new Vrm10RuntimeExpression(target, m_lookat, m_lookat.EyeDirectionApplicable);

            if (m_constraints == null)
            {
                m_constraints = target.GetComponentsInChildren<IVrm10Constraint>();
            }

            if (!Application.isPlaying)
            {
                // for UnitTest
                return;
            }
            
            var instance = target.GetComponent<RuntimeGltfInstance>();
            if (instance != null)
            {
                // ランタイムインポートならここに到達してほぼゼロコストになる
                m_defaultTransformStates = instance.Nodes.ToDictionary(tf=> tf, tf=>(tf.position, tf.rotation));
            }
            else
            {
                // エディタでプレハブ配置してる奴ならこっちに到達して収集する
                m_defaultTransformStates = target.GetComponentsInChildren<Transform>().ToDictionary(tf=> tf, tf=>(tf.position, tf.rotation));
            }

            m_fastSpringBoneService = FastSpringBoneService.Instance;
            m_fastSpringBoneBuffer = CreateFastSpringBoneBuffer(m_target.SpringBone);
            m_fastSpringBoneService.BufferCombiner.Register(m_fastSpringBoneBuffer);
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
                        DefaultLocalRotation = GetOrAddDefaultTransformState(joint.transform).rotation
                    }).ToArray(),
                }).ToArray());
        }

        private (Vector3 position, Quaternion rotation) GetOrAddDefaultTransformState(Transform tf)
        {
            if (m_defaultTransformStates.TryGetValue(tf, out var defaultTransformState))
            {
                return defaultTransformState;
            }
            return m_defaultTransformStates[tf] = (tf.position, tf.rotation);
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
            // 
            // constraint
            //
            foreach (var constraint in m_constraints)
            {
                constraint.Process();
            }

            //
            // gaze control
            //
            m_target.Runtime.LookAt.Process(m_target.LookAtTargetType, m_target.Gaze);

            //
            // expression
            //
            m_target.Runtime.Expression.Process();
        }

        public void Dispose()
        {
            m_fastSpringBoneService.BufferCombiner.Unregister(m_fastSpringBoneBuffer);
            m_fastSpringBoneBuffer.Dispose();
        }
    }
}