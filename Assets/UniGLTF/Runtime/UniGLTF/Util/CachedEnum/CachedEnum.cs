using System;
using System.Collections;

namespace UniGLTF
{
    public static class CachedEnum
    {
        public static T Parse<T>(string name, bool ignoreCase = false) where T : struct, Enum
        {
            if (ignoreCase)
            {
                return CachedEnumType<T>.ParseIgnoreCase(name);
            }
            else
            {
                return CachedEnumType<T>.Parse(name);
            }
        }

        public static T TryParseOrDefault<T>(string name, bool ignoreCase = false, T defaultValue = default)
            where T : struct, Enum
        {
            try
            {
                if (ignoreCase)
                {
                    return CachedEnumType<T>.ParseIgnoreCase(name);
                }
                else
                {
                    return CachedEnumType<T>.Parse(name);
                }
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

