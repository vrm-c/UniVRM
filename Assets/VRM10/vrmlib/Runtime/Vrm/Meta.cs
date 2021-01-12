using System.Collections.Generic;
using System.Linq;

namespace VrmLib
{
    public class Meta
    {
        public string Name = "";

        public string Version = "";

        // 1.0 added
        public string CopyrightInformation = "";

        // 1.0 added
        public List<string> Authors = new List<string>();

        // backward compatibility
        public string Author
        {
            get => Authors.FirstOrDefault();
            set
            {
                Authors.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    Authors.Add(value);
                }
            }
        }

        public string ContactInformation = "";

        public List<string> References = new List<string>();

        public string Reference
        {
            get => References.FirstOrDefault();
            set
            {
                References.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    References.Add(value);
                }
            }
        }

        public Image Thumbnail;

        public IAvatarPermission AvatarPermission;

        public IRedistributionLicense RedistributionLicense;
    }
}
