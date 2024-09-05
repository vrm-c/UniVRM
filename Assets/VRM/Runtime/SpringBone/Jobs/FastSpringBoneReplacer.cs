using System;
using System.Linq;
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

            var springs = new List<FastSpringBoneSpring>();
            foreach (var component in components)
            {
                if (awaitCaller != null)
                {
                    await awaitCaller.NextFrame();
                    token.ThrowIfCancellationRequested();
                }

                var colliders = new List<FastSpringBoneCollider>();
                foreach (var group in component.ColliderGroups)
                {
                    if (group == null) continue;
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
                    foreach (var joint in Traverse(component, springRoot))
                    {
                        joints.Add(joint);
                    }
                }

                var spring = new FastSpringBoneSpring
                {
                    center = component.m_center,
                    colliders = colliders.ToArray(),
                    joints = joints.ToArray(),
                };
                springs.Add(spring);
            }

            return new FastSpringBoneBuffer(springs);
        }

        static IEnumerable<FastSpringBoneJoint> Traverse(VRMSpringBone spring, Transform joint)
        {
            yield return new FastSpringBoneJoint
            {
                Transform = joint.transform,
                Joint = new BlittableJoint
                {
                    radius = spring.m_hitRadius,
                    dragForce = spring.m_dragForce,
                    gravityDir = spring.m_gravityDir,
                    gravityPower = spring.m_gravityPower,
                    stiffnessForce = spring.m_stiffnessForce
                },
                DefaultLocalRotation = joint.transform.localRotation,
            };
            foreach (Transform child in joint)
            {
                foreach (var x in Traverse(spring, child))
                {
                    yield return x;
                }
            }

            // TODO: 詳細分岐

            // 7cm tail

            //     var springRootBones =
            //     (
            //         from springBone in springBones
            //         from rootBone in springBone.RootBones
            //         select (springBone, rootBone)
            //     ).ToList();

            //     for (var i = 0; i < springRootBones.Count; i++)
            //     {
            //         var current = springRootBones[i];

            //         // 他のRootBoneのどれかが、自分の親（もしくは同じTransform）なら自分自身を削除する
            //         if (springRootBones
            //             .Where(other => other != current)
            //             .Any(other => current.rootBone.IsChildOf(other.rootBone)))
            //         {
            //             springRootBones.RemoveAt(i);
            //             --i;
            //         }
            //     }            
        }
    }
}