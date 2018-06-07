using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public static class VRMSpringUtility
    {
        public static void ExportSecondary(Transform root, List<Transform> nodes,
            Action<glTF_VRM_SecondaryAnimationColliderGroup> addSecondaryColliderGroup,
            Action<glTF_VRM_SecondaryAnimationGroup> addSecondaryGroup)
        {
            var colliders = new List<VRMSpringBoneColliderGroup>();
            foreach (var vrmColliderGroup in root.Traverse()
                .Select(x => x.GetComponent<VRMSpringBoneColliderGroup>())
                .Where(x => x != null))
            {
                colliders.Add(vrmColliderGroup);

                var colliderGroup = new glTF_VRM_SecondaryAnimationColliderGroup
                {
                    node = nodes.IndexOf(vrmColliderGroup.transform)
                };

                colliderGroup.colliders = vrmColliderGroup.Colliders.Select(x =>
                {
                    return new glTF_VRM_SecondaryAnimationCollider
                    {
                        offset = x.Offset,
                        radius = x.Radius,
                    };

                }).ToList();

                addSecondaryColliderGroup(colliderGroup);
            }

            foreach (var spring in root.Traverse()
                .Select(x => x.GetComponent<VRMSpringBone>())
                .Where(x => x != null))
            {
                addSecondaryGroup(new glTF_VRM_SecondaryAnimationGroup
                {
                    comment = spring.m_comment,
                    center = nodes.IndexOf(spring.m_center),
                    dragForce = spring.m_dragForce,
                    gravityDir = spring.m_gravityDir,
                    gravityPower = spring.m_gravityPower,
                    stiffiness = spring.m_stiffnessForce,
                    hitRadius = spring.m_hitRadius,
                    colliderGroups = spring.ColliderGroups
                        .Select(x =>
                        {
                            var index = colliders.IndexOf(x);
                            if (index == -1)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            return index;
                        })
                        .ToArray(),
                    bones = spring.RootBones.Select(x => nodes.IndexOf(x)).ToArray(),
                });
            }
        }
    }
}
