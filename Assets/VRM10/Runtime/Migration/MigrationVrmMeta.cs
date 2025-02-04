using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    ///
    /// 互換性の無いところ
    /// 
    /// * きつくなる方向は許す
    /// * 緩くなる方向は不許可(throw)
    /// 
    // "meta": {
    //   "title": "Alicia Solid",
    //   "version": "1.10",
    //   "author": "© DWANGO Co., Ltd.",
    //   "contactInformation": "https://3d.nicovideo.jp/alicia/",
    //   "reference": "",
    //   "texture": 7,
    //   "allowedUserName": "Everyone",
    //   "violentUssageName": "Disallow",
    //   "sexualUssageName": "Disallow",
    //   "commercialUssageName": "Allow",
    //   "otherPermissionUrl": "https://3d.nicovideo.jp/alicia/rule.html",
    //   "licenseName": "Other",
    //   "otherLicenseUrl": "https://3d.nicovideo.jp/alicia/rule.html"
    // },
    public static class MigrationVrmMeta
    {
        public static UniGLTF.Extensions.VRMC_vrm.Meta Migrate(UniGLTF.glTF gltf, JsonNode vrm0)
        {
            var meta = new UniGLTF.Extensions.VRMC_vrm.Meta
            {
                LicenseUrl = Vrm10Exporter.LICENSE_URL_JA,
                AllowPoliticalOrReligiousUsage = false,
                AllowExcessivelySexualUsage = false,
                AllowExcessivelyViolentUsage = false,
                AllowRedistribution = false,
                AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.onlyAuthor,
                CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit,
                CreditNotation = UniGLTF.Extensions.VRMC_vrm.CreditNotationType.required,
                Modification = UniGLTF.Extensions.VRMC_vrm.ModificationType.prohibited,
            };

            string otherLicenseUrl = default;
            string otherPermissionUrl = default;

            foreach (var kv in vrm0.ObjectItems())
            {
                var key = kv.Key.GetString();
                switch (key)
                {
                    case "title": meta.Name = kv.Value.GetString(); break;
                    case "version": meta.Version = kv.Value.GetString(); break;
                    case "author": meta.Authors = new List<string>() { kv.Value.GetString() }; break;
                    case "contactInformation": meta.ContactInformation = kv.Value.GetString(); break;
                    case "reference": meta.References = new List<string>() { kv.Value.GetString() }; break;
                    case "texture":
                        {
                            // vrm0x use texture. vrm10 use image
                            var textureIndex = kv.Value.GetInt32();
                            if (textureIndex == -1)
                            {
                                meta.ThumbnailImage = -1;
                            }
                            else
                            {
                                var gltfTexture = gltf.textures[textureIndex];
                                meta.ThumbnailImage = gltfTexture.source;
                            }
                            break;
                        }

                    case "allowedUserName":
                        {
                            var allowedUserName = kv.Value.GetString();
                            switch (allowedUserName)
                            {
                                case "OnlyAuthor":
                                    meta.AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.onlyAuthor;
                                    break;
                                case "ExplicitlyLicensedPerson":
                                    meta.AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.onlySeparatelyLicensedPerson;
                                    break;
                                case "Everyone":
                                    meta.AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.everyone;
                                    break;
                                default:
                                    throw new NotImplementedException($"{key}: {allowedUserName}");
                            }
                        }
                        break;

                    case "violentUssageName": // Typo "Ussage" is VRM 0.x spec.
                        {
                            var violentUsageName = kv.Value.GetString();
                            switch (violentUsageName)
                            {
                                case "Allow":
                                    meta.AllowExcessivelyViolentUsage = true;
                                    break;
                                case "Disallow":
                                    meta.AllowExcessivelyViolentUsage = false;
                                    break;
                                default:
                                    throw new NotImplementedException($"{key}: {violentUsageName}");
                            }
                        }
                        break;

                    case "sexualUssageName": // Typo "Ussage" is VRM 0.x spec.
                        {
                            var sexualUsageName = kv.Value.GetString();
                            switch (sexualUsageName)
                            {
                                case "Allow":
                                    meta.AllowExcessivelySexualUsage = true;
                                    break;
                                case "Disallow":
                                    meta.AllowExcessivelySexualUsage = false;
                                    break;
                                default:
                                    throw new NotImplementedException($"{key}: {sexualUsageName}");
                            }
                        }
                        break;

                    case "commercialUssageName": // Typo "Ussage" is VRM 0.x spec.
                        {
                            var commercialUsageName = kv.Value.GetString();
                            switch (commercialUsageName)
                            {
                                case "Allow":
                                    meta.CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalProfit;
                                    break;
                                case "Disallow":
                                    meta.CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit;
                                    break;
                                default:
                                    throw new NotImplementedException($"{key}: {commercialUsageName}");
                            }
                        }
                        break;

                    case "otherPermissionUrl": otherPermissionUrl = kv.Value.GetString(); break;
                    case "otherLicenseUrl": otherLicenseUrl = kv.Value.GetString(); break;

                    case "licenseName":
                        {
                            // TODO
                            // CreditNotation = CreditNotationType.required,
                        }
                        break;

                    default:
                        UniGLTFLogger.Warning($"[meta migration] unknown key: {key}");
                        break;
                } // switch
            } // foreach

            //
            // OtherLicenseUrl migrate
            // OtherPermissionURL removed
            //
            if (!string.IsNullOrEmpty(otherLicenseUrl) && !string.IsNullOrEmpty(otherPermissionUrl))
            {
                if (otherLicenseUrl == otherPermissionUrl)
                {
                    // OK
                    meta.OtherLicenseUrl = otherLicenseUrl;
                }
                else
                {
                    // https://github.com/vrm-c/UniVRM/issues/1611
                    // 両方を記述しエラーとしない
                    meta.OtherLicenseUrl = $"'{otherLicenseUrl}', '{otherPermissionUrl}'";
                }
            }
            else if (!string.IsNullOrEmpty(otherLicenseUrl))
            {
                meta.OtherLicenseUrl = otherLicenseUrl;
            }
            else if (!string.IsNullOrEmpty(otherPermissionUrl))
            {
                // otherPermissionUrl => otherLicenseUrl
                meta.OtherLicenseUrl = otherPermissionUrl;
            }
            else
            {
                // null
            }

            return meta;
        }

        public static string GetLicenseUrl(JsonNode vrm0)
        {
            string l0 = default;
            string l1 = default;
            foreach (var kv in vrm0.ObjectItems())
            {
                switch (kv.Key.GetString())
                {
                    case "otherLicenseUrl":
                        l0 = kv.Value.GetString();
                        break;

                    case "otherPermissionUrl":
                        l1 = kv.Value.GetString();
                        break;
                }
            }
            if (!string.IsNullOrWhiteSpace(l0))
            {
                return l0;
            }
            if (!string.IsNullOrWhiteSpace(l1))
            {
                return l1;
            }
            return "";
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

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.Meta vrm1)
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
    }
}
