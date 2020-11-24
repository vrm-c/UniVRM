using System;
using System.Collections;
using System.Collections.Generic;

namespace UniGLTF
{
    public sealed class CacheEnum 
    {
        public static T Parse<T>(string name, bool ignoreCase = false) where T : struct, Enum
        {
            if(ignoreCase)
            {
                return CacheParse<T>.ParseIgnoreCase(name);
            }
            else
            {
                return CacheParse<T>.Parse(name);
            }
        }

        public static T TryParseOrDefault<T>(string name,  bool ignoreCase = false, T defaultValue=default(T)) where T : struct, Enum
        {
            try
            {
                if(ignoreCase)
                {
                    return CacheParse<T>.ParseIgnoreCase(name);
                }
                else
                {
                    return CacheParse<T>.Parse(name);
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T[] GetValues<T>() where T : struct, Enum
        {
            return CacheValues<T>.Values;
        }

        private static class CacheParse<T> where T : struct, Enum
        {
            private static Dictionary<string, T> _values = new Dictionary<string, T>();
            private static Dictionary<string, T> _ignoreCaseValues = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            static CacheParse()
            {
            }

            public static T ParseIgnoreCase(string name)
            {
                if(_ignoreCaseValues.TryGetValue(name, out var value))
                {
                    return value;
                }
                else
                {
                    T result;
                    value =  Enum.TryParse<T>(name, true, out result)
                        ? result
                        : throw new ArgumentException(nameof(result));
                    _ignoreCaseValues.Add(name, value);
                    return value;
                }
            }

            public static T Parse(string name)
            {
                if(_values.TryGetValue(name, out var value))
                {
                    return value;
                }
                else
                {
                    T result;
                    value =  Enum.TryParse<T>(name, false, out result)
                        ? result
                        : throw new ArgumentException(nameof(result));
                    _values.Add(name, value);
                    return value;
                }
            }
        }

        private static class CacheValues<T> where T : struct, Enum
        {
            public static readonly T[] Values;

            static CacheValues()
            {
                Values = Enum.GetValues(typeof(T)) as T[];
            }
        }
    }
}

