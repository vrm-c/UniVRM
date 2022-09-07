using System;

namespace UniGLTF.Utils
{
    public static class CachedEnum
    {
        public static T Parse<T>(string name, bool ignoreCase = false) where T : struct, Enum
        {
            return CachedEnumType<T>.Parse(name, ignoreCase);
        }

        public static T TryParseOrDefault<T>(string name, bool ignoreCase = false, T defaultValue = default)
            where T : struct, Enum
        {
            try
            {
                return Parse<T>(name, ignoreCase: ignoreCase);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T[] GetValues<T>() where T : struct, Enum
        {
            return CachedEnumType<T>.Values;
        }
    }
}

