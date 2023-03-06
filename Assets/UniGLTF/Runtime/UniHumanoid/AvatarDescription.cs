#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


namespace UniHumanoid
{
    // TODO: BoneLimit.cs に分ける(v0.104以降)
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


        // HumanTrait.BoneName は HumanBodyBones.ToString とほぼ一対一に対応するが、
        // 指のボーンについては " " の有無という微妙な違いがある。
        // このスペースは AvatarBuilder.BuildHumanAvatar において必用であり、
        // HumanBodyBones.ToString と区別する必要がある。
        //
        // また、下記についてGCが発生するのでキャッシュします。
        // * HumanTrait.BoneName
        // * traitName.Replace
        // * Enum.Parse
        //
        private static readonly Dictionary<HumanBodyBones, string> cachedHumanBodyBonesToBoneTraitNameMap =
        HumanTrait.BoneName.ToDictionary(
            traitName => (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), traitName.Replace(" ", "")),
            traitName => traitName);

        // 逆引き
        private static readonly Dictionary<string, HumanBodyBones> cachedBoneTraitNameToHumanBodyBonesMap =
        HumanTrait.BoneName.ToDictionary(
            traitName => traitName,
            traitName => (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), traitName.Replace(" ", "")));

        public static BoneLimit From(HumanBone bone)
        {
            return new BoneLimit
            {
                humanBone = cachedBoneTraitNameToHumanBodyBonesMap[bone.humanName],
                boneName = bone.boneName,
                useDefaultValues = bone.limit.useDefaultValues,
                min = bone.limit.min,
                max = bone.limit.max,
                center = bone.limit.center,
                axisLength = bone.limit.axisLength,
            };
        }

        public HumanBone ToHumanBone()
        {
            return new HumanBone
            {
                boneName = boneName,
                humanName = cachedHumanBodyBonesToBoneTraitNameMap[humanBone],
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

    class UniqueName
    {
        HashSet<string> m_uniqueNameSet = new HashSet<string>();
        int m_counter = 1;

        public void ForceUniqueName(Transform t)
        {
            if (!m_uniqueNameSet.Contains(t.name))
            {
                m_uniqueNameSet.Add(t.name);
                return;
            }

            if (t.parent != null && t.childCount == 0)
            {
                /// AvatarBuilder:BuildHumanAvatar で同名の Transform があるとエラーになる。
                /// 
                /// AvatarBuilder 'GLTF': Ambiguous Transform '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/neck_1/neck_2/head/ENDSITE' and '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/l_shoulder/l_up_arm/l_low_arm/l_hand/ENDSITE' found in hierarchy for human bone 'Head'. Transform name mapped to a human bone must be unique.
                /// UnityEngine.AvatarBuilder:BuildHumanAvatar (UnityEngine.GameObject,UnityEngine.HumanDescription)
                /// UniHumanoid.AvatarDescription:CreateAvatar (UnityEngine.Transform) 
                /// 
                /// 主に BVH の EndSite 由来の GameObject 名が重複することへの対策
                ///  ex: parent-ENDSITE
                var newName = $"{t.parent.name}-{t.name}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    Debug.LogWarning($"force rename: {t.name} => {newName}");
                    t.name = newName;
                    m_uniqueNameSet.Add(newName);
                    return;
                }
            }

            // 連番
            for (int i = 0; i < 100; ++i)
            {
                // ex: name.1
                var newName = $"{t.name}{m_counter++}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    Debug.LogWarning($"force rename: {t.name} => {newName}");
                    t.name = newName;
                    m_uniqueNameSet.Add(newName);
                    return;
                }
            }

            throw new NotImplementedException();
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
            // force unique name
            var uniqueName = new UniqueName();
            var transforms = root.GetComponentsInChildren<Transform>();
            foreach (var t in transforms)
            {
                uniqueName.ForceUniqueName(t);
            }

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