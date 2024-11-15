using System.Collections;
using System.Collections.Generic;
using RotateParticle;
using UnityEngine;

namespace UniVRM10.Cloth.Viewer
{
    public static class HumanCollider
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

        public static void AddColliders(RotateParticleSystem _system, Animator animator)
        {
            foreach (var (group, head, tail, radius) in Capsules)
            {
                _system.AddColliderIfNotExists(group,
                    animator.GetBoneTransform(head), animator.GetBoneTransform(tail), radius);
            }
        }
    }
}