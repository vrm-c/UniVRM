using System;
using System.Linq;
using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;
using UniVRM10.FastSpringBones.System;

namespace UniVRM10
{
    /// <summary>
    /// Play時 と Editorからの参照情報置き場
    /// </summary>
    public class Vrm10InstanceRuntime : IDisposable
    {
        private readonly Vrm10Instance m_target;
        private readonly VRM10Constraint[] m_constraints;
        private readonly Transform m_head;
        private readonly FastSpringBoneService m_fastSpringBoneService;
        private readonly FastSpringBoneBuffer m_fastSpringBoneBuffer;

        public Vrm10InstanceRuntime(Vrm10Instance target)
        {
            m_target = target;
            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                throw new Exception();
            }

            m_head = animator.GetBoneTransform(HumanBodyBones.Head);
            target.Vrm.LookAt.Setup(animator, m_head, target.LookAtTargetType, target.Gaze);
            target.Vrm.Expression.Setup(target, target.Vrm.LookAt, target.Vrm.LookAt.EyeDirectionApplicable);

            if (m_constraints == null)
            {
                m_constraints = target.GetComponentsInChildren<VRM10Constraint>();
            }

            if (!Application.isPlaying)
            {
                // for UnitTest
                return;
            }

            m_fastSpringBoneService = FastSpringBoneService.Instance;
            m_fastSpringBoneBuffer = CreateFastSpringBoneBuffer(m_target.SpringBone);
            m_fastSpringBoneService.BufferCombiner.Register(m_fastSpringBoneBuffer);
        }

        private FastSpringBoneBuffer CreateFastSpringBoneBuffer(Vrm10InstanceSpringBone springBone)
        {
            return new FastSpringBoneBuffer(
                springBone.Springs.Select(spring => new FastSpringBoneSpring
                {
                    center = m_target.SpringBoneCenter,
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
                        }
                    }).ToArray(),
                }).ToArray());
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
            m_target.Vrm.LookAt.Process(m_target.LookAtTargetType, m_target.Gaze);

            //
            // expression
            //
            m_target.Vrm.Expression.Process();
        }

        public void Dispose()
        {
            m_fastSpringBoneService.BufferCombiner.Unregister(m_fastSpringBoneBuffer);
            m_fastSpringBoneBuffer.Dispose();
        }
    }
}