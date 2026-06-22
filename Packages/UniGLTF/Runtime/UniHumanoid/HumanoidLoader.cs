using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
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
        /// Recreates an animiator's humanoid avatar. 
        /// The old Avatar is discarded.
        /// </summary>
        public static void RebuildHumanAvatar(Animator animator)
        {
            var task = RebuildHumanAvatarAsync(animator, new ImmediateCaller());
            if (!task.IsCompleted)
            {
                throw new Exception("task not completed");
            }
        }

        public static async Task RebuildHumanAvatarAsync(Animator animator, IAwaitCaller awaitCaller)
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
            // 1. Delete this to avoid changing Transform as a side effect when assigning Animator.avatar.
            if (Application.isPlaying)
            {
                GameObject.Destroy(animator);

                // https://github.com/vrm-c/UniVRM/pull/2764
                // Require IAwaitCaller that has NextFrame capability. RuntimeOnlyAwaitCaller etc. not ImmediateCaller.
                // Else, the following AddComponent call will fail.
                await awaitCaller.NextFrame();
            }
            else
            {
                GameObject.DestroyImmediate(animator);
            }

            // 2. Attach a new one
            target.AddComponent<Animator>().avatar = newAvatar;
        }
    }
}