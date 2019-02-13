using System;
using System.Collections.Generic;
using System.Reflection;


namespace UniJSON
{
    static class GenericFieldView<T>
    {
        public static FieldInfo[] GetFields()
        {
            var t = typeof(T);
            return t.GetFields(BindingFlags.Instance | BindingFlags.Public);
        }

        public static void CreateFieldProcessors<G, D>(
            Func<FieldInfo, D> creator,
            Dictionary<string, D> processors
            )
        {
            foreach (var fi in GetFields())
            {
                processors.Add(fi.Name, creator(fi));
            }
        }
    }

    class ValidationResult
    {
        public bool IsIgnorable;
        public JsonSchemaValidationException Ex;
    }

    static class GenericObjectValidator<T>
    {
        class ObjectValidator
        {
            delegate JsonSchemaValidationException FieldValidator(
                JsonSchema s, JsonSchemaValidationContext c, T o, out bool isIgnorable);

            Dictionary<string, FieldValidator> m_validators;

            static FieldValidator CreateFieldValidator(FieldInfo fi)
            {
                var mi = typeof(ObjectValidator).GetMethod("_CreateFieldValidator",
                    BindingFlags.Static | BindingFlags.NonPublic)
                    ;
                var g = mi.MakeGenericMethod(fi.FieldType);
                return GenericInvokeCallFactory.StaticFunc<FieldInfo, FieldValidator>(g)(fi);
            }

            static FieldValidator _CreateFieldValidator<U>(FieldInfo fi)
            {
                var getter = (Func<T, U>)((t) => (U)fi.GetValue(t));

                return (JsonSchema s, JsonSchemaValidationContext c, T o, out bool isIgnorable) =>
                {
                    var v = s.Validator;
                    using (c.Push(fi.Name))
                    {
                        var field = getter(o);
                        var ex = v.Validate(c, field);

                        isIgnorable = ex != null && s.IsExplicitlyIgnorableValue(field);

                        return ex;
                    }
                };
            }

            public ObjectValidator()
            {
                var validators = new Dictionary<string, FieldValidator>();
                GenericFieldView<T>.CreateFieldProcessors<ObjectValidator, FieldValidator>(
                    CreateFieldValidator, validators);

                m_validators = validators;
            }

            public JsonSchemaValidationException ValidateProperty(
                HashSet<string> required,
                KeyValuePair<string, JsonSchema> property,
                JsonSchemaValidationContext c,
                T o,
                out bool isIgnorable
                )
            {
                var fieldName = property.Key;
                var schema = property.Value;

                isIgnorable = false;

                FieldValidator fv;
                if (m_validators.TryGetValue(fieldName, out fv))
                {
                    var isRequired = required != null && required.Contains(fieldName);

                    bool isMemberIgnorable;
                    var ex = fv(schema, c, o, out isMemberIgnorable);
                    if (ex != null)
                    {
                        isIgnorable = !isRequired && isMemberIgnorable;

                        if (isRequired // required fields must be checked
                            || c.EnableDiagnosisForNotRequiredFields)
                        {
                            return ex;
                        }
                    }
                }

                return null;
            }

            public JsonSchemaValidationException Validate(
                HashSet<string> required,
                Dictionary<string, JsonSchema> properties,
                JsonSchemaValidationContext c, T o)
            {
                foreach (var kv in properties)
                {
                    bool isIgnorable;
                    var ex = ValidateProperty(required, kv, c, o, out isIgnorable);
                    if (ex != null && !isIgnorable)
                    {
                        return ex;
                    }
                }

                return null;
            }

            public void ValidationResults
                (HashSet<string> required,
                Dictionary<string, JsonSchema> properties,
                JsonSchemaValidationContext c, T o,
                Dictionary<string, ValidationResult> results)
            {
                foreach (var kv in properties)
                {
                    bool isIgnorable;
                    var ex = ValidateProperty(required, kv, c, o, out isIgnorable);

                    results.Add(kv.Key, new ValidationResult
                    {
                        IsIgnorable = isIgnorable,
                        Ex = ex,
                    });
                }
            }
        }

        static ObjectValidator s_validator;

        static void prepareValidator()
        {
            if (s_validator == null)
            {
                s_validator = new ObjectValidator();
            }
        }

        public static JsonSchemaValidationException Validate(HashSet<string> required,
            Dictionary<string, JsonSchema> properties,
            JsonSchemaValidationContext c, T o)
        {
            prepareValidator();
            return s_validator.Validate(required, properties, c, o);
        }

        internal static void ValidationResults(HashSet<string> required,
            Dictionary<string, JsonSchema> properties,
            JsonSchemaValidationContext c, T o,
            Dictionary<string, ValidationResult> results)
        {
            prepareValidator();
            s_validator.ValidationResults(required, properties, c, o, results);
        }
    }

}
