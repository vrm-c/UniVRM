using System;
using System.Linq;
using System.Collections.Generic;

namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5
    /// </summary>
    public class JsonDictionaryValidator<T> : IJsonSchemaValidator
    {
        public JsonDictionaryValidator()
        {
            AdditionalProperties = JsonSchema.FromType<T>();
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.1
        /// </summary>
        public int MaxProperties
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.2
        /// </summary>
        public int MinProperties
        {
            get; set;
        }

        List<string> m_required = new List<string>();
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.3
        /// </summary>
        public List<string> Required
        {
            get { return m_required; }
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.5
        /// </summary>
        public string PatternProperties
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.6
        /// </summary>
        public JsonSchema AdditionalProperties
        {
            get; set;
        }

        Dictionary<string, string[]> m_dependencies;
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.7
        /// </summary>
        public Dictionary<string, string[]> Dependencies
        {
            get
            {
                if (m_dependencies == null)
                {
                    m_dependencies = new Dictionary<string, string[]>();
                }
                return m_dependencies;
            }
        }

        public override int GetHashCode()
        {
            return 6;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonObjectValidator;
            if (rhs == null)
            {
                return false;
            }

            if (Required.Count != rhs.Required.Count)
            {
                return false;
            }
            if (!Required.OrderBy(x => x).SequenceEqual(rhs.Required.OrderBy(x => x)))
            {
                return false;
            }

            if (Dependencies.Count != rhs.Dependencies.Count)
            {
                return false;
            }
            foreach (var kv in Dependencies)
            {
                if (!kv.Value.OrderBy(x => x).SequenceEqual(rhs.Dependencies[kv.Key].OrderBy(x => x)))
                {
                    return false;
                }
            }

            if (AdditionalProperties == null
                && rhs.AdditionalProperties == null)
            {
                // ok
            }
            else if (AdditionalProperties == null)
            {
                return false;
            }
            else if (rhs.AdditionalProperties == null)
            {
                return false;
            }
            else
            {
                if (!AdditionalProperties.Equals(rhs.AdditionalProperties))
                {
                    return false;
                }
            }

            return true;
        }

        public void Merge(IJsonSchemaValidator obj)
        {
            var rhs = obj as JsonObjectValidator;
            if (rhs == null)
            {
                throw new ArgumentException();
            }

            foreach (var x in rhs.Required)
            {
                this.Required.Add(x);
            }

            if (rhs.AdditionalProperties != null)
            {
                if (AdditionalProperties != null)
                {
                    throw new NotImplementedException();
                }
                AdditionalProperties = rhs.AdditionalProperties;
            }
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value)
        {
            switch (key)
            {
                case "maxProperties":
                    MaxProperties = value.GetInt32();
                    return true;

                case "minProperties":
                    MinProperties = value.GetInt32();
                    return true;

                case "required":
                    {
                        foreach (var req in value.ArrayItems())
                        {
                            m_required.Add(req.GetString());
                        }
                    }
                    return true;

                case "patternProperties":
                    PatternProperties = value.GetString();
                    return true;

                case "additionalProperties":
                    {
                        var sub = new JsonSchema();
                        sub.Parse(fs, value, "additionalProperties");
                        AdditionalProperties = sub;
                    }
                    return true;

                case "dependencies":
                    {
                        foreach (var kv in value.ObjectItems())
                        {
                            Dependencies.Add(kv.Key.GetString(), kv.Value.ArrayItems().Select(x => x.GetString()).ToArray());
                        }
                    }
                    return true;

                case "propertyNames":
                    return true;
            }

            return false;
        }

        public void ToJsonSchema(IFormatter f)
        {
            f.Key("type"); f.Value("object");
        }

        public JsonSchemaValidationException Validate<S>(JsonSchemaValidationContext c, S o)
        {
            if (o == null)
            {
                return new JsonSchemaValidationException(c, "null");
            }

            var d = o as IDictionary<string, T>;
            if (d == null)
            {
                return new JsonSchemaValidationException(c, "not dictionary");
            }

            if (Required != null)
            {
                foreach (var x in Required)
                {
                    using (c.Push(x))
                    {
                        // ToDo
                    }
                }
            }

            if (AdditionalProperties != null)
            {
                foreach (var kv in d)
                {
                    using (c.Push(kv.Key))
                    {
                        var result = AdditionalProperties.Validator.Validate(c, kv.Value);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        Dictionary<string, object> m_validValueMap = new Dictionary<string, object>();

        public void Serialize<S>(IFormatter f, JsonSchemaValidationContext c, S o)
        {
            // validate properties
            m_validValueMap.Clear();

            var dict = o as Dictionary<string, T>;
            f.BeginMap(dict.Count);
            {
                foreach (var kv in dict)
                {
                    // key
                    f.Key(kv.Key);

                    // value
                    //using (c.Push(kv.Key))
                    {
                        AdditionalProperties.Validator.Serialize(f, c, kv.Value);
                    }
                }
            }
            f.EndMap();
        }

        public void Deserialize<U, V>(ListTreeNode<U> src, ref V dst)
            where U : IListTreeItem, IValue<U>
        {
            src.Deserialize(ref dst);
        }
    }

    public static class JsonDictionaryValidator
    {
        public static JsonDictionaryValidator<T> Create<T>()
        {
            return new JsonDictionaryValidator<T>();
        }

        #region AOT
        public static JsonDictionaryValidator<Single> CreateSingle()
        {
            return Create<Single>();
        }

        public static JsonDictionaryValidator<Int32> CreateInt32()
        {
            return Create<Int32>();
        }

        public static JsonDictionaryValidator<Boolean> CreateBoolean()
        {
            return Create<Boolean>();
        }
        #endregion
    }
}
