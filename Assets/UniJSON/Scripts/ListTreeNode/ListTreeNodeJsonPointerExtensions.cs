using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class ListTreeNodeJsonPointerExtensions
    {
        public static void SetValue<T>(this ListTreeNode<T> self, 
            Utf8String jsonPointer, ArraySegment<Byte> bytes)
            where T: IListTreeItem, IValue<T>
        {
            foreach (var node in self.GetNodes(jsonPointer))
            {
                node.SetValue(default(T).New(
                    bytes,
                    ValueNodeType.Boolean,
                    node.Value.ParentIndex));
            }
        }

        public static void RemoveValue<T>(this ListTreeNode<T> self, Utf8String jsonPointer)
            where T : IListTreeItem, IValue<T>
        {
            foreach (var node in self.GetNodes(new JsonPointer(jsonPointer)))
            {
                if (node.Parent.IsMap())
                {
                    node.Prev.SetValue(default(T)); // remove key
                }
                node.SetValue(default(T)); // remove
            }
        }

        public static JsonPointer Pointer<T>(this ListTreeNode<T> self)
            where T: IListTreeItem, IValue<T>
        {
            return JsonPointer.Create(self);
        }

        public static IEnumerable<ListTreeNode<T>> Path<T>(this ListTreeNode<T> self)
            where T : IListTreeItem, IValue<T>
        {
            if (self.HasParent)
            {
                foreach (var x in self.Parent.Path())
                {
                    yield return x;
                }
            }
            yield return self;
        }

        public static IEnumerable<ListTreeNode<T>> GetNodes<T>(this ListTreeNode<T> self, 
            JsonPointer jsonPointer)
            where T : IListTreeItem, IValue<T>
        {
            if (jsonPointer.Path.Count == 0)
            {
                yield return self;
                yield break;
            }

            if (self.IsArray())
            {
                // array
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var child in self.ArrayItems())
                    {
                        foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                        {
                            yield return childChild;
                        }
                    }
                }
                else
                {
                    int index = jsonPointer[0].ToInt32();
                    var child = self.ArrayItems().Skip(index).First();
                    foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                    {
                        yield return childChild;
                    }
                }
            }
            else if (self.IsMap())
            {
                // object
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var kv in self.ObjectItems())
                    {
                        foreach (var childChild in kv.Value.GetNodes(jsonPointer.Unshift()))
                        {
                            yield return childChild;
                        }
                    }
                }
                else
                {
                    ListTreeNode<T> child;
                    try
                    {
                        child = self.ObjectItems().First(x => x.Key.GetUtf8String() == jsonPointer[0]).Value;
                    }
                    catch (Exception)
                    {
                        // key
                        self.AddKey(jsonPointer[0]);
                        // value
                        self.AddValue(default(ArraySegment<byte>), ValueNodeType.Object);

                        child = self.ObjectItems().First(x => x.Key.GetUtf8String() == jsonPointer[0]).Value;
                    }
                    foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                    {
                        yield return childChild;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<ListTreeNode<T>> GetNodes<T>(this ListTreeNode<T> self, 
            Utf8String jsonPointer) 
            where T : IListTreeItem, IValue<T>
        {
            return self.GetNodes(new JsonPointer(jsonPointer));
        }
    }
}
