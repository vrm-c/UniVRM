using System;
using System.Collections.Generic;

namespace UniGLTF.Utils
{
    internal static class CachedEnumType<T> where T : struct, Enum
    {
        private static readonly Dictionary<string, T> _values = new Dictionary<string, T>();
        private static readonly Dictionary<string, T> _ignoreCaseValues = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
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

        public static T Parse(string name, bool ignoreCase)
        {
            var caches = ignoreCase ? _ignoreCaseValues : _values;

            if (caches.TryGetValue(name, out var ignoreCaseValue))
            {
                return ignoreCaseValue;
            }

            if (Enum.TryParse<T>(name, ignoreCase, out var result))
            {
                caches.Add(name, result);
                return result;
            }

            throw new ArgumentException(name);
        }
    }
}