using System.Collections.Generic;

namespace UniVRM10
{
    public static class DictionaryExtensions
    {
        public static U GetOrDefault<T, U>(this Dictionary<T, U> d, T key)
        {
            if(key == null)
            {
                return default;
            }

            if(d.TryGetValue(key, out U value))
            {
                return value;;
            }
            else{
                return default;
            }
        }
    }
}
