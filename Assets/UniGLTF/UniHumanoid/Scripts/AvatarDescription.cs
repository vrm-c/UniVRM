#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


namespace UniHumanoid
{
    [Serializable]
    public struct BoneLimit
    {
        public HumanBodyBones humanBone;
        public string boneName;
        public bool useDefaultValues;
        public Vector3 min;
        public Vector3 max;
        public Vector3 center;
        public float axisLength;
        private static string[] cashedHumanTraitBoneName = null;

        public static BoneLimit From(HumanBone bone)
        {
            return new BoneLimit
            {
                humanBone = (HumanBodyBones) Enum.Parse(typeof(HumanBodyBones), bone.humanName.Replace(" ", ""), true),
                boneName = bone.boneName,
                useDefaultValues = bone.limit.useDefaultValues,
                min = bone.limit.min,
                max = bone.limit.max,
                center = bone.limit.center,
                axisLength = bone.limit.axisLength,
            };
        }

        public static String ToHumanBoneName(HumanBodyBones b)
        {
            // 呼び出し毎にGCが発生するのでキャッシュする
            if (cashedHumanTraitBoneName == null)
            {
                cashedHumanTraitBoneName = HumanTrait.BoneName;
            }

            foreach (var x in cashedHumanTraitBoneName)
            {
                if (x.Replace(" ", "") == b.ToString())
                {
                    return x;
                }
            }

            throw new KeyNotFoundException();
        }

        public HumanBone ToHumanBone()
        {
            return new HumanBone
            {
                boneName = boneName,
                humanName = ToHumanBoneName(humanBone),
                limit = new HumanLimit
                {
                    useDefaultValues = useDefaultValues,
                    axisLength = axisLength,
                    center = center,
                    max = max,
                    min = min
                },
            };
        }
    }

    [Serializable]
    public class AvatarDescription : ScriptableObject
    {
        public float armStretch = 0.05f;
        public float legStretch = 0.05f;
        public float upperArmTwist = 0.5f;
        public float lowerArmTwist = 0.5f;
        public float upperLegTwist = 0.5f;
        public float lowerLegTwist = 0.5f;
        public float feetSpacing = 0;
        public bool hasTranslationDoF;
        public BoneLimit[] human;

        public HumanDescription ToHumanDescription(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>();
            var skeletonBones = new SkeletonBone[transforms.Length];
            var index = 0;
            foreach (var t in transforms)
            {
                skeletonBones[index] = t.ToSkeletonBone();
                index++;
            }

            var humanBones = new HumanBone[human.Length];
            index = 0;
            foreach (var bonelimit in human)
            {
                humanBones[index] = bonelimit.ToHumanBone();
                index++;
            }


            return new HumanDescription
            {
                skeleton = skeletonBones,
                human = humanBones,
                armStretch = armStretch,
                legStretch = legStretch,
                upperArmTwist = upperArmTwist,
                lowerArmTwist = lowerArmTwist,
                upperLegTwist = upperLegTwist,
                lowerLegTwist = lowerLegTwist,
                feetSpacing = feetSpacing,
                hasTranslationDoF = hasTranslationDoF,
            };
        }

        public Avatar CreateAvatar(Transform root)
        {
            return AvatarBuilder.BuildHumanAvatar(root.gameObject, ToHumanDescription(root));
        }

        public Avatar CreateAvatarAndSetup(Transform root)
        {
            var avatar = CreateAvatar(root);
            avatar.name = name;

            var animator = root.GetComponent<Animator>();
            if (animator != null)
            {
                var positionMap = root.Traverse().ToDictionary(x => x, x => x.position);
                animator.avatar = avatar;
                foreach (var x in root.Traverse())
                {
                    x.position = positionMap[x];
                }
            }

            var transfer = root.GetComponent<HumanPoseTransfer>();
            if (transfer != null)
            {
                transfer.Avatar = avatar;
            }

            return avatar;
        }

#if UNITY_EDITOR
        public static AvatarDescription CreateFrom(Avatar avatar)
        {
            var description = default(HumanDescription);
            if (!GetHumanDescription(avatar, ref description))
            {
                return null;
            }

            return CreateFrom(description);
        }
#endif

