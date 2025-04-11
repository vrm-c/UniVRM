using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UniGLTF.Utils;
using UniHumanoid;
using UnityEngine;


namespace VRM
{
    public static class VRMBoneNormalizer
    {
        public static void EnforceTPose(GameObject go)
        {
            var animator = go.GetComponentOrThrow<Animator>();

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
        /// 
        /// v0.115 ヒエラルキーのコピーをしなくまりました(仕様変更)
        /// v0.116 Animator.avatar 代入の副作用回避修正
        ///
        /// v0.114以前: 非破壊
        ///    - return コピーされて正規化されたヒエラルキー
        /// v0.115以降: 対象のヒエラルキーが正規化されます。
        ///    - Transform が変更されます。
        ///    - Animator.avatar が差し替えられます。
        ///    - SkinnedMeshRenderer.sharedMesh が差し替えられます。
        ///    - MeshFilter.sharedMesh が差し替えられます。
        ///    - return void
        /// </summary>
        /// <param name="go">対象モデルのルート</param>
        /// <param name="forceTPose">強制的にT-Pose化するか</param>
        /// <param name="useCurrentBlendShapeWeight">BlendShape の現状をbakeするか</param>
        public static void Execute(GameObject go, bool forceTPose, bool useCurrentBlendShapeWeight)
        {
            if (forceTPose)
            {
                // T-Poseにする
                var hips = go.GetComponentOrThrow<Animator>().GetBoneTransform(HumanBodyBones.Hips);
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

            // Transform の回転とスケールを Mesh に適用します。
            // - 回転とスケールが反映された新しい Mesh が作成されます
            // - Transform の回転とスケールはクリアされます。world position を維持します
            var newMeshMap = BoneNormalizer.NormalizeHierarchyFreezeMesh(go, useCurrentBlendShapeWeight);

            // SkinnedMeshRenderer.sharedMesh と MeshFilter.sharedMesh を新しいMeshで置き換える
            BoneNormalizer.Replace(go, newMeshMap, false);

            // 回転とスケールが除去された新しいヒエラルキーからAvatarを作る
            if (go.TryGetComponent<Animator>(out var animator))
            {
                HumanoidLoader.RebuildHumanAvatar(animator);
            }
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
            // blendshape
            {
                if (go.TryGetComponent<VRMBlendShapeProxy>(out var src))
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
                        var ls = src.transform.UniformedLossyScale();
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
                            .Select(x => map[x.transform].GetComponentOrThrow<VRMSpringBoneColliderGroup>()).ToArray();
                    }
                }
            }

#pragma warning disable 0618
            {
                // meta(obsolete)
                if (go.TryGetComponent<VRMMetaInformation>(out var src))
                {
                    src.CopyTo(root);
                }
            }
#pragma warning restore 0618

            {
                // meta
                if (go.TryGetComponent<VRMMeta>(out var src))
                {
                    var dst = root.AddComponent<VRMMeta>();
                    dst.Meta = src.Meta;
                }
            }

            {
                // firstPerson
                if (go.TryGetComponent<VRMFirstPerson>(out var src))
                {
                    src.CopyTo(root, map);
                }
            }
            {
                // look at
                if (go.TryGetComponent<VRMLookAtHead>(out var src))
                {
                    var dst = root.AddComponent<VRMLookAtHead>();
                }
            }
            {
                // bone applier
                if (go.TryGetComponent<VRMLookAtBoneApplyer>(out var src))
                {
                    var dst = root.AddComponent<VRMLookAtBoneApplyer>();
                    dst.HorizontalInner.Assign(src.HorizontalInner);
                    dst.HorizontalOuter.Assign(src.HorizontalOuter);
                    dst.VerticalUp.Assign(src.VerticalUp);
                    dst.VerticalDown.Assign(src.VerticalDown);
                }
            }
            {
                // blendshape applier
                if (go.TryGetComponent<VRMLookAtBlendShapeApplyer>(out var src))
                {
                    var dst = root.AddComponent<VRMLookAtBlendShapeApplyer>();
                    dst.Horizontal.Assign(src.Horizontal);
                    dst.VerticalUp.Assign(src.VerticalUp);
                    dst.VerticalDown.Assign(src.VerticalDown);
                }
            }

            {
                // humanoid
                var dst = root.AddComponent<VRMHumanoidDescription>();
                if (go.TryGetComponent<VRMHumanoidDescription>(out var src))
                {
                    dst.Avatar = src.Avatar;
                    dst.Description = src.Description;
                }
                else
                {
                    if (go.TryGetComponent<Animator>(out var animator))
                    {
                        dst.Avatar = animator.avatar;
                    }
                }
            }
        }
    }
}
