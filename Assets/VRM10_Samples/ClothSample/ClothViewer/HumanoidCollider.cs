using System.Collections.Generic;
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

        public static void AddColliders(Animator animator)
        {
            Dictionary<string, VRM10SpringBoneColliderGroup> map = new();

            foreach (var (group, _head, _tail, radius) in Capsules)
            {
                if (!map.ContainsKey(group))
                {
                    var g = animator.gameObject.AddComponent<VRM10SpringBoneColliderGroup>();
                    map.Add(group, g);
                }


                var head = animator.GetBoneTransform(_head);
                var vrmCollider = head.gameObject.AddComponent<VRM10SpringBoneCollider>();
                if (vrmCollider != null)
                {
                    vrmCollider.Radius = radius;
                    vrmCollider.ColliderType = VRM10SpringBoneColliderTypes.Capsule;
                    var tail = animator.GetBoneTransform(_tail);
                    vrmCollider.Tail = head.worldToLocalMatrix.MultiplyPoint(tail.position);

                    map[group].Colliders.Add(vrmCollider);    
                }
            }
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