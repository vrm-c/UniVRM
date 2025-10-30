using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class ListTreeNodeArrayExtensions
    {
        public static IEnumerable<JsonNode> ArrayItems(this JsonNode self)
        {
            if (!self.IsArray()) throw new DeserializationException("is not array");
            return self.Children;
        }

        [Obsolete("Use GetArrayItem(index)")]
        public static JsonNode GetArrrayItem(this JsonNode self, int index)
        {
            return GetArrayItem(self, index);
        }

        public static JsonNode GetArrayItem(this JsonNode self, int index)
        {
            int i = 0;
            foreach (var v in self.ArrayItems())
            {
                if (i++ == index)
                {
                    return v;
                }
            }
            throw new KeyNotFoundException();
        }

        public static int GetArrayCount(this JsonNode self)
        {
            if (!self.IsArray()) throw new DeserializationException("is not array");
            return self.Children.Count();
        }

        public static int IndexOf(this JsonNode self, JsonNode child)
        {
            int i = 0;
            foreach (var v in self.ArrayItems())
            {
                if (v.ValueIndex == child.ValueIndex)
                {
                    return i;
                }
                ++i;
            }
            throw new KeyNotFoundException();
        }
    }
}
