using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    static class GenericObjectDeserializer<S, T>
        where S : IListTreeItem, IValue<S>
    {
        delegate T Deserializer(ListTreeNode<S> src);

        static Deserializer s_d;

        delegate void FieldSetter(ListTreeNode<S> s, object o);
        static FieldSetter GetFieldDeserializer<U>(FieldInfo fi)
        {
            return (s, o) =>
            {
                var u = default(U);
                s.Deserialize(ref u);
                fi.SetValue(o, u);
            };
        }

        static U DeserializeField<U>(JsonSchema prop, ListTreeNode<S> s)
        {
            var u = default(U);
            prop.Validator.Deserialize(s, ref u);
            return u;
        }

        public static void Deserialize(ListTreeNode<S> src, ref T dst, Dictionary<string, JsonSchema> props)
        {
            if (s_d == null)
            {
                var target = typeof(T);

                var fields = target.GetFields(BindingFlags.Instance | BindingFlags.Public);
                var fieldDeserializers = fields.ToDictionary(x => Utf8String.From(x.Name), x =>
                {
                    /*
                    var mi = typeof(GenericDeserializer<T>).GetMethod("GetFieldDeserializer",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    var g = mi.MakeGenericMethod(x.FieldType);
                    return (FieldSetter)g.Invoke(null, new object[] { x });
                    */
                    JsonSchema prop;
                    if (!props.TryGetValue(x.Name, out prop))
                    {
                        return null;
                    }

                    var mi = typeof(GenericObjectDeserializer<S, T>).GetMethod("DeserializeField",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    var g = mi.MakeGenericMethod(x.FieldType);

                    return (FieldSetter)((s, o) =>
                    {
                        var f = g.Invoke(null, new object[] { prop, s });
                        x.SetValue(o, f);
                    });
                });

                s_d = (ListTreeNode<S> s) =>
                {
                    if (!s.IsMap())
                    {
                        throw new ArgumentException(s.Value.ValueType.ToString());
                    }

                    // boxing
                    var t = (object)Activator.CreateInstance<T>();
                    foreach (var kv in s.ObjectItems())
                    {
                        FieldSetter setter;
                        if (fieldDeserializers.TryGetValue(kv.Key.GetUtf8String(), out setter))
                        {
                            if (setter != null)
                            {
                                setter(kv.Value, t);
                            }
                        }
                    }
                    return (T)t;
                };

            }
            dst = s_d(src);
        }
    }
}
