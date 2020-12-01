using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class IValueNodeObjectExtensions
    {
        public static IEnumerable<KeyValuePair<ListTreeNode<T>, ListTreeNode<T>>> ObjectItems<T>(this ListTreeNode<T> self)
            where T : IListTreeItem, IValue<T>
        {
            if (!self.IsMap()) throw new DeserializationException("is not object");
            var it = self.Children.GetEnumerator();
            while (it.MoveNext())
            {
                var key = it.Current;

                it.MoveNext();
                yield return new KeyValuePair<ListTreeNode<T>, ListTreeNode<T>>(key, it.Current);
            }
        }

        public static int GetObjectCount<T>(this ListTreeNode<T> self)
            where T : IListTreeItem, IValue<T>
        {
            if (!self.IsMap()) throw new DeserializationException("is not object");
            return self.Children.Count() / 2;
        }

        public static ListTreeNode<T> GetObjectItem<T>(this ListTreeNode<T> self, String key)
            where T : IListTreeItem, IValue<T>
        {
            return self.GetObjectItem(Utf8String.From(key));
        }

        public static ListTreeNode<T> GetObjectItem<T>(this ListTreeNode<T> self, Utf8String key)
            where T : IListTreeItem, IValue<T>

        {
            foreach (var kv in self.ObjectItems())
            {
                if (kv.Key.GetUtf8String() == key)
                {
                    return kv.Value;
                }
            }
            throw new KeyNotFoundException();
        }

        public static bool ContainsKey<T>(this ListTreeNode<T> self, Utf8String key)
            where T : IListTreeItem, IValue<T>
        {
            return self.ObjectItems().Any(x => x.Key.GetUtf8String() == key);
        }

        public static bool ContainsKey<T>(this ListTreeNode<T> self, String key)
            where T : IListTreeItem, IValue<T>
        {
            var ukey = Utf8String.From(key);
            return self.ContainsKey(ukey);
        }

        public static Utf8String KeyOf<T>(this ListTreeNode<T> self, ListTreeNode<T> node)
            where T : IListTreeItem, IValue<T>
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
