using System;

namespace UniGLTF.Utils
{
    /// <summary>
    /// CachedEnumType<T> に対するインターフェース。
    /// 非 Generic class
    /// </summary>
    public static class CachedEnum
    {
        public static T Parse<T>(string name, bool ignoreCase = false) where T : struct, Enum
        {
            if (ignoreCase)
            {
                return CachedEnumType<T>.IgnoreCaseMap[name];
            }
            else
            {
                return CachedEnumType<T>.Map[name];
            }
        }

        public static T ParseOrDefault<T>(string name, bool ignoreCase = false, T defaultValue = default)
            where T : struct, Enum
        {
            try
            {
                return Parse<T>(name, ignoreCase: ignoreCase);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
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

