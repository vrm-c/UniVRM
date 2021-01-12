using System;
using System.Collections.Generic;

namespace VrmLib
{
    public static class ListExtensions
    {
        public static int IndexOfThrow<T>(this List<T> list, T target)
        {
            var index = list.IndexOf(target);
            if (index == -1)
            {
                throw new KeyNotFoundException();
            }
            return index;
        }

        public static int? IndexOfNullable<T>(this List<T> list, T target)
        {
            var index = list.IndexOf(target);
            if (index == -1)
            {
                return default;
            }
            return index;
        }
     }
}