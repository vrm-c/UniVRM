using System;
using System.Collections.Generic;

namespace UniGLTF
{
    internal static class CachedEnumType<T> where T : struct, Enum
    {
        private static Dictionary<string, T> _values = new Dictionary<string, T>();
        private static Dictionary<string, T> _ignoreCaseValues = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        private static T[] _allValues;

        public static T[] Values
        {
            get
            {
                if (_allValues == null)
                {
                    _allValues = Enum.GetValues(typeof(T)) as T[];
                }

                return _allValues;
            }
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
}