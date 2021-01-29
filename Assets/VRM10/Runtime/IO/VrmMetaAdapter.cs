using System.Collections.Generic;
using VrmLib;

namespace UniVRM10
{
    public static class VrmMetaAdapter
    {
        public static AvatarPermission ToAvaterPermission(this UniGLTF.Extensions.VRMC_vrm.Meta self)
        {
            return new AvatarPermission
            {
                AvatarUsage = (AvatarUsageType)self.AvatarPermission,
                IsAllowedViolentUsage = self.AllowExcessivelyViolentUsage.Value,
                IsAllowedSexualUsage = self.AllowExcessivelySexualUsage.Value,
                CommercialUsage = (CommercialUsageType)self.CommercialUsage,
                // OtherPermissionUrl = self.OtherPermissionUrl,
                IsAllowedPoliticalOrReligiousUsage = self.AllowPoliticalOrReligiousUsage.Value,
            };
        }

        public static RedistributionLicense ToRedistributionLicense(this UniGLTF.Extensions.VRMC_vrm.Meta self)
        {
            return new RedistributionLicense
            {
                CreditNotation = (CreditNotationType)self.CreditNotation,
                IsAllowRedistribution = self.AllowRedistribution.Value,
                ModificationLicense = (ModificationLicenseType)self.Modification,
                OtherLicenseUrl = self.OtherLicenseUrl,
            };
        }

        public static Meta FromGltf(this UniGLTF.Extensions.VRMC_vrm.Meta self, List<Texture> textures)
        {
            var meta = new Meta
            {
                Name = self.Name,
                Version = self.Version,
                ContactInformation = self.ContactInformation,
                AvatarPermission = ToAvaterPermission(self),
                RedistributionLicense = ToRedistributionLicense(self),
            };
            if (self.References != null)
            {
                meta.References.AddRange(self.References);
            }
            if (self.Authors != null)
            {
                meta.Authors.AddRange(self.Authors);
            }
            if (self.ThumbnailImage.HasValue)
            {
                var texture = textures[self.ThumbnailImage.Value] as ImageTexture;
                if (texture != null)
                {
                    meta.Thumbnail = texture.Image;
                }
            }

            return meta;
        }

        public static UniGLTF.Extensions.VRMC_vrm.Meta ToGltf(this Meta self, List<Texture> textures)
        {
            var meta = new UniGLTF.Extensions.VRMC_vrm.Meta
            {
                Name = self.Name,
                Version = self.Version,
                ContactInformation = self.ContactInformation,
                CopyrightInformation = self.CopyrightInformation,
                // AvatarPermission
                AvatarPermission = (UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType)self.AvatarPermission.AvatarUsage,
                AllowExcessivelyViolentUsage = self.AvatarPermission.IsAllowedViolentUsage,
                AllowExcessivelySexualUsage = self.AvatarPermission.IsAllowedSexualUsage,
                CommercialUsage = (UniGLTF.Extensions.VRMC_vrm.CommercialUsageType)self.AvatarPermission.CommercialUsage,
                AllowPoliticalOrReligiousUsage = self.AvatarPermission.IsAllowedPoliticalOrReligiousUsage,
                // OtherPermissionUrl = self.AvatarPermission.OtherPermissionUrl,
                // RedistributionLicense
                CreditNotation = (UniGLTF.Extensions.VRMC_vrm.CreditNotationType)self.RedistributionLicense.CreditNotation,
                AllowRedistribution = self.RedistributionLicense.IsAllowRedistribution,
                Modification = (UniGLTF.Extensions.VRMC_vrm.ModificationType)self.RedistributionLicense.ModificationLicense,
                OtherLicenseUrl = self.RedistributionLicense.OtherLicenseUrl,
                References = self.References,
                Authors = self.Authors,
            };
            if (self.Thumbnail != null)
            {
                for (int i = 0; i < textures.Count; ++i)
                {
                    var texture = textures[i] as ImageTexture;
                    if (texture.Image == self.Thumbnail)
                    {
                        meta.ThumbnailImage = i;
                        break;
                    }
                }
            }
            return meta;
        }
    }
}