        public static AvatarDescription CreateFrom(HumanDescription description)
        {
            var avatarDescription = ScriptableObject.CreateInstance<AvatarDescription>();
            avatarDescription.name = "AvatarDescription";
            avatarDescription.armStretch = description.armStretch;
            avatarDescription.legStretch = description.legStretch;
            avatarDescription.feetSpacing = description.feetSpacing;
            avatarDescription.hasTranslationDoF = description.hasTranslationDoF;
            avatarDescription.lowerArmTwist = description.lowerArmTwist;
            avatarDescription.lowerLegTwist = description.lowerLegTwist;
            avatarDescription.upperArmTwist = description.upperArmTwist;
            avatarDescription.upperLegTwist = description.upperLegTwist;
            avatarDescription.human = description.human.Select(BoneLimit.From).ToArray();
            return avatarDescription;
        }

        public static AvatarDescription Create(AvatarDescription src = null)
        {
            var avatarDescription = ScriptableObject.CreateInstance<AvatarDescription>();
            avatarDescription.name = "AvatarDescription";
            if (src != null)
            {
                avatarDescription.armStretch = src.armStretch;
                avatarDescription.legStretch = src.legStretch;
                avatarDescription.feetSpacing = src.feetSpacing;
                avatarDescription.upperArmTwist = src.upperArmTwist;
                avatarDescription.lowerArmTwist = src.lowerArmTwist;
                avatarDescription.upperLegTwist = src.upperLegTwist;
                avatarDescription.lowerLegTwist = src.lowerLegTwist;
            }
            else
            {
                avatarDescription.armStretch = 0.05f;
                avatarDescription.legStretch = 0.05f;
                avatarDescription.feetSpacing = 0.0f;
                avatarDescription.lowerArmTwist = 0.5f;
                avatarDescription.upperArmTwist = 0.5f;
                avatarDescription.upperLegTwist = 0.5f;
                avatarDescription.lowerLegTwist = 0.5f;
            }

            return avatarDescription;
        }

        public static AvatarDescription Create(Transform[] boneTransforms, Skeleton skeleton)
        {
            return Create(skeleton.Bones.Select(
                x => new KeyValuePair<HumanBodyBones, Transform>(x.Key, boneTransforms[x.Value])));
        }

        public static AvatarDescription Create(IEnumerable<KeyValuePair<HumanBodyBones, Transform>> skeleton)
        {
            var description = Create();
            description.SetHumanBones(skeleton);
            return description;
        }

        public void SetHumanBones(IEnumerable<KeyValuePair<HumanBodyBones, Transform>> skeleton)
        {
            human = skeleton.Select(x =>
            {
                return new BoneLimit
                {
                    humanBone = x.Key,
                    boneName = x.Value.name,
                    useDefaultValues = true,
                };
            }).ToArray();
        }

#if UNITY_EDITOR
        /// <summary>
        /// * https://answers.unity.com/questions/612177/how-can-i-access-human-avatar-bone-and-muscle-valu.html
        /// </summary>
        /// <param name="target"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public static bool GetHumanDescription(UnityEngine.Object target, ref HumanDescription des)
        {
            if (target != null)
            {
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(target));
                if (importer != null)
                {
                    Debug.Log("AssetImporter Type: " + importer.GetType());
                    ModelImporter modelImporter = importer as ModelImporter;
                    if (modelImporter != null)
                    {
                        des = modelImporter.humanDescription;
                        Debug.Log("## Cool stuff data by ModelImporter ##");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("## Please Select Imported Model in Project View not prefab or other things ##");
                    }
                }
            }

            return false;
        }
#endif
    }
}