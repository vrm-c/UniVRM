using System;
using System.Collections.Generic;
using UniJSON;

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
        public static UniGLTF.Extensions.VRMC_vrm.Meta Migrate(JsonNode vrm0)
        {
            var meta = new UniGLTF.Extensions.VRMC_vrm.Meta
            {
                AllowPoliticalOrReligiousUsage = false,
                AllowExcessivelySexualUsage = false,
                AllowExcessivelyViolentUsage = false,
                AllowRedistribution = false,
                AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.onlyAuthor,
                CommercialUsage = UniGLTF.Extensions.VRMC_vrm.CommercialUsageType.personalNonProfit,
                CreditNotation = UniGLTF.Extensions.VRMC_vrm.CreditNotationType.required,
                Modification = UniGLTF.Extensions.VRMC_vrm.ModificationType.prohibited,
            };

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
                    case "texture": meta.ThumbnailImage = kv.Value.GetInt32(); break;

                    case "allowedUserName":
                        {
                            var allowedUserName = kv.Value.GetString();
                            switch (allowedUserName)
                            {
                                case "OnlyAuthor":
                                    meta.AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.onlyAuthor;
                                    break;
                                case "ExplicitlyLicensedPerson":
                                    meta.AvatarPermission = UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType.explicitlyLicensedPerson;
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

                    case "otherPermissionUrl":
                        {
                            // TODO
                            // var url = kv.Value.GetString();
                            // if (!String.IsNullOrWhiteSpace(url))
                            // {
                            //     throw new NotImplementedException("otherPermissionUrl not allowd");
                            // }
                        }
                        break;

                    case "otherLicenseUrl": meta.OtherLicenseUrl = kv.Value.GetString(); break;

                    case "licenseName":
                        {
                            // TODO
                            // CreditNotation = CreditNotationType.required,
                        }
                        break;

                    default:
                        throw new NotImplementedException(key);
                }
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
    }
}
