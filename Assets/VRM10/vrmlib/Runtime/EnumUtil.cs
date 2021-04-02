using System;
using System.Collections.Generic;
using System.Linq;

namespace VrmLib
{
    public static class EnumUtil
    {
        public static T Parse<T>(string src, bool ignoreCase = true) where T : struct
        {
            if (string.IsNullOrEmpty(src))
            {
                return default(T);
            }

            return (T)Enum.Parse(typeof(T), src, ignoreCase);
        }

        public static T TryParseOrDefault<T>(string src, T defaultValue = default(T)) where T : struct
        {
            try
            {
                return (T)Enum.Parse(typeof(T), src, true);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static T Cast<T>(object src, bool ignoreCase = true) where T : struct
        {
            if (src is null)
            {
                throw new ArgumentNullException();
            }

            return (T)Enum.Parse(typeof(T), src.ToString(), ignoreCase);
        }

        class GenericCache<T> where T : Enum
        {
            public static T[] Values = GetValues().ToArray();

            static IEnumerable<T> GetValues()
            {
                foreach (var t in Enum.GetValues(typeof(T)))
                {
                    yield return (T)t;
                }
            }
        }

        public static T[] Values<T>() where T : Enum
        {
            return GenericCache<T>.Values;
        }
    }
}
