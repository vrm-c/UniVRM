using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniHumanoid
{
    public static class HumanoidLoader
    {
        public static Avatar LoadHumanoidAvatar(Transform root, IEnumerable<(Transform, HumanBodyBones)> boneMap)
        {
            ForceUniqueName.Process(root);

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
    }
}
