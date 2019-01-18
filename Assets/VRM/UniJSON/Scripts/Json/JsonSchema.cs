using System;
using System.Collections.Generic;
using System.Linq;

namespace UniJSON
{
    public class JsonSchema : IEquatable<JsonSchema>
    {
        public string Schema; // http://json-schema.org/draft-04/schema

        #region Annotations
        string m_title;
        public string Title
        {
            get { return m_title; }
            private set
            {
                if (value == null)
                {
                    m_title = "";
                }
                else
                {
                    m_title = value.Trim();
                }
            }
        }

        string m_desc;
        public string Description
        {
            get { return m_desc; }
            private set
            {
                if (value == null)
                {
                    m_desc = "";
                }
                else
                {
                    m_desc = value.Trim();
                }
            }
        }

        public object Default
        {
            get;
            private set;
        }
        #endregion

        public IJsonSchemaValidator Validator { get; set; }

        /// <summary>
        /// Skip validator comparison
        /// </summary>
        public bool SkipComparison { get; set; }

        public object ExplicitIgnorableValue { private get; set; }
        public int ExplicitIgnorableItemLength { private get; set; }

        public override string ToString()
        {
            return string.Format("<{0}>", Title);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonSchema;
            if (rhs == null) return false;
            return Equals(rhs);
        }

        public bool Equals(JsonSchema rhs)
        {
            // skip comparison
            if (SkipComparison) return true;
            if (rhs.SkipComparison) return true;
            return Validator.Equals(rhs.Validator);
        }

        public static bool operator ==(JsonSchema obj1, JsonSchema obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null))
            {
                return false;
            }

            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(JsonSchema obj1, JsonSchema obj2)
        {
            return !(obj1 == obj2);
        }

        #region FromType
        public static JsonSchema FromType<T>()
        {
            return FromType(typeof(T), null, null);
        }

        public static JsonSchema FromType(Type t,
            BaseJsonSchemaAttribute a = null, // field attribute
            ItemJsonSchemaAttribute ia = null
            )
        {
            // class attribute
            var aa = t.GetCustomAttributes(typeof(JsonSchemaAttribute), true)
                .FirstOrDefault() as JsonSchemaAttribute;
            if (a != null)
            {
                a.Merge(aa);
            }
            else
            {
                if (aa == null)
                {
                    a = new JsonSchemaAttribute();
                }
                else
                {
                    a = aa;
                }
            }

            if (ia == null)
            {
                ia = t.GetCustomAttributes(typeof(ItemJsonSchemaAttribute), true)
                    .FirstOrDefault() as ItemJsonSchemaAttribute;
            }

            IJsonSchemaValidator validator = null;
            bool skipComparison = a.SkipSchemaComparison;
            if (t == typeof(object))
            {
                skipComparison = true;
            }

            if (a.EnumValues != null)
            {
                try
                {
                    validator = JsonEnumValidator.Create(a.EnumValues, a.EnumSerializationType);
                }
                catch (Exception)
                {
                    throw new Exception(String.Join(", ", a.EnumValues.Select(x => x.ToString()).ToArray()));
                }
            }
            else if (t.IsEnum)
            {
                validator = JsonEnumValidator.Create(t, a.EnumSerializationType, a.EnumExcludes);
            }
            else
            {
                validator = JsonSchemaValidatorFactory.Create(t, a, ia);
            }

            var schema = new JsonSchema
            {
                Title = a.Title,
                Description = a.Description,
                Validator = validator,
                SkipComparison = skipComparison,
                ExplicitIgnorableValue = a.ExplicitIgnorableValue,
                ExplicitIgnorableItemLength = a.ExplicitIgnorableItemLength,
            };

            return schema;
        }
        #endregion

        #region FromJson
        static ValueNodeType ParseValueType(string type)
        {
            try
            {
                return (ValueNodeType)Enum.Parse(typeof(ValueNodeType), type, true);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(string.Format("unknown type: {0}", type));
            }
        }

        Stack<string> m_context = new Stack<string>();

        static Utf8String s_ref = Utf8String.From("$ref");

