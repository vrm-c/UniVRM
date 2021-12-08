using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

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
                .SelectMany(x => x.GetComponents<VRMSpringBone>())
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
                        .Select(x => colliders.IndexOf(x))
                        .Where(x => x != -1)
                        .ToArray(),
                    bones = spring.RootBones.Select(x => nodes.IndexOf(x)).ToArray(),
                });
            }
        }

        public static void LoadSecondary(Transform root, List<Transform> nodes,
            glTF_VRM_SecondaryAnimation secondaryAnimation)
        {
            var secondary = root.Find("secondary");
            if (secondary == null)
            {
                secondary = new GameObject("secondary").transform;
                secondary.SetParent(root, false);
            }

            // clear components
            var vrmSpringBones = root.GetComponentsInChildren<VRMSpringBone>();
            var vrmSpringBoneColliderGroup = root.GetComponentsInChildren<VRMSpringBoneColliderGroup>();

            var length = (vrmSpringBones?.Length ?? 0) + (vrmSpringBoneColliderGroup?.Length ?? 0);
            var remove = new Component[length];

            var index = 0;
            if (vrmSpringBones != null)
            {
                foreach (var vrmSpringBone in vrmSpringBones)
                {
                    remove[index++] = vrmSpringBone;
                }
            }

            if (vrmSpringBoneColliderGroup != null)
            {
                foreach (var vrmSpringBoneCollider in vrmSpringBoneColliderGroup)
                {
                    remove[index++] = vrmSpringBoneCollider;
                }
            }

            foreach (var x in remove)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(x);
                }
                else
                {
                    GameObject.DestroyImmediate(x);
                }
            }

            //var secondaryAnimation = context.VRM.extensions.VRM.secondaryAnimation;
            var colliders = new List<VRMSpringBoneColliderGroup>();
            foreach (var colliderGroup in secondaryAnimation.colliderGroups)
            {
                var vrmGroup = nodes[colliderGroup.node].gameObject.AddComponent<VRMSpringBoneColliderGroup>();
                vrmGroup.Colliders = colliderGroup.colliders.Select(x =>
                {
                    return new VRMSpringBoneColliderGroup.SphereCollider
                    {
                        Offset = x.offset,
                        Radius = x.radius
                    };
                }).ToArray();
                colliders.Add(vrmGroup);
            }

            if (secondaryAnimation.boneGroups.Count > 0)
            {
                foreach (var boneGroup in secondaryAnimation.boneGroups)
                {
                    var vrmBoneGroup = secondary.gameObject.AddComponent<VRMSpringBone>();
                    if (boneGroup.center != -1)
                    {
                        vrmBoneGroup.m_center = nodes[boneGroup.center];
                    }

                    vrmBoneGroup.m_comment = boneGroup.comment;
                    vrmBoneGroup.m_dragForce = boneGroup.dragForce;
                    vrmBoneGroup.m_gravityDir = boneGroup.gravityDir;
                    vrmBoneGroup.m_gravityPower = boneGroup.gravityPower;
                    vrmBoneGroup.m_hitRadius = boneGroup.hitRadius;
                    vrmBoneGroup.m_stiffnessForce = boneGroup.stiffiness;

                    if (boneGroup.colliderGroups != null && boneGroup.colliderGroups.Any())
                    {
                        vrmBoneGroup.ColliderGroups = new VRMSpringBoneColliderGroup[boneGroup.colliderGroups.Length];
                        for (int i = 0; i < boneGroup.colliderGroups.Length; ++i)
                        {
                            var colliderGroup = boneGroup.colliderGroups[i];
                            vrmBoneGroup.ColliderGroups[i] = colliders[colliderGroup];
                        }
                    }

                    var boneList = new List<Transform>();
                    foreach (var x in boneGroup.bones)
                    {
                        boneList.Add(nodes[x]);
                    }

                    vrmBoneGroup.RootBones = boneList;
                }
            }
            else
            {
                secondary.gameObject.AddComponent<VRMSpringBone>();
            }
        }
    }
}