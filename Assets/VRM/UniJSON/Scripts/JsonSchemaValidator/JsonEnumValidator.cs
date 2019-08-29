using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static class JsonEnumValidator
    {
        public static IJsonSchemaValidator Create(ListTreeNode<JsonValue> value)
        {
            foreach (var x in value.ArrayItems())
            {
                if (x.IsInteger() || x.IsFloat())
                {
                    return JsonIntEnumValidator.Create(value.ArrayItems()
                        .Where(y => y.IsInteger() || y.IsFloat())
                        .Select(y => y.GetInt32())
                        );
                }
                else if (x.IsString())
                {

                    return JsonStringEnumValidator.Create(value.ArrayItems()
                        .Where(y => y.IsString())
                        .Select(y => y.GetString())
                        , EnumSerializationType.AsString
                        );
                }
                else
                {
                }
            }

            throw new NotImplementedException();
        }

        public static IJsonSchemaValidator Create(IEnumerable<JsonSchema> composition, EnumSerializationType type)
        {
            foreach (var x in composition)
            {
                if (x.Validator is JsonStringEnumValidator)
                {
                    return JsonStringEnumValidator.Create(composition
                        .Select(y => y.Validator as JsonStringEnumValidator)
                        .Where(y => y != null)
                        .SelectMany(y => y.Values),
                        type
                        );
                }
                if (x.Validator is JsonIntEnumValidator)
                {
                    return JsonIntEnumValidator.Create(composition
                        .Select(y => y.Validator as JsonIntEnumValidator)
                        .Where(y => y != null)
                        .SelectMany(y => y.Values)
                        );
                }
            }

            throw new NotImplementedException();
        }

        static IEnumerable<string> GetStringValues(Type t, object[] excludes, Func<String, String> filter)
        {
            foreach (var x in Enum.GetValues(t))
            {
                if (excludes == null || !excludes.Contains(x))
                {
                    yield return filter(x.ToString());
                }
            }
        }

        static IEnumerable<int> GetIntValues(Type t, object[] excludes)
        {
            foreach (var x in Enum.GetValues(t))
            {
                if (excludes == null || !excludes.Contains(x))
                {
                    yield return (int)x;
                }
            }
        }

        public static IJsonSchemaValidator Create(Type t, EnumSerializationType serializationType, object[] excludes)
        {
            switch (serializationType)
            {
                case EnumSerializationType.AsInt:
                    return JsonIntEnumValidator.Create(GetIntValues(t, excludes));

                case EnumSerializationType.AsString:
                    return JsonStringEnumValidator.Create(GetStringValues(t, excludes, x => x), serializationType);

                case EnumSerializationType.AsLowerString:
                    return JsonStringEnumValidator.Create(GetStringValues(t, excludes, x => x.ToLower()), serializationType);

                case EnumSerializationType.AsUpperString:
                    return JsonStringEnumValidator.Create(GetStringValues(t, excludes, x => x.ToUpper()), serializationType);

                default:
                    throw new NotImplementedException();
            }
        }

        public static IJsonSchemaValidator Create(object[] values, EnumSerializationType type)
        {
            foreach (var x in values)
            {
                if (x is string)
                {
                    return JsonStringEnumValidator.Create(values.Select(y => (string)y), type);
                }
                if (x is int)
                {
                    return JsonIntEnumValidator.Create(values.Select(y => (int)y));
                }
            }

            throw new NotImplementedException();
        }
    }

    public class JsonStringEnumValidator : IJsonSchemaValidator
    {
        EnumSerializationType SerializationType;

        public String[] Values
        {
            get; set;
        }

        JsonStringEnumValidator(IEnumerable<string> values, EnumSerializationType type)
        {
            SerializationType = type;
            switch (SerializationType)
            {
                case EnumSerializationType.AsString:
                    Values = values.ToArray();
                    break;

                case EnumSerializationType.AsLowerString:
                    Values = values.Select(x => x.ToLower()).ToArray();
                    break;

                case EnumSerializationType.AsUpperString:
                    Values = values.Select(x => x.ToUpper()).ToArray();
                    break;

                case EnumSerializationType.AsInt:
                    throw new ArgumentException("JsonStringEnumValidator not allow AsInt");

                default:
                    throw new NotImplementedException("");
            }
        }

        public static JsonStringEnumValidator Create(IEnumerable<string> values, EnumSerializationType type)
        {
            return new JsonStringEnumValidator(values, type);
        }

        public override int GetHashCode()
        {
            return 7;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonStringEnumValidator;
            if (rhs == null) return false;

            if (Values.Length != rhs.Values.Length) return false;

            var l = Values.OrderBy(x => x).GetEnumerator();
            var r = rhs.Values.OrderBy(x => x).GetEnumerator();
            while (l.MoveNext() && r.MoveNext())
            {
                if (l.Current != r.Current)
                {
                    return false;
                }
            }
            return true;
        }

        public void Merge(IJsonSchemaValidator obj)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value)
        {
            throw new NotImplementedException();
        }

        public void ToJsonSchema(IFormatter f)
        {
            f.Key("type"); f.Value("string");
            f.Key("enum");
            f.BeginList(Values.Length);
            foreach (var x in Values)
            {
                f.Value(x);
            }
            f.EndList();
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            if (o == null)
            {
                return new JsonSchemaValidationException(c, "null");
            }

            var t = o.GetType();
            string value = null;
            if (t.IsEnum)
            {
                value = Enum.GetName(t, o);
            }
            else
            {
                value = GenericCast<T, string>.Cast(o);
            }

            if (SerializationType == EnumSerializationType.AsLowerString)
            {
                value = value.ToLower();
            }
            else if (SerializationType == EnumSerializationType.AsUpperString)
            {
                value = value.ToUpper();
            }

            if (Values.Contains(value))
            {
                return null;
            }
            else
            {
                return new JsonSchemaValidationException(c, string.Format("{0} is not valid enum", o));
            }
        }

        public static class GenericSerializer<T>
        {
            delegate void Serializer(JsonStringEnumValidator v,
                IFormatter f, JsonSchemaValidationContext c, T o);

            static Serializer s_serializer;

            public static void Serialize(JsonStringEnumValidator validator,
                IFormatter f, JsonSchemaValidationContext c, T o)
            {
                if (s_serializer == null)
                {
                    var t = typeof(T);
                    if (t.IsEnum)
                    {
                        s_serializer = (vv, ff, cc, oo) =>
                        {
                            var value = Enum.GetName(t, oo);
                            if (vv.SerializationType == EnumSerializationType.AsLowerString)
                            {
                                value = value.ToLower();
                            }
                            else if (vv.SerializationType == EnumSerializationType.AsUpperString)
                            {
                                value = value.ToUpper();
                            }
                            ff.Value(value);
                        };
                    }
                    else if (t == typeof(string))
                    {
                        s_serializer = (vv, ff, cc, oo) =>
                        {
                            var value = GenericCast<T, string>.Cast(oo);
                            if (vv.SerializationType == EnumSerializationType.AsLowerString)
                            {
                                value = value.ToLower();
                            }
                            else if (vv.SerializationType == EnumSerializationType.AsUpperString)
                            {
                                value = value.ToUpper();
                            }
                            ff.Value(value);
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                s_serializer(validator, f, c, o);
            }
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T o)
        {
            GenericSerializer<T>.Serialize(this, f, c, o);
        }

        static class GenericDeserializer<T, U>
            where T : IListTreeItem, IValue<T>
        {
            delegate U Deserializer(ListTreeNode<T> src);
            static Deserializer s_d;
            public static void Deserialize(ListTreeNode<T> src, ref U t)
            {
                if (s_d == null)
                {
                    if (typeof(U).IsEnum)
                    {
                        // enum from string
                        var mi = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).First(
                            x => x.Name == "Parse" && x.GetParameters().Length == 3
                            );

                        var enumParse = GenericInvokeCallFactory.StaticFunc<Type, string, bool, object>(mi);
                        s_d = x =>
                        {
                            var enumValue = enumParse(typeof(U), x.GetString(), true);
                            return GenericCast<object, U>.Cast(enumValue);
                        };
                    }
                    else
                    {
                        s_d = x => GenericCast<string, U>.Cast(x.GetString());
                    }
                }
                t = s_d(src);
            }
        }

        public void Deserialize<T, U>(ListTreeNode<T> src, ref U dst)
            where T : IListTreeItem, IValue<T>
        {
            GenericDeserializer<T, U>.Deserialize(src, ref dst);
        }
    }

    public class JsonIntEnumValidator : IJsonSchemaValidator
    {
        public int[] Values
        {
            get; set;
        }

        public static JsonIntEnumValidator Create(IEnumerable<int> values)
        {
            return new JsonIntEnumValidator
            {
                Values = values.ToArray()
            };
        }

        public override int GetHashCode()
        {
            return 7;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonIntEnumValidator;
            if (rhs == null) return false;

            if (Values.Length != rhs.Values.Length) return false;

            var l = Values.OrderBy(x => x).GetEnumerator();
            var r = rhs.Values.OrderBy(x => x).GetEnumerator();
            while (l.MoveNext() && r.MoveNext())
            {
                if (l.Current != r.Current)
                {
                    return false;
                }
            }
            return true;
        }

        public void Merge(IJsonSchemaValidator obj)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value)
        {
            throw new NotImplementedException();
        }

        public void ToJsonSchema(IFormatter f)
        {
            f.Key("type"); f.Value("integer");
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            if (Values.Contains(GenericCast<T, int>.Cast(o)))
            {
                return null;
            }
            else
            {
                return new JsonSchemaValidationException(c, string.Format("{0} is not valid enum", o));
            }
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T o)
        {
            f.Serialize(GenericCast<T, int>.Cast(o));
        }

        static class GenericDeserializer<T, U>
            where T : IListTreeItem, IValue<T>
        {
            delegate U Deserializer(ListTreeNode<T> src);

            static Deserializer s_d;

            public static void Deserialize(ListTreeNode<T> src, ref U dst)
            {
                if (s_d == null)
                {
                    // enum from int
                    s_d = s => GenericCast<int, U>.Cast(s.GetInt32());
                }
                dst = s_d(src);
            }
        }

        public void Deserialize<T, U>(ListTreeNode<T> src, ref U dst)
            where T : IListTreeItem, IValue<T>
        {
            GenericDeserializer<T, U>.Deserialize(src, ref dst);
        }
    }
}
