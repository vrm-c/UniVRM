using System.Collections.Generic;
using UnityEngine;
using UniGLTF.SpringBoneJobs.InputPorts;
using UniGLTF.SpringBoneJobs.Blittables;
using UniGLTF;
using System.Threading;
using System.Threading.Tasks;


namespace VRM.SpringBoneJobs
{
    public static class FastSpringBoneReplacer
    {
        /// <summary>
        /// - 指定された GameObject 内にある SpringBone を停止させる
        /// - FastSpringBoneBuffer に変換する
        /// </summary>
        public static async Task<FastSpringBoneBuffer> MakeBufferAsync(GameObject root, IAwaitCaller awaitCaller = null, CancellationToken token = default)
        {
            var components = root.GetComponentsInChildren<VRMSpringBone>();
            foreach (var sb in components)
            {
                // 停止させて FastSpringBoneService から実行させる
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.Manual;
            }

            var springs = new FastSpringBoneSpring[components.Length];
            for (int i = 0; i < components.Length; ++i)
            {
                var component = components[i];
                var colliders = new List<FastSpringBoneCollider>();
                foreach (var group in component.ColliderGroups)
                {
                    if (group == null) continue;
                    if (awaitCaller != null)
                    {
                        await awaitCaller.NextFrame();
                        token.ThrowIfCancellationRequested();
                    }

                    foreach (var collider in group.Colliders)
                    {
                        if (collider == null) continue;

                        var c = new FastSpringBoneCollider
                        {
                            Transform = group.transform,
                            Collider = new BlittableCollider
                            {
                                offset = collider.Offset,
                                radius = collider.Radius,
                                tailOrNormal = default,
                                colliderType = BlittableColliderType.Sphere
                            }
                        };
                        colliders.Add(c);
                    }
                }

                var joints = new List<FastSpringBoneJoint>();
                foreach (var springRoot in component.RootBones)
                {
                    if (springRoot == null) continue;
                    if (awaitCaller != null)
                    {
                        await awaitCaller.NextFrame();
                        token.ThrowIfCancellationRequested();
                    }

                    Traverse(joints, component, springRoot);
                }

                var spring = new FastSpringBoneSpring
                {
                    center = component.m_center,
                    colliders = colliders.ToArray(),
                    joints = joints.ToArray(),
                };
                springs[i] = spring;
            }

            return new FastSpringBoneBuffer(springs);
        }

        static void Traverse(List<FastSpringBoneJoint> joints, VRMSpringBone spring, Transform joint)
        {
            joints.Add(new FastSpringBoneJoint
            {
                Transform = joint.transform,
                Joint = new BlittableJointMutable
                {
                    radius = spring.m_hitRadius,
                    dragForce = spring.m_dragForce,
                    gravityDir = spring.m_gravityDir,
                    gravityPower = spring.m_gravityPower,
                    stiffnessForce = spring.m_stiffnessForce
                },
                DefaultLocalRotation = joint.transform.localRotation,
            });
            foreach (Transform child in joint)
            {
                Traverse(joints, spring, child);
            }
        }
    }
}