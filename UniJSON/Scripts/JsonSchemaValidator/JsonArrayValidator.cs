using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;


namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4
    /// </summary>
    public class JsonArrayValidator : IJsonSchemaValidator
    {
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.1
        /// </summary>
        public JsonSchema Items
        {
            get; set;
        }

        // additionalItems

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.3
        /// </summary>
        public int? MaxItems
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.4
        /// </summary>
        public int? MinItems
        {
            get; set;
        }

        // uniqueItems

        // contains

        public override int GetHashCode()
        {
            return 5;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonArrayValidator;
            if (rhs == null) return false;

            if (Items != rhs.Items) return false;
            if (MaxItems != rhs.MaxItems) return false;
            if (MinItems != rhs.MinItems) return false;

            return true;
        }

        public void Merge(IJsonSchemaValidator rhs)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value)
        {
            switch (key)
            {
                case "items":
                    if (value.IsArray())
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var sub = new JsonSchema();
                        sub.Parse(fs, value, "items");
                        Items = sub;
                    }
                    return true;

                case "additionalItems":
                    return true;

                case "maxItems":
                    MaxItems = value.GetInt32();
                    return true;

                case "minItems":
                    MinItems = value.GetInt32();
                    return true;

                case "uniqueItems":
                    return true;

                case "contains":
                    return true;
            }

            return false;
        }

        static class GenericCounter<T>
        {
            delegate int Counter(T value);

            static Counter s_counter;

            public static int Count(T value)
            {
                if (s_counter == null)
                {
                    var t = typeof(T);
                    if (t.IsArray)
                    {
                        var pi = t.GetProperty("Length");
                        var v = Expression.Parameter(t, "value");
                        var call = Expression.Property(v, pi);
                        var compiled = (Func<T, int>)Expression.Lambda(call, v).Compile();
                        s_counter = new Counter(compiled);
                    }
                    else if (t.GetIsGenericList())
                    {
                        var pi = t.GetProperty("Count");
                        var v = Expression.Parameter(t, "value");
                        var call = Expression.Property(v, pi);
                        var compiled = (Func<T, int>)Expression.Lambda(call, v).Compile();
                        s_counter = new Counter(compiled);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return s_counter(value);
            }
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext context, T o)
        {
            if (o == null)
            {
                return new JsonSchemaValidationException(context, "null");
            }

            var count = GenericCounter<T>.Count(o);
            if (count == 0)
            {
                return new JsonSchemaValidationException(context, "empty");
            }

            if (MaxItems.HasValue && count > MaxItems.Value)
            {
                return new JsonSchemaValidationException(context, "maxOtems");
            }

            if (MinItems.HasValue && count < MinItems.Value)
            {
                return new JsonSchemaValidationException(context, "minItems");
            }

            return null;
        }

        static void ArraySerializer<U>(IJsonSchemaValidator v, IFormatter f, JsonSchemaValidationContext c, U[] array)
        {
            f.BeginList(array.Length);
            {
                //int i = 0;
                foreach (var x in array)
                {
                    //using (c.Push(i++))
                    {
                        v.Serialize(f, c, x);
                    }
                }
            }
            f.EndList();
        }

        static void ListSerializer<U>(IJsonSchemaValidator v, IFormatter f, JsonSchemaValidationContext c, List<U> list)
        {
            f.BeginList(list.Count);
            {
                //int i = 0;
                foreach (var x in list)
                {
                    //using (c.Push(i++))
                    {
                        v.Serialize(f, c, x);
                    }
                }
            }
            f.EndList();
        }

        static class GenericSerializer<T>
        {
            delegate void Serializer(IJsonSchemaValidator v, IFormatter f, JsonSchemaValidationContext c, T o);

            static Serializer s_serializer;

            public static void Serialize(IJsonSchemaValidator v, IFormatter f, JsonSchemaValidationContext c, T o)
            {
                if (s_serializer == null)
                {
                    var t = typeof(T);
                    MethodInfo g = null;
                    if (t.IsArray)
                    {
                        var mi = typeof(JsonArrayValidator).GetMethod("ArraySerializer",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        g = mi.MakeGenericMethod(t.GetElementType());
                    }
                    else if (t.GetIsGenericList())
                    {
                        // ToDo: IList
                        var mi = typeof(JsonArrayValidator).GetMethod("ListSerializer",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        g = mi.MakeGenericMethod(t.GetGenericArguments());
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    var vv = Expression.Parameter(typeof(IJsonSchemaValidator), "v");
                    var ff = Expression.Parameter(typeof(IFormatter), "f");
                    var cc = Expression.Parameter(typeof(JsonSchemaValidationContext), "c");
                    var oo = Expression.Parameter(typeof(T), "o");
                    var call = Expression.Call(g, vv, ff, cc, oo);
                    var compiled = (Action<IJsonSchemaValidator, IFormatter, JsonSchemaValidationContext, T>)Expression.Lambda(call, vv, ff, cc, oo).Compile();
                    s_serializer = new Serializer(compiled);
                }
                s_serializer(v, f, c, o);
            }
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T o)
        {
            GenericSerializer<T>.Serialize(Items.Validator, f, c, o);
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("array");

            if (Items != null)
            {
                f.Key("items");
                Items.ToJson(f);
            }
        }

        public void Deserialize<T, U>(ListTreeNode<T> src, ref U dst) 
            where T : IListTreeItem, IValue<T>
        {
            src.Deserialize(ref dst);
        }
    }
}
