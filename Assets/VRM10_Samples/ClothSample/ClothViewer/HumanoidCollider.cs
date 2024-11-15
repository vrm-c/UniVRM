using System.Collections;
using System.Collections.Generic;
using RotateParticle;
using SphereTriangle;
using UnityEngine;

namespace UniVRM10.Cloth.Viewer
{
    public static class HumanoidCollider
    {
        static (string group, HumanBodyBones head, HumanBodyBones tail, float radius)[] Capsules = new[]
        {
            ("Leg", HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, 0.06f),
            ("Leg", HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, 0.05f),
            ("Leg", HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, 0.06f),
            ("Leg", HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, 0.05f),

            ("Arm", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, 0.03f),
            ("Arm", HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, 0.03f),
            ("Arm", HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal, 0.02f),
            ("Arm", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, 0.03f),
            ("Arm", HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, 0.03f),
            ("Arm", HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal, 0.02f),
        };

        public static void AddColliders(RotateParticleSystem system, Animator animator)
        {
            foreach (var (group, head, tail, radius) in Capsules)
            {
                AddColliderIfNotExists(system._colliderGroups, group,
                    animator.GetBoneTransform(head), animator.GetBoneTransform(tail), radius);
            }
        }

        static void AddColliderIfNotExists(List<ColliderGroup> _colliderGroups, string groupName, Transform head, Transform tail, float radius)
        {
            ColliderGroup group = default;
            foreach (var g in _colliderGroups)
            {
                if (g.Name == groupName)
                {
                    group = g;
                    break;
                }
            }
            if (group == null)
            {
                group = new ColliderGroup { Name = groupName };
                _colliderGroups.Add(group);
            }

            foreach (var collider in group.Colliders)
            {
                if (collider.transform == head)
                {
                    return;
                }
            }

            var c = GetOrAddComponent<SphereCapsuleCollider>(head.gameObject);
            c.Tail = tail;
            c.Radius = radius;
            // c.GizmoColor = GetGizmoColor(group);
            group.Colliders.Add(c);
        }

        static T GetOrAddComponent<T>(GameObject o) where T : Component
        {
            var t = o.GetComponent<T>();
            if (t != null)
            {
                return t;
            }
            return o.AddComponent<T>();
        }

    }
}