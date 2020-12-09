using System.Collections.Generic;


namespace UniJSON
{
    public static class ListTreeNodeExtensions
    {
        #region IValue
        public static bool IsNull<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Null;
        }

        public static bool IsBoolean<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Boolean;
        }

        public static bool IsString<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.String;
        }

        public static bool IsInteger<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Integer;
        }

        public static bool IsFloat<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Number
                   || self.Value.ValueType == ValueNodeType.NaN
                   || self.Value.ValueType == ValueNodeType.Infinity
                   || self.Value.ValueType == ValueNodeType.MinusInfinity;
        }

        public static bool IsArray<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Array;
        }

        public static bool IsMap<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Object;
        }

        public static bool GetBoolean<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetBoolean(); }
        public static string GetString<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetString(); }
        public static Utf8String GetUtf8String<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUtf8String(); }
        public static sbyte GetSByte<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetSByte(); }
        public static short GetInt16<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetInt16(); }
        public static int GetInt32<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetInt32(); }
        public static long GetInt64<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetInt64(); }
        public static byte GetByte<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetByte(); }
        public static ushort GetUInt16<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUInt16(); }
        public static uint GetUInt32<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUInt32(); }
        public static ulong GetUInt64<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUInt64(); }
        public static float GetSingle<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetSingle(); }
        public static double GetDouble<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetDouble(); }

        /// <summary>
        /// for UnitTest. Use explicit GetT() or Deserialize(ref T)
        /// </summary>
        /// <returns></returns>
        public static object GetValue<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.GetValue<object>();
        }
        #endregion

        public static IEnumerable<ListTreeNode<T>> Traverse<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            yield return self;
            if (self.IsArray())
            {
                foreach (var x in self.ArrayItems())
                {
                    foreach (var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.IsMap())
            {
                foreach (var kv in self.ObjectItems())
                {
                    foreach (var y in kv.Value.Traverse())
                    {
                        yield return y;
                    }
                }
            }
        }
    }
}
