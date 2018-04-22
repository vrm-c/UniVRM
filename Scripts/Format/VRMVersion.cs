
using System;

namespace VRM
{
    public static class VRMVersion
    {
        public const int MAJOR = 0;
        public const int MINOR = 36;

        public const string VERSION = "0.36";

        public const string DecrementMenuName = "VRM/Version(0.36) Decrement";
        public const string IncrementMenuName = "VRM/Version(0.36) Increment";

        public static bool IsNewer(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            var prefix = "UniVRM-";
            if (version.StartsWith(prefix))
            {
                version = version.Substring(prefix.Length);
            }

            var splited = version.Split('.');
            if (splited.Length < 2)
            {
                return false;
            }

            try
            {
                var major = int.Parse(splited[0]);
                var minor = int.Parse(splited[1]);

                if (major < MAJOR)
                {
                    return false;
                }
                else if (major > MAJOR)
                {
                    return true;
                }
                else
                {
                    return minor > MINOR;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
