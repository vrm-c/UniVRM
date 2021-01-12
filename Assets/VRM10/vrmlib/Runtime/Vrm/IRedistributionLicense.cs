using System;

namespace VrmLib
{
    public enum DistributionLicenseType
    {
        Redistribution_Prohibited,
        CC0,
        CC_BY,
        CC_BY_NC,
        CC_BY_SA,
        CC_BY_NC_SA,
        CC_BY_ND,
        CC_BY_NC_ND,
        Other
    }

    public enum CreditNotationType
    {
        Required,
        Unnecessary,
        Abandoned,
    }

    public enum ModificationLicenseType
    {
        Prohibited,
        Inherited,
        NotInherited
    }

    public interface IRedistributionLicense
    {
        // 1.0 removed
        DistributionLicenseType License { get; }

        CreditNotationType CreditNotation { get; }
        bool IsAllowRedistribution { get; }
        string OtherLicenseUrl { get; }

        ModificationLicenseType ModificationLicense { get; }
    }

    /// 1.0 向け
    public class RedistributionLicense : IRedistributionLicense
    {
        public DistributionLicenseType License
        {
            get => DistributionLicenseType.Redistribution_Prohibited;
        }

        public CreditNotationType CreditNotation { get; set; }

        public bool IsAllowRedistribution { get; set; }

        public ModificationLicenseType ModificationLicense { get; set; }

        public string OtherLicenseUrl { get; set; } = "";
    }
}
