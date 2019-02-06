using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static class ListTreeNodeDeserializerExtensions
    {
        static object DictionaryDeserializer<T>(ListTreeNode<T> s)
            where T : IListTreeItem, IValue<T>
        {
            switch (s.Value.ValueType)
            {
                case ValueNodeType.Object:
                    {
                        var u = new Dictionary<string, object>();
                        foreach (var kv in s.ObjectItems())
                        {
                            //var e = default(object);
                            //kv.Value.Deserialize(ref e);
                            u.Add(kv.Key.GetString(), DictionaryDeserializer(kv.Value));
                        }
                        return u;
                    }

                case ValueNodeType.Null:
                    return null;

                case ValueNodeType.Boolean:
                    return s.GetBoolean();

                case ValueNodeType.Integer:
                    return s.GetInt32();

                case ValueNodeType.Number:
                    return s.GetDouble();

                case ValueNodeType.String:
                    return s.GetString();

                default:
                    throw new NotImplementedException(s.Value.ValueType.ToString());
            }
        }

        public static void Deserialize<T, U>(this ListTreeNode<T> self, ref U value)
            where T : IListTreeItem, IValue<T>
        {
            GenericDeserializer<T, U>.Deserialize(self, ref value);
        }
    }
}
