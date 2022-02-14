using System.Collections.Generic;

namespace UniGLTF
{
    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<T, U>(this KeyValuePair<T, U> pair, out T key, out U value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
