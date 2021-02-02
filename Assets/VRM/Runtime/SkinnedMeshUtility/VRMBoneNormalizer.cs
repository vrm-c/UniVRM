using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;


namespace VRM
{
    public static class VRMBoneNormalizer
    {
        public static void EnforceTPose(GameObject go)
        {
            var animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                throw new ArgumentException("Animator with avatar is required");
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                throw new ArgumentException("avatar is required");
            }

            if (!avatar.isValid)
            {
                throw new ArgumentException("invalid avatar");
            }

            if (!avatar.isHuman)
            {
                throw new ArgumentException("avatar is not human");
            }

            HumanPoseTransfer.SetTPose(avatar, go.transform);
        }

        /// <summary>
        /// モデルの正規化を実行する
        /// </summary>
        /// <param name="go">対象モデルのルート</param>
        /// <param name="forceTPose">強制的にT-Pose化するか</param>
        /// <returns>正規化済みのモデル</returns>
        public static GameObject Execute(GameObject go, bool forceTPose)
        {
            //
            // T-Poseにする
            //
            if (forceTPose)
            {
                var hips = go.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                var hipsPosition = hips.position;
                var hipsRotation = hips.rotation;
                try
                {
                    EnforceTPose(go);
                }
                finally
                {
                    hips.position = hipsPosition; // restore hipsPosition
                    hips.rotation = hipsRotation;
                }
            }

            //
            // 正規化されたヒエラルキーを作る
            //
            var (normalized, bMap) = MeshUtility.BoneNormalizer.Execute(go, (_src, dst, boneMap) =>
            {
                var src = _src.GetComponent<Animator>();

                var srcHumanBones = Enum.GetValues(typeof(HumanBodyBones))
                    .Cast<HumanBodyBones>()
                    .Where(x => x != HumanBodyBones.LastBone)
                    .Select(x => new { Key = x, Value = src.GetBoneTransform(x) })
                    .Where(x => x.Value != null)
                    ;

                var map =
                       srcHumanBones
                       .Where(x => boneMap.ContainsKey(x.Value))
                       .ToDictionary(x => x.Key, x => boneMap[x.Value])
                       ;

                if (dst.GetComponent<Animator>() == null)
                {
                    var animator = dst.AddComponent<Animator>();
                }
                var vrmHuman = go.GetComponent<VRMHumanoidDescription>();
                var avatarDescription = AvatarDescription.Create();
                if (vrmHuman != null && vrmHuman.Description != null)
                {
                    avatarDescription.armStretch = vrmHuman.Description.armStretch;
                    avatarDescription.legStretch = vrmHuman.Description.legStretch;
                    avatarDescription.upperArmTwist = vrmHuman.Description.upperArmTwist;
                    avatarDescription.lowerArmTwist = vrmHuman.Description.lowerArmTwist;
                    avatarDescription.upperLegTwist = vrmHuman.Description.upperLegTwist;
                    avatarDescription.lowerLegTwist = vrmHuman.Description.lowerLegTwist;
                    avatarDescription.feetSpacing = vrmHuman.Description.feetSpacing;
                    avatarDescription.hasTranslationDoF = vrmHuman.Description.hasTranslationDoF;
                }
                avatarDescription.SetHumanBones(map);
                var avatar = avatarDescription.CreateAvatar(dst.transform);
                return avatar;
            });

            CopyVRMComponents(go, normalized, bMap);

            return normalized;
        }

        /// <summary>
        /// VRMを構成するコンポーネントをコピーする。
        /// </summary>
        /// <param name="go">コピー元</param>
        /// <param name="root">コピー先</param>
        /// <param name="map">コピー元とコピー先の対応関係</param>
        static void CopyVRMComponents(GameObject go, GameObject root,
            Dictionary<Transform, Transform> map)
        {
            {
                // blendshape
                var src = go.GetComponent<VRMBlendShapeProxy>();
                if (src != null)
                {
                    var dst = root.AddComponent<VRMBlendShapeProxy>();
                    dst.BlendShapeAvatar = src.BlendShapeAvatar;
                }
            }

            {
                // springbone
                var secondary = go.transform.Find("secondary");
                if (secondary == null)
                {
                    secondary = go.transform;
                }

                var dstSecondary = root.transform.Find("secondary");
                if (dstSecondary == null)
                {
                    dstSecondary = new GameObject("secondary").transform;
                    dstSecondary.SetParent(root.transform, false);
                }

                // VRMSpringBoneColliderGroup
                foreach (var src in go.transform.GetComponentsInChildren<VRMSpringBoneColliderGroup>())
                {
                    var dst = map[src.transform];
                    var dstColliderGroup = dst.gameObject.AddComponent<VRMSpringBoneColliderGroup>();
                    dstColliderGroup.Colliders = src.Colliders.Select(y =>
                    {
                        var offset = dst.worldToLocalMatrix.MultiplyPoint(src.transform.localToWorldMatrix.MultiplyPoint(y.Offset));
                        var ls = src.UniformedLossyScale;
                        return new VRMSpringBoneColliderGroup.SphereCollider
                        {
                            Offset = offset,
                            Radius = y.Radius * ls
                        };
                    }).ToArray();
                }

                // VRMSpringBone
                foreach (var src in go.transform.GetComponentsInChildren<VRMSpringBone>())
                {
                    var dst = dstSecondary.gameObject.AddComponent<VRMSpringBone>();
                    dst.m_comment = src.m_comment;
                    dst.m_stiffnessForce = src.m_stiffnessForce;
                    dst.m_gravityPower = src.m_gravityPower;
                    dst.m_gravityDir = src.m_gravityDir;
                    dst.m_dragForce = src.m_dragForce;
                    if (src.m_center != null)
                    {
                        dst.m_center = map[src.m_center];
                    }

                    dst.RootBones = src.RootBones.Select(x => map[x]).ToList();
                    dst.m_hitRadius = src.m_hitRadius;
                    if (src.ColliderGroups != null)
                    {
                        dst.ColliderGroups = src.ColliderGroups
                            .Select(x => map[x.transform].GetComponent<VRMSpringBoneColliderGroup>()).ToArray();
                    }
                }
            }

#pragma warning disable 0618
            {
                // meta(obsolete)
                var src = go.GetComponent<VRMMetaInformation>();
                if (src != null)
                {
                    src.CopyTo(root);
                }
            }
#pragma warning restore 0618

            {
                // meta
                var src = go.GetComponent<VRMMeta>();
                if (src != null)
                {
                    var dst = root.AddComponent<VRMMeta>();
                    dst.Meta = src.Meta;
                }
            }

            {
                // firstPerson
                var src = go.GetComponent<VRMFirstPerson>();
                if (src != null)
                {
                    src.CopyTo(root, map);
                }
            }

            {
                // humanoid
                var dst = root.AddComponent<VRMHumanoidDescription>();
                var src = go.GetComponent<VRMHumanoidDescription>();
                if (src != null)
                {
                    dst.Avatar = src.Avatar;
                    dst.Description = src.Description;
                }
                else
                {
                    var animator = go.GetComponent<Animator>();
                    if (animator != null)
                    {
                        dst.Avatar = animator.avatar;
                    }
                }
            }
        }
    }
}
