using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.Utils;
using UnityEngine;

namespace UniHumanoid
{
    using BoneMap = IEnumerable<(Transform, HumanBodyBones)>;

    public static class HumanoidLoader
    {
        public static Avatar BuildHumanAvatarFromMap(Transform root, BoneMap boneMap)
        {
            var description = new HumanDescription
            {
                skeleton = root.GetComponentsInChildren<Transform>()
                    .Select(x => x.ToSkeletonBone()).ToArray(),
                human = boneMap
                    .Select(x => new HumanBone
                    {
                        boneName = x.Item1.name,
                        humanName = s_humanTranitBoneNameMap[x.Item2],
                        limit = new HumanLimit
                        {
                            useDefaultValues = true,
                        }
                    }).ToArray(),

                armStretch = 0.05f,
                legStretch = 0.05f,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                feetSpacing = 0,
                hasTranslationDoF = false,
            };

            return AvatarBuilder.BuildHumanAvatar(root.gameObject, description);
        }

        static HumanBodyBones TraitToHumanBone(string x)
        {
            return (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), x.Replace(" ", ""), true);
        }

        static readonly Dictionary<HumanBodyBones, string> s_humanTranitBoneNameMap =
        HumanTrait.BoneName.ToDictionary(
            x => TraitToHumanBone(x),
            x => x);

        /// <summary>
        /// Avatar を保持する既存の Animatorヒエラルキーの Transform を変更したのちに、
        /// HumanBone のマッピングを流用して、新たな Avatar を作り直す。
        /// 古い Avatar は破棄する。
        /// </summary>
        public static void RebuildHumanAvatar(Animator animator)
        {
            if (animator == null)
            {
                throw new ArgumentNullException("src");
            }

            var target = animator.gameObject;

            var map = CachedEnum.GetValues<HumanBodyBones>()
                .Where(x => x != HumanBodyBones.LastBone)
                .Select(x => (animator.GetBoneTransform(x), x))
                .Where(x => x.Item1 != null)
                ;
            var newAvatar = BuildHumanAvatarFromMap(animator.transform, map);
            newAvatar.name = "re-created";

            // var newAvatar = LoadHumanoidAvatarFromAnimator(animator);
            // Animator.avatar を代入したときに副作用でTransformが変更されるのを回避するために削除します。
            if (Application.isPlaying)
            {
                GameObject.Destroy(animator);
            }
            else
            {
                GameObject.DestroyImmediate(animator);
            }
            // 新たに AddComponent する
            target.AddComponent<Animator>().avatar = newAvatar;
        }
    }
}