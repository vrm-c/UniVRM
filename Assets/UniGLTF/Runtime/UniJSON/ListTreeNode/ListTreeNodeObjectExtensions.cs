using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class IValueNodeObjectExtensions
    {
        public static IEnumerable<KeyValuePair<JsonNode, JsonNode>> ObjectItems(this JsonNode self)
        {
            if (!self.IsMap()) throw new DeserializationException("is not object");
            var it = self.Children.GetEnumerator();
            while (it.MoveNext())
            {
                var key = it.Current;

                it.MoveNext();
                yield return new KeyValuePair<JsonNode, JsonNode>(key, it.Current);
            }
        }

        public static int GetObjectCount(this JsonNode self)
        {
            if (!self.IsMap()) throw new DeserializationException("is not object");
            return self.Children.Count() / 2;
        }

        public static string GetObjectValueOrDefault(this JsonNode self, String key, string defualtValue)
        {
            try
            {
                return self[key].GetString();
            }
            catch (KeyNotFoundException)
            {
                return defualtValue;
            }
        }

        public static JsonNode GetObjectItem(this JsonNode self, String key)
        {
            return self.GetObjectItem(Utf8String.From(key));
        }

        public static JsonNode GetObjectItem(this JsonNode self, Utf8String key)
        {
            foreach (var kv in self.ObjectItems())
            {
                if (kv.Key.GetUtf8String() == key)
                {
                    return kv.Value;
                }
            }
            throw new KeyNotFoundException(key.ToString());
        }

        public static bool ContainsKey(this JsonNode self, Utf8String key)
        {
            return self.ObjectItems().Any(x => x.Key.GetUtf8String() == key);
        }

        public static bool ContainsKey(this JsonNode self, String key)
        {
            var ukey = Utf8String.From(key);
            return self.ContainsKey(ukey);
        }

        public static bool TryGet(this JsonNode self, Utf8String key, out JsonNode found)
        {
            foreach (var kv in self.ObjectItems())
            {
                if (kv.Key.GetUtf8String() == key)
                {
                    found = kv.Value;
                    return true;
                }
            }

            found = default;
            return false;
        }

        public static bool TryGet(this JsonNode self, String key, out JsonNode found)
        {
            var ukey = Utf8String.From(key);
            return self.TryGet(ukey, out found);
        }

        public static Utf8String KeyOf(this JsonNode self, JsonNode node)
        {
            foreach (var kv in self.ObjectItems())
            {
                if (node.ValueIndex == kv.Value.ValueIndex)
                {
                    return kv.Key.GetUtf8String();
                }
            }
            throw new KeyNotFoundException();
        }
    }
}
