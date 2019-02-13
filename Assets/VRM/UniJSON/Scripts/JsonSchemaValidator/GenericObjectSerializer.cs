using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace UniJSON
{
    static class GenericObjectSerializer<T>
    {
        class Serializer
        {
            delegate void FieldSerializer(JsonSchema s, JsonSchemaValidationContext c, IFormatter f, T o,
                Dictionary<string, ValidationResult> vRes, string[] deps);

            Dictionary<string, FieldSerializer> m_serializers;

            static FieldSerializer CreateFieldSerializer(FieldInfo fi)
            {
                var mi = typeof(Serializer).GetMethod("_CreateFieldSerializer",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var g = mi.MakeGenericMethod(fi.FieldType);
                return GenericInvokeCallFactory.StaticFunc<FieldInfo, FieldSerializer>(g)(fi);
            }

            static FieldSerializer _CreateFieldSerializer<U>(FieldInfo fi)
            {
                Func<T, U> getter = t =>
                {
                    return (U)fi.GetValue(t);
                };

                return (s, c, f, o, vRes, deps) =>
                {
                    var v = s.Validator;
                    var field = getter(o);

                    if (vRes[fi.Name].Ex != null)
                    {
                        return;
                    }

                    if (deps != null)
                    {
                        foreach (var dep in deps)
                        {
                            if (vRes[dep].Ex != null)
                            {
                                return;
                            }
                        }
                    }

                    f.Key(fi.Name);
                    v.Serialize(f, c, field);
                };
            }

            public Serializer()
            {
                var serializers = new Dictionary<string, FieldSerializer>();
                GenericFieldView<T>.CreateFieldProcessors<Serializer, FieldSerializer>(
                    CreateFieldSerializer, serializers);

                m_serializers = serializers;
            }

            public void Serialize(JsonObjectValidator objectValidator,
                IFormatter f, JsonSchemaValidationContext c, T o)
            {
                // Validates fields
                var validationResults = new Dictionary<string, ValidationResult>();
                GenericObjectValidator<T>.ValidationResults(
                    objectValidator.Required, objectValidator.Properties,
                    c, o, validationResults);

                // Serialize fields
                f.BeginMap(objectValidator.Properties.Count());
                foreach (var property in objectValidator.Properties)
                {
                    var fieldName = property.Key;
                    var schema = property.Value;

                    string[] deps = null;
                    objectValidator.Dependencies.TryGetValue(fieldName, out deps);

                    FieldSerializer fs;
                    if (m_serializers.TryGetValue(fieldName, out fs))
                    {
                        fs(schema, c, f, o, validationResults, deps);
                    }
                }
                f.EndMap();
            }
        }

        static FieldInfo[] s_fields;
        static Serializer s_serializer;

        public static void Serialize(JsonObjectValidator objectValidator,
                IFormatter f, JsonSchemaValidationContext c, T value)
        {
            if (s_serializer == null)
            {
                s_serializer = new Serializer();
            }

            s_serializer.Serialize(objectValidator, f, c, value);
        }
    }
}