        public void Parse(IFileSystemAccessor fs, ListTreeNode<JsonValue> root, string Key)
        {
            m_context.Push(Key);

            var compositionType = default(CompositionType);
            var composition = new List<JsonSchema>();
            foreach (var kv in root.ObjectItems())
            {
                switch (kv.Key.GetString())
                {
                    case "$schema":
                        Schema = kv.Value.GetString();
                        break;

                    case "$ref":
                        {
                            var refFs = fs.Get(kv.Value.GetString());

                            // parse JSON
                            var json = refFs.ReadAllText();
                            var refRoot = JsonParser.Parse(json);

                            Parse(refFs, refRoot, "$ref");
                        }
                        break;

                    #region Annotation
                    case "title":
                        Title = kv.Value.GetString();
                        break;

                    case "description":
                        Description = kv.Value.GetString();
                        break;

                    case "default":
                        Default = kv.Value;
                        break;
                    #endregion

                    #region Validation
                    // http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.1
                    case "type":
                        if (Validator == null)
                        {
                            Validator = JsonSchemaValidatorFactory.Create(kv.Value.GetString());
                        }
                        break;

                    case "enum":
                        Validator = JsonEnumValidator.Create(kv.Value);
                        break;

                    case "const":
                        break;
                    #endregion

                    #region Composite
                    // http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.7
                    case "oneOf":
                        break;

                    case "not":
                        break;

                    case "anyOf": // composition
                    case "allOf": // composition
                        {
                            compositionType = (CompositionType)Enum.Parse(typeof(CompositionType), kv.Key.GetString(), true);
                            foreach (var item in kv.Value.ArrayItems())
                            {
                                if (item.ContainsKey(s_ref))
                                {
                                    var sub = JsonSchema.ParseFromPath(fs.Get(item[s_ref].GetString()));
                                    composition.Add(sub);
                                }
                                else
                                {
                                    var sub = new JsonSchema();
                                    sub.Parse(fs, item, compositionType.ToString());
                                    composition.Add(sub);
                                }
                            }
                            Composite(compositionType, composition);
                        }
                        break;
                    #endregion

                    // http://json-schema.org/latest/json-schema-validation.html#rfc.section.7
                    case "format":
                        break;

                    #region Gltf
                    case "gltf_detailedDescription":
                        break;

                    case "gltf_webgl":
                        break;

                    case "gltf_uriType":
                        break;
                    #endregion

                    default:
                        {
                            if (Validator != null)
                            {
                                if (Validator.FromJsonSchema(fs, kv.Key.GetString(), kv.Value))
                                {
                                    continue;
                                }
                            }
                            throw new NotImplementedException(string.Format("unknown key: {0}", kv.Key));
                        }
                }
            }
            m_context.Pop();

            if (Validator == null)
            {
                SkipComparison = true;
            }
        }

        void Composite(CompositionType compositionType, List<JsonSchema> composition)
        {
            switch (compositionType)
            {
                case CompositionType.AllOf:
                    if (composition.Count == 1)
                    {
                        // inheritance
                        if (Validator == null)
                        {
                            //Validator = JsonSchemaValidatorFactory.Create(composition[0].Validator.ValueNodeType);
                            Validator = composition[0].Validator;
                        }
                        else
                        {
                            Validator.Merge(composition[0].Validator);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;

                case CompositionType.AnyOf:
                    if (Validator == null)
                    {
                        if (composition.Count == 1)
                        {
                            throw new NotImplementedException();
                            //Validator = composition[0].Validator;
                        }
                        else
                        {
                            // extend enum
                            // enum, enum..., type
                            Validator = JsonEnumValidator.Create(composition, EnumSerializationType.AsString);
                        }
                    }
                    //throw new NotImplementedException();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static JsonSchema ParseFromPath(IFileSystemAccessor fs)
        {
            // parse JSON
            var json = fs.ReadAllText();
            var root = JsonParser.Parse(json);

            // create schema
            var schema = new JsonSchema();
            schema.Parse(fs, root, "__ParseFromPath__" + fs.ToString());
            return schema;
        }
        #endregion

        public void Serialize<T>(IFormatter f, T o, JsonSchemaValidationContext c = null)
        {
            if (c == null)
            {
                c = new JsonSchemaValidationContext(o)
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };
            }

            var ex = Validator.Validate(c, o);
            if (ex != null)
            {
                throw ex;
            }

            Validator.Serialize(f, c, o);
        }

        public void ToJson(IFormatter f)
        {
            f.BeginMap(2);
            if (!string.IsNullOrEmpty(Title)) { f.Key("title"); f.Value(Title); }
            if (!string.IsNullOrEmpty(Description)) { f.Key("description"); f.Value(Description); }
            Validator.ToJsonScheama(f);
            f.EndMap();
        }

        public bool IsExplicitlyIgnorableValue<T>(T obj)
        {
            if (obj == null)
            {
                return ExplicitIgnorableValue == null;
            }

            var iter = obj as System.Collections.ICollection;
            if (ExplicitIgnorableItemLength != -1 && iter != null)
            {
                return iter.Count == ExplicitIgnorableItemLength;
            }

            return obj.Equals(ExplicitIgnorableValue);
        }
    }

    public static class JsonSchemaExtensions
    {
        public static string Serialize<T>(this JsonSchema s, T o, JsonSchemaValidationContext c = null)
        {
            var f = new JsonFormatter();
            s.Serialize(f, o, c);
            return f.ToString();
        }
    }
}
