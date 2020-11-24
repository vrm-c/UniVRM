using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class ListTreeNodeArrayExtensions
    {
        public static IEnumerable<ListTreeNode<T>> ArrayItems<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            if (!self.IsArray()) throw new DeserializationException("is not array");
            return self.Children;
        }

        [Obsolete("Use GetArrayItem(index)")]
        public static ListTreeNode<T> GetArrrayItem<T>(this ListTreeNode<T> self, int index)
            where T : IListTreeItem, IValue<T>
        {
            return GetArrayItem(self, index);
        }

        public static ListTreeNode<T> GetArrayItem<T>(this ListTreeNode<T> self, int index)
            where T : IListTreeItem, IValue<T>
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

        public static int GetArrayCount<T>(this ListTreeNode<T> self) 
            where T : IListTreeItem, IValue<T>
        {
            if (!self.IsArray()) throw new DeserializationException("is not array");
            return self.Children.Count();
        }

        public static int IndexOf<T>(this ListTreeNode<T> self, ListTreeNode<T> child) 
            where T : IListTreeItem, IValue<T>
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
