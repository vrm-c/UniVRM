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
        private FastSpringBoneSpring[] m_springs;
        private Quaternion[] m_initialLocalRotations;
        private FastSpringBoneBuffer m_fastSpringBoneBuffer;
        public Vector3 ExternalForce
        {
            get => m_fastSpringBoneBuffer.ExternalForce;
            set => m_fastSpringBoneBuffer.ExternalForce = value;
        }
        public bool IsSpringBoneEnabled
        {
            get => m_fastSpringBoneBuffer.IsSpringBoneEnabled;
            set => m_fastSpringBoneBuffer.IsSpringBoneEnabled = value;
        }

        internal Vrm10RuntimeSpringBone(Vrm10Instance instance)
        {
            m_instance = instance;

            if (instance.TryGetComponent<RuntimeGltfInstance>(out var gltfInstance))
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
                ReconstructSpringBone();
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
            // rerelase
            if (m_fastSpringBoneBuffer != null)
            {
                m_fastSpringBoneService.BufferCombiner.Unregister(m_fastSpringBoneBuffer);
                m_fastSpringBoneBuffer.Dispose();
            }

            // create(Spring情報の再収集。設定変更の反映)
            m_springs = m_instance.SpringBone.Springs.Select(spring => new FastSpringBoneSpring
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
            }).ToArray();

            // DOTS buffer 構築
            m_fastSpringBoneBuffer = new FastSpringBoneBuffer(m_springs);
            m_fastSpringBoneService.BufferCombiner.Register(m_fastSpringBoneBuffer);
            // reset 用の初期状態の記録
            m_initialLocalRotations = m_fastSpringBoneBuffer.Transforms.Select(x => x.localRotation).ToArray();
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

        public void RestoreInitialTransform()
        {
            // Spring の joint に対応する transform の回転を初期状態
            for (int i = 0; i < m_fastSpringBoneBuffer.Transforms.Length; ++i)
            {
                var transform = m_fastSpringBoneBuffer.Transforms[i];
                transform.localRotation = m_initialLocalRotations[i];
            }

            // 初期状態にしたtransformを使って spring logic を構築
            List<BlittableLogic> blittableLogics = new();
            foreach (var spring in m_springs)
            {
                blittableLogics.AddRange(
                    FastSpringBoneBuffer.LogicFromTransform(m_fastSpringBoneBuffer.Transforms, spring));
            }
            // DOTS バッファーを更新
            m_fastSpringBoneBuffer.SyncAndZeroVelocity(blittableLogics);
        }
    }
}