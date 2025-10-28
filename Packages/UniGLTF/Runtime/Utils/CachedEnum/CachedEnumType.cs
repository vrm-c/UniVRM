using System;
using System.Collections.Generic;


namespace UniGLTF.Utils
{
    /// <summary>
    /// enum T に対する static type caching 。
    /// 
    /// CachedEnumType<T>.Values
    /// CachedEnumType<T>.Map
    /// CachedEnumType<T>.IgnoreCaseMap
    /// 
    /// がスレッドセーフに(キャッシュされた)同じ値を返す。
    /// </summary>
    internal static class CachedEnumType<T> where T : struct, Enum
    {
        public static IReadOnlyDictionary<string, T> Map { get; } = CreateStringEnumMap(false);
        public static IReadOnlyDictionary<string, T> IgnoreCaseMap { get; } = CreateStringEnumMap(true);
        public static T[] Values { get; } = (T[])Enum.GetValues(typeof(T));

        private static Dictionary<string, T> CreateStringEnumMap(bool ignoreCase)
        {
            var dict = ignoreCase
                ? new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, T>()
                ;

            // ここで Values を使うと
            // System.TypeInitializationException
            // が起きる。
            // static 変数初期化中に別の static 変数を参照すると未初期化がありえるぽい(初期化順？)
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                dict.Add(value.ToString(), value);
            }
            return dict;
        }
    }
}
