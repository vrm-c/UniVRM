using System.IO;

namespace VRM
{
    public static class VRMTestAssets
    {
        public static bool TryGetPath(string relative, out string path)
        {
            var env = System.Environment.GetEnvironmentVariable("VRM_TEST_MODELS");
            if (string.IsNullOrEmpty(env))
            {
                path = default;
                return false;
            }
            var root = new DirectoryInfo(env);
            if (!root.Exists)
            {
                path = default;
                return false;
            }

            path = Path.Combine(root.FullName, relative);
            if (!File.Exists(path))
            {
                return false;
            }
            return true;
        }
    }
}
