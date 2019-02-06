using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static class ListTreeNodeDeserializerExtensions
    {
        public static void Deserialize<T, U>(this ListTreeNode<T> self, ref U value)
            where T : IListTreeItem, IValue<T>
        {
            GenericDeserializer<T, U>.Deserialize(self, ref value);
        }
    }
}
