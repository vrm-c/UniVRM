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
    public class Vrm10RuntimeSpringBone : IDisposable
    {
        private readonly Vrm10Instance m_instance;
        private readonly IReadOnlyDictionary<Transform, TransformState> m_defaultTransformStates;
        private readonly FastSpringBoneService m_fastSpringBoneService;
        private FastSpringBoneBuffer m_fastSpringBoneBuffer;
        private BlittableExternalData m_externalData;

        public Vector3 ExternalForce
        {
            get => m_externalData.ExternalForce;
            set
            {
                m_externalData.ExternalForce = value;
                m_fastSpringBoneBuffer.ExternalData = m_externalData;
            }
        }

        public bool IsSpringBoneEnabled
        {
            get => m_externalData.IsSpringBoneEnabled;
            set
            {
                m_externalData.IsSpringBoneEnabled = value;
                m_fastSpringBoneBuffer.ExternalData = m_externalData;
            }
        }

        public Vrm10RuntimeSpringBone(Vrm10Instance instance)
        {
            m_instance = instance;

            var gltfInstance = instance.GetComponent<RuntimeGltfInstance>();
            if (gltfInstance != null)
            {
                // ランタイムインポートならここに到達してゼロコストになる
                m_defaultTransformStates = gltfInstance.InitialTransformStates;
            }
            else
            {
                // エディタでプレハブ配置してる奴ならこっちに到達して収集する
                m_defaultTransformStates = instance.GetComponentsInChildren<Transform>()
                    .ToDictionary(tf => tf, tf => new TransformState(tf));
            }

            // NOTE: FastSpringBoneService は UnitTest などでは動作しない
            if (Application.isPlaying)
            {
                m_fastSpringBoneService = FastSpringBoneService.Instance;
                m_fastSpringBoneBuffer = CreateFastSpringBoneBuffer(m_instance.SpringBone);
                m_fastSpringBoneService.BufferCombiner.Register(m_fastSpringBoneBuffer);
            }

        }

        public void Dispose()
        {
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
            m_fastSpringBoneBuffer = CreateFastSpringBoneBuffer(m_instance.SpringBone);

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
                            tailOrNormal = collider.TailOrNormal,
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
                case VRM10SpringBoneColliderTypes.Plane:
                    return BlittableColliderType.Plane;
                case VRM10SpringBoneColliderTypes.SphereInside:
                    return BlittableColliderType.SphereInside;
                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    return BlittableColliderType.CapsuleInside;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}