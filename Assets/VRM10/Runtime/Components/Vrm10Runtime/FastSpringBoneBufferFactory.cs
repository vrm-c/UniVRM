using System;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UniGLTF.SpringBoneJobs.InputPorts;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public static class FastSpringBoneBufferFactory
    {
        /// <summary>
        /// このVRMに紐づくSpringBone関連のバッファを構築する。
        /// </summary>
        /// <param name="awaitCaller"></param>
        /// <param name="fastSpringBoneBuffer">TODO: 再利用する</param>
        /// <returns></returns>
        public static async Task<FastSpringBoneBuffer> ConstructSpringBoneAsync(IAwaitCaller awaitCaller, Vrm10Instance vrm,
            FastSpringBoneBuffer fastSpringBoneBuffer = null)
        {
            // TODO: Dispose せずに再利用する最適化
            // new FastSpringBoneBuffer にも構築ロジックがあるので合体して整理する必要あり。
            // GC 軽減と await 挟み込み
            if (fastSpringBoneBuffer != null)
            {
                fastSpringBoneBuffer.Dispose();
            }

            Func<Transform, TransformState> GetOrAddDefaultTransformState = (Transform tf) =>
            {
                if (vrm.DefaultTransformStates.TryGetValue(tf, out var defaultTransformState))
                {
                    return defaultTransformState;
                }

                Debug.LogWarning($"{tf.name} does not exist on load.");
                return new TransformState(null);
            };

            // create(Spring情報の再収集。設定変更の反映)
            var springs = vrm.SpringBone.Springs.Select(spring => new FastSpringBoneSpring
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
                       Joint = new BlittableJointMutable
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

            await awaitCaller.NextFrame();

            fastSpringBoneBuffer = new FastSpringBoneBuffer(vrm.transform, springs);
            return fastSpringBoneBuffer;
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