using System;
using System.Collections.Generic;
using UniJSON;

namespace UniVRM10
{
    /// <summary>
    /// for UnitTest
    /// </summary>
    internal static class MigrationCheck
    {
        #region for UnitTest
        public class MigrationException : Exception
        {
            public MigrationException(string key, string value) : base($"{key}: {value}")
            {
            }
        }

        public static void CheckBone(string bone, JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.HumanBone vrm1)
        {
            var vrm0NodeIndex = vrm0["node"].GetInt32();
            if (vrm0NodeIndex != vrm1.Node)
            {
                throw new Exception($"different {bone}: {vrm0NodeIndex} != {vrm1.Node}");
            }
        }

        public static void CheckHumanoid(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.Humanoid vrm1)
        {
            foreach (var humanoidBone in vrm0["humanBones"].ArrayItems())
            {
                var boneType = humanoidBone["bone"].GetString();
                switch (boneType)
                {
                    case "hips": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Hips); break;
                    case "leftUpperLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftUpperLeg); break;
                    case "rightUpperLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightUpperLeg); break;
                    case "leftLowerLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLowerLeg); break;
                    case "rightLowerLeg": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLowerLeg); break;
                    case "leftFoot": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftFoot); break;
                    case "rightFoot": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightFoot); break;
                    case "spine": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Spine); break;
                    case "chest": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Chest); break;
                    case "neck": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Neck); break;
                    case "head": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Head); break;
                    case "leftShoulder": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftShoulder); break;
                    case "rightShoulder": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightShoulder); break;
                    case "leftUpperArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftUpperArm); break;
                    case "rightUpperArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightUpperArm); break;
                    case "leftLowerArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLowerArm); break;
                    case "rightLowerArm": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLowerArm); break;
                    case "leftHand": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftHand); break;
                    case "rightHand": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightHand); break;
                    case "leftToes": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftToes); break;
                    case "rightToes": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightToes); break;
                    case "leftEye": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftEye); break;
                    case "rightEye": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightEye); break;
                    case "jaw": CheckBone(boneType, humanoidBone, vrm1.HumanBones.Jaw); break;
                    case "leftThumbMetacarpal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbMetacarpal); break;
                    case "leftThumbProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbProximal); break;
                    case "leftThumbDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftThumbDistal); break;
                    case "leftIndexProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftIndexProximal); break;
                    case "leftIndexIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftIndexIntermediate); break;
                    case "leftIndexDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftIndexDistal); break;
                    case "leftMiddleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftMiddleProximal); break;
                    case "leftMiddleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftMiddleIntermediate); break;
                    case "leftMiddleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftMiddleDistal); break;
                    case "leftRingProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftRingProximal); break;
                    case "leftRingIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftRingIntermediate); break;
                    case "leftRingDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftRingDistal); break;
                    case "leftLittleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLittleProximal); break;
                    case "leftLittleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLittleIntermediate); break;
                    case "leftLittleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.LeftLittleDistal); break;
                    case "rightThumbMetacarpal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbMetacarpal); break;
                    case "rightThumbProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbProximal); break;
                    case "rightThumbDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightThumbDistal); break;
                    case "rightIndexProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightIndexProximal); break;
                    case "rightIndexIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightIndexIntermediate); break;
                    case "rightIndexDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightIndexDistal); break;
                    case "rightMiddleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightMiddleProximal); break;
                    case "rightMiddleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightMiddleIntermediate); break;
                    case "rightMiddleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightMiddleDistal); break;
                    case "rightRingProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightRingProximal); break;
                    case "rightRingIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightRingIntermediate); break;
                    case "rightRingDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightRingDistal); break;
                    case "rightLittleProximal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLittleProximal); break;
                    case "rightLittleIntermediate": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLittleIntermediate); break;
                    case "rightLittleDistal": CheckBone(boneType, humanoidBone, vrm1.HumanBones.RightLittleDistal); break;
                    case "upperChest": CheckBone(boneType, humanoidBone, vrm1.HumanBones.UpperChest); break;
                    default: throw new MigrationException("humanonoid.humanBones[*].bone", boneType);
                }
            }
        }

        static bool IsSingleList(string key, string lhs, List<string> rhs)
        {
            if (rhs.Count != 1) throw new MigrationException(key, $"{rhs.Count}");
            return lhs == rhs[0];
        }

        static string AvatarPermission(string key, UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType x)
        {
            switch (x)
            {
                case UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.everyone: return "Everyone";
                    // case AvatarPermissionType.onlyAuthor: return "OnlyAuthor";
                    // case AvatarPermissionType.explicitlyLicensedPerson: return "Explicited";
            }
            throw new MigrationException(key, $"{x}");
        }

        public static void CheckMeta(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.Meta vrm1)
        {
            if (vrm0["title"].GetString() != vrm1.Name) throw new MigrationException("meta.title", vrm1.Name);
            if (vrm0["version"].GetString() != vrm1.Version) throw new MigrationException("meta.version", vrm1.Version);
            if (!IsSingleList("meta.author", vrm0["author"].GetString(), vrm1.Authors)) throw new MigrationException("meta.author", $"{vrm1.Authors}");
            if (vrm0["contactInformation"].GetString() != vrm1.ContactInformation) throw new MigrationException("meta.contactInformation", vrm1.ContactInformation);
            if (!IsSingleList("meta.reference", vrm0["reference"].GetString(), vrm1.References)) throw new MigrationException("meta.reference", $"{vrm1.References}");
            if (vrm0["texture"].GetInt32() != vrm1.ThumbnailImage) throw new MigrationException("meta.texture", $"{vrm1.ThumbnailImage}");

            if (vrm0["allowedUserName"].GetString() != AvatarPermission("meta.allowedUserName", vrm1.AvatarPermission)) throw new MigrationException("meta.allowedUserName", $"{vrm1.AvatarPermission}");
            if (vrm0["violentUssageName"].GetString() == "Allow" != vrm1.AllowExcessivelyViolentUsage) throw new MigrationException("meta.violentUssageName", $"{vrm1.AllowExcessivelyViolentUsage}");
            if (vrm0["sexualUssageName"].GetString() == "Allow" != vrm1.AllowExcessivelySexualUsage) throw new MigrationException("meta.sexualUssageName", $"{vrm1.AllowExcessivelyViolentUsage}");

            if (vrm0["commercialUssageName"].GetString() == "Allow")
            {
                if (vrm1.CommercialUsage == UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit)
                {
                    throw new MigrationException("meta.commercialUssageName", $"{vrm1.CommercialUsage}");
                }
            }
            else
            {
                if (vrm1.CommercialUsage == UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.corporation
                || vrm1.CommercialUsage == UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalProfit)
                {
                    throw new MigrationException("meta.commercialUssageName", $"{vrm1.CommercialUsage}");
                }
            }

            if (MigrationVrmMeta.GetLicenseUrl(vrm0) != vrm1.OtherLicenseUrl) throw new MigrationException("meta.otherLicenseUrl", vrm1.OtherLicenseUrl);

            switch (vrm0["licenseName"].GetString())
            {
                case "Other":
                    {
                        if (vrm1.Modification != UniGLTF.Extensions.VRMC_vrm.ModificationType.prohibited) throw new MigrationException("meta.licenceName", $"{vrm1.Modification}");
                        if (vrm1.AllowRedistribution.Value) throw new MigrationException("meta.liceneName", $"{vrm1.Modification}");
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm1)
        {
            CheckMeta(vrm0["meta"], vrm1.Meta);
            CheckHumanoid(vrm0["humanoid"], vrm1.Humanoid);
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone vrm1, List<UniGLTF.glTFNode> nodes)
        {
            // Migration.CheckSpringBone(vrm0["secondaryAnimation"], vrm1.sp)
        }
        #endregion

    }
}
