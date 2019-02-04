using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static class ListTreeNodeDeserializerExtensions
    {
        struct GenericCreator<T, U>
            where T : IListTreeItem, IValue<T>
        {
            static V[] ArrayCreator<V>(ListTreeNode<T> src)
            {
                if (!src.IsArray())
                {
                    throw new ArgumentException("value is not array");
                }
                var count = src.GetArrayCount();
                return new V[count];
            }

            static Func<ListTreeNode<T>, U> GetCreator()
            {
                var t = typeof(U);
                if (t.IsArray)
                {
                    var mi = typeof(GenericCreator<T, U>).GetMethod("ArrayCreator",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    var g = mi.MakeGenericMethod(t.GetElementType());
                    return GenericInvokeCallFactory.StaticFunc<ListTreeNode<T>, U>(g);
                }

                {
                    return _s =>
                    {
                        return Activator.CreateInstance<U>();
                    };
                }
            }

            delegate U Creator(ListTreeNode<T> src);

            static Creator s_creator;

            public U Create(ListTreeNode<T> src)
            {
                if (s_creator == null)
                {
                    var d = GetCreator();
                    s_creator = new Creator(d);
                }
                return s_creator(src);
            }
        }

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

    public static class GenericDeserializer<T, U>
        where T : IListTreeItem, IValue<T>
    {
        static V[] GenericArrayDeserializer<V>(ListTreeNode<T> s)
        {
            if (!s.IsArray())
            {
                throw new ArgumentException("not array: " + s.Value.ValueType);
            }
            var u = new V[s.GetArrayCount()];
            int i = 0;
            foreach (var x in s.ArrayItems())
            {
                x.Deserialize(ref u[i++]);
            }
            return u;
        }

        static List<V> GenericListDeserializer<V>(ListTreeNode<T> s)
        {
            if (!s.IsArray())
            {
                throw new ArgumentException("not array: " + s.Value.ValueType);
            }
            var u = new List<V>(s.GetArrayCount());
            foreach (var x in s.ArrayItems())
            {
                var e = default(V);
                x.Deserialize(ref e);
                u.Add(e);
            }
            return u;
        }

        delegate void FieldSetter(ListTreeNode<T> s, object o);
        static FieldSetter GetFieldDeserializer<V>(FieldInfo fi)
        {
            return (s, o) =>
            {
                var u = default(V);
                s.Deserialize(ref u);
                fi.SetValue(o, u);
            };
        }

        static Func<ListTreeNode<T>, U> GetDeserializer()
        {
            // primitive
            {
                var mi = typeof(ListTreeNode<T>).GetMethods().FirstOrDefault(x =>
                {
                    if (!x.Name.StartsWith("Get"))
                    {
                        return false;
                    }

                    if (!x.Name.EndsWith(typeof(U).Name))
                    {
                        return false;
                    }

                    var parameters = x.GetParameters();
                    if (parameters.Length != 0)
                    {
                        return false;
                    }

                    if (x.ReturnType != typeof(U))
                    {
                        return false;
                    }

                    return true;
                });

                if (mi != null)
                {
                    return GenericInvokeCallFactory.StaticFunc<ListTreeNode<T>, U>(mi);
                }
            }

            var target = typeof(U);

            if (target.IsArray)
            {
                var mi = typeof(GenericDeserializer<T, U>).GetMethod("GenericArrayDeserializer",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var g = mi.MakeGenericMethod(target.GetElementType());
                return GenericInvokeCallFactory.StaticFunc<ListTreeNode<T>, U>(g);
            }

            if (target.IsGenericType)
            {
                if (target.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var mi = typeof(GenericDeserializer<T, U>).GetMethod("GenericListDeserializer",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    var g = mi.MakeGenericMethod(target.GetGenericArguments());
                    return GenericInvokeCallFactory.StaticFunc<ListTreeNode<T>, U>(g);
                }

                if (target.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    target.GetGenericArguments()[0] == typeof(string))
                {
                    var mi = typeof(ListTreeNodeDeserializerExtensions).GetMethod("DictionaryDeserializer",
                    BindingFlags.Static | BindingFlags.NonPublic);
                    var g = mi.MakeGenericMethod(typeof(T));
                    return GenericInvokeCallFactory.StaticFunc<ListTreeNode<T>, U>(g);
                }
            }

            {
                var schema = JsonSchema.FromType<U>();
                return s =>
                {
                    var t = default(U);
                    schema.Validator.Deserialize(s, ref t);
                    return t;
                };
            }

#if false
                if (target.IsEnum)
                {
                    var value = Expression.Parameter(typeof(int), "value");
                    var cast = Expression.Convert(value, target);
                    var func = Expression.Lambda(cast, value);
                    var compiled = (Func<int, T>)func.Compile();
                    return s =>
                    {
                        return compiled(s.GetInt32());
                    };
                }

                {
                    var fields = target.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    var fieldDeserializers = fields.ToDictionary(x => Utf8String.From(x.Name), x =>
                    {
                        var mi = typeof(GenericDeserializer<S, T>).GetMethod("GetFieldDeserializer",
                            BindingFlags.Static|BindingFlags.NonPublic);
                        var g = mi.MakeGenericMethod(x.FieldType);
                        return (FieldSetter)g.Invoke(null, new object[] { x });
                    });
                    
                    return (S s) =>
                    {
                        if (!s.IsMap())
                        {
                            throw new ArgumentException(s.ValueType.ToString());
                        }

                        var t = (object)default(GenericCreator<S, T>).Create(s);
                        foreach(var kv in s.ObjectItems())
                        {
                            FieldSetter setter;
                            if (fieldDeserializers.TryGetValue(kv.Key, out setter))
                            {
                                setter(kv.Value, t);
                            }
                        }
                        return (T)t;
                    };
                }
#endif
        }

        public delegate U Deserializer(ListTreeNode<T> node);

        public static Deserializer s_deserializer;

        public static void Deserialize(ListTreeNode<T> node, ref U value)
        {
            if (s_deserializer == null)
            {
                var d = GetDeserializer();
                s_deserializer = new Deserializer(d);
            }
            value = s_deserializer(node);
        }

        public static void SetCustomDeserializer(Deserializer deserializer)
        {
            s_deserializer = deserializer;
        }
    }
}
