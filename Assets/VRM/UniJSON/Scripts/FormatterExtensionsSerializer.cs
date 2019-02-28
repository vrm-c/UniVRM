using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UniJSON
{
    public static class FormatterExtensionsSerializer
    {
        public static void SerializeDictionary(this IFormatter f, IDictionary<string, object> dictionary)
        {
            f.BeginMap(dictionary.Count);
            foreach (var kv in dictionary)
            {
                f.Key(kv.Key);
                f.SerializeObject(kv.Value);
            }
            f.EndMap();
        }

        public static void SerializeArray<T>(this IFormatter f, IEnumerable<T> values)
        {
            f.BeginList(values.Count());
            foreach (var value in values)
            {
                f.Serialize(value);
            }
            f.EndList();
        }

        public static void SerializeObjectArray(this IFormatter f, object[] array)
        {
            f.BeginList(array.Length);
            foreach (var x in array)
            {
                f.SerializeObject(x);
            }
            f.EndList();
        }

        public static void SerializeObject(this IFormatter f, object value)
        {
            if (value == null)
            {
                f.Null();
            }
            else
            {
                typeof(FormatterExtensionsSerializer).GetMethod("Serialize")
                    .MakeGenericMethod(value.GetType()).Invoke(null, new object[] { f, value });
            }
        }

        public static void Serialize<T>(this IFormatter f, T arg)
        {
            if (arg == null)
            {
                f.Null();
                return;
            }

            GenericSerializer<T>.Serialize(f, arg);
        }

        public static void SetCustomSerializer<T>(Action<IFormatter, T> serializer)
        {
            GenericSerializer<T>.Set(serializer);
        }

        public static MethodInfo GetMethod(string name)
        {
            return typeof(FormatterExtensionsSerializer).GetMethod(name);
        }
    }

    static class GenericSerializer<T>
    {
        delegate void Serializer(IFormatter f, T t);

        static Action<IFormatter, T> GetSerializer()
        {
            var t = typeof(T);

            // object
            if (typeof(T) == typeof(object) && t.GetType() != typeof(object))
            {
                var mi = FormatterExtensionsSerializer.GetMethod("SerializeObject");
                return GenericInvokeCallFactory.StaticAction<IFormatter, T>(mi);
            }

            try
            {
                // primitive
                var mi = typeof(IFormatter).GetMethod("Value", new Type[] { t });
                if (mi != null)
                {
                    return GenericInvokeCallFactory.OpenAction<IFormatter, T>(mi);
                }
            }
            catch (AmbiguousMatchException)
            {
                // do nothing
            }

            {
                // dictionary
                var idictionary = t.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType
                && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                && x.GetGenericArguments()[0] == typeof(string)
                );
                if (idictionary != null)
                {
                    var mi = FormatterExtensionsSerializer.GetMethod("SerializeDictionary");
                    return GenericInvokeCallFactory.StaticAction<IFormatter, T>(mi);
                }
            }

            {
                // object[]
                if (t == typeof(object[]))
                {
                    var mi = FormatterExtensionsSerializer.GetMethod("SerializeObjectArray");
                    return GenericInvokeCallFactory.StaticAction<IFormatter, T>(mi);
                }
            }

            {
                // list
                var ienumerable = t.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType
                && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                );
                if (ienumerable != null)
                {
                    var g = FormatterExtensionsSerializer.GetMethod("SerializeArray");
                    var mi = g.MakeGenericMethod(ienumerable.GetGenericArguments());
                    return GenericInvokeCallFactory.StaticAction<IFormatter, T>(mi);
                }
            }

            {
                // reflection
                var schema = JsonSchema.FromType<T>();
                return (IFormatter f, T value) =>
                {
                    var c = new JsonSchemaValidationContext(value)
                    {
                        EnableDiagnosisForNotRequiredFields = true
                    };
                    schema.Serialize(f, value, c);
                };
            }


            //throw new NotImplementedException();
        }

        static Serializer s_serializer;

        public static void Set(Action<IFormatter, T> serializer)
        {
            s_serializer = new Serializer(serializer);
        }

        public static void Serialize(IFormatter f, T t)
        {
            if (s_serializer == null)
            {
                s_serializer = new Serializer(GetSerializer());
            }
            s_serializer(f, t);
        }
    }
}
