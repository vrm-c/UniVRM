using System;

namespace VRM
{
    public class VRMSpecVersion
    {
        public const int Major = 0;
        public const int Minor = 0;

        public static string Version
        {
            get
            {
                return String.Format("{0}.{1}", Major, Minor);
            }
        }

        public const string VERSION = "0.0";
    }
}
