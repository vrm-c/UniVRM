namespace VrmLib
{
    public enum AvatarUsageType
    {
        OnlyAuthor,
        ExplicitlyLicensedPerson,
        Everyone,
    }

    public enum CommercialUsageType
    {
        PersonalNonCommercialNonProfit,
        PersonalNonCommercialProfit,
        PersonalCommercial,
        Corporation,
    }

    public interface IAvatarPermission
    {
        AvatarUsageType AvatarUsage { get; }
        bool IsAllowedViolentUsage { get; }
        bool IsAllowedSexualUsage { get; }

        // 1.0 removed
        bool IsAllowedCommercialUsage { get; }

        // 1.0 added
        CommercialUsageType CommercialUsage { get; }

        // 1.0 added
        bool IsAllowedPoliticalOrReligiousUsage { get; }

        // 1.0 added
        bool IsAllowedGameUsage { get; }

        string OtherPermissionUrl { get; }
    }

    /// <summary>
    /// 1.0向け
    /// </summary>
    public class AvatarPermission : IAvatarPermission
    {
        public AvatarUsageType AvatarUsage { get; set; }

        public bool IsAllowedViolentUsage { get; set; }

        public bool IsAllowedSexualUsage { get; set; }

        public bool IsAllowedCommercialUsage
        {
            get
            {
                switch (CommercialUsage)
                {
                    case CommercialUsageType.Corporation:
                    case CommercialUsageType.PersonalCommercial:
                        return true;
                }
                return false;
            }
        }

        public CommercialUsageType CommercialUsage { get; set; }

        public bool IsAllowedPoliticalOrReligiousUsage { get; set; }

        public bool IsAllowedGameUsage { get; set; }

        public string OtherPermissionUrl { get; set; } = "";
    }
}
