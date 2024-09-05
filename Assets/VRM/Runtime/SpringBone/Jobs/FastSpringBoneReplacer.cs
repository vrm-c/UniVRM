using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF.SpringBoneJobs.InputPorts;
using UniGLTF.SpringBoneJobs.Blittables;


namespace VRM.SpringBoneJobs
{
    public static class FastSpringBoneReplacer
    {
        /// <summary>
        /// - 指定された GameObject 内にある SpringBone を停止させる
        /// - FastSpringBoneBuffer に変換する
        /// </summary>
        public static FastSpringBoneBuffer MakeBuffer(GameObject root)
        {
            var components = root.GetComponentsInChildren<VRMSpringBone>();
            foreach (var sb in components)
            {
                // 停止させて FastSpringBoneService から実行させる
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.Manual;
            }

            // create(Spring情報の再収集。設定変更の反映)
            var springs = components.Select(spring => new FastSpringBoneSpring
            {
                center = spring.m_center,
                colliders = spring.ColliderGroups
                   .SelectMany(group => group.Colliders.Select(collider => new FastSpringBoneCollider
                   {
                       Transform = group.transform,
                       Collider = new BlittableCollider
                       {
                           offset = collider.Offset,
                           radius = collider.Radius,
                           tailOrNormal = default,
                           colliderType = BlittableColliderType.Sphere
                       }
                   })).ToArray(),
                joints = spring.RootBones.Select(x => Traverse(spring, x)).SelectMany(x => x).ToArray(),
            }).ToArray();

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