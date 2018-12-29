using System;


namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#numeric
    /// </summary>
    public class JsonIntValidator : IJsonSchemaValidator
    {
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.1
        /// </summary>
        public int? MultipleOf
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.2
        /// </summary>
        public int? Maximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.3
        /// </summary>
        public bool ExclusiveMaximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.4
        /// </summary>
        public int? Minimum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.5
        /// </summary>
        public bool ExclusiveMinimum
        {
            get; set;
        }

        public override int GetHashCode()
        {
            return 2;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonIntValidator;
            if (rhs == null) return false;

            if (MultipleOf != rhs.MultipleOf)
            {
                Console.WriteLine("MultipleOf");
                return false;
            }
            if (Maximum != rhs.Maximum)
            {
                Console.WriteLine("Maximum");
                return false;
            }

            if (ExclusiveMaximum != rhs.ExclusiveMaximum)
            {
                Console.WriteLine("ExclusiveMaximum");
                return false;
            }

            if (Minimum != rhs.Minimum)
            {
                Console.WriteLine("Minimum");
                return false;
            }

            if (ExclusiveMinimum != rhs.ExclusiveMinimum)
            {
                Console.WriteLine("ExclusiveMinimum");
                return false;
            }

            return true;
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value)
        {
            switch (key)
            {
                case "multipleOf":
                    MultipleOf = value.GetInt32();
                    return true;

                case "maximum":
                    Maximum = value.GetInt32();
                    return true;

                case "exclusiveMaximum":
                    ExclusiveMaximum = value.GetBoolean();
                    return true;

                case "minimum":
                    Minimum = value.GetInt32();
                    return true;

                case "exclusiveMinimum":
                    ExclusiveMinimum = value.GetBoolean();
                    return true;
            }

            return false;
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("integer");
            if (Minimum.HasValue)
            {
                f.Key("minimum"); f.Value(Minimum.Value);
            }
            if (Maximum.HasValue)
            {
                f.Key("maximum"); f.Value(Maximum.Value);
            }
        }

        public void Merge(IJsonSchemaValidator obj)
        {
            var rhs = obj as JsonIntValidator;
            if (rhs == null)
            {
                throw new ArgumentException();
            }

            MultipleOf = rhs.MultipleOf;
            Maximum = rhs.Maximum;
            ExclusiveMaximum = rhs.ExclusiveMaximum;
            Minimum = rhs.Minimum;
            ExclusiveMinimum = rhs.ExclusiveMinimum;
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            try
            {
                var value = GenericCast<T, int>.Cast(o);

                if (Minimum.HasValue)
                {
                    if (ExclusiveMinimum)
                    {
                        if (value > Minimum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("minimum: ! {0}>{1}", value, Minimum.Value));
                        }
                    }
                    else
                    {
                        if (value >= Minimum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("minimum: ! {0}>={1}", value, Minimum.Value));
                        }
                    }
                }

                if (Maximum.HasValue)
                {
                    if (ExclusiveMaximum)
                    {
                        if (value < Maximum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("maximum: ! {0}<{1}", value, Maximum.Value));
                        }
                    }
                    else
                    {
                        if (value <= Maximum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("maximum: ! {0}<={1}", value, Maximum.Value));
                        }
                    }
                }

                if (MultipleOf.HasValue && value % MultipleOf.Value != 0)
                {
                    return new JsonSchemaValidationException(c, string.Format("multipleOf: {0}%{1}", value, MultipleOf.Value));
                }

                return null;
            }
            catch (Exception ex)
            {
                return new JsonSchemaValidationException(c, ex);
            }
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T o)
        {
            f.Serialize(GenericCast<T, int>.Cast(o));
        }

        public void Deserialize<T, U>(ListTreeNode<T> src, ref U dst) 
            where T : IListTreeItem, IValue<T>
        {
            dst = GenericCast<int, U>.Cast(src.GetInt32());
        }
    }

    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#numeric
    /// </summary>
    public class JsonNumberValidator : IJsonSchemaValidator
    {
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.1
        /// </summary>
        public double? MultipleOf
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.2
        /// </summary>
        public double? Maximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.3
        /// </summary>
        public bool ExclusiveMaximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.4
        /// </summary>
        public double? Minimum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.5
        /// </summary>
        public bool ExclusiveMinimum
        {
            get; set;
        }

        public override int GetHashCode()
        {
            return 3;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonNumberValidator;
            if (rhs == null) return false;

            if (MultipleOf != rhs.MultipleOf) return false;
            if (Maximum != rhs.Maximum) return false;
            if (ExclusiveMaximum != rhs.ExclusiveMaximum) return false;
            if (Minimum != rhs.Minimum) return false;
            if (ExclusiveMinimum != rhs.ExclusiveMinimum) return false;

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
                case "multipleOf":
                    MultipleOf = value.GetDouble();
                    return true;

                case "maximum":
                    Maximum = value.GetDouble();
                    return true;

                case "exclusiveMaximum":
                    ExclusiveMaximum = value.GetBoolean();
                    return true;

                case "minimum":
                    Minimum = value.GetDouble();
                    return true;

                case "exclusiveMinimum":
                    ExclusiveMinimum = value.GetBoolean();
                    return true;
            }

            return false;
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("number");
            if (Minimum.HasValue)
            {
                f.Key("minimum"); f.Value(Minimum.Value);
            }
            if (Maximum.HasValue)
            {
                f.Key("maximum"); f.Value(Maximum.Value);
            }
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            try
            {
                var value = Convert.ToDouble(o);

                if (Minimum.HasValue)
                {
                    if (ExclusiveMinimum)
                    {
                        if (value > Minimum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("minimum: ! {0}>{1}", value, Minimum.Value));
                        }
                    }
                    else
                    {
                        if (value >= Minimum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("minimum: ! {0}>={1}", value, Minimum.Value));
                        }
                    }
                }

                if (Maximum.HasValue)
                {
                    if (ExclusiveMaximum)
                    {
                        if (value < Maximum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("maximum: ! {0}<{1}", value, Maximum.Value));
                        }
                    }
                    else
                    {
                        if (value <= Maximum.Value)
                        {
                            // ok
                        }
                        else
                        {
                            return new JsonSchemaValidationException(c, string.Format("maximum: ! {0}<={1}", value, Maximum.Value));
                        }
                    }
                }

                /*
                if (MultipleOf.HasValue && value % MultipleOf.Value != 0)
                {
                    return new JsonSchemaValidationException(c, string.Format("multipleOf: {0}%{1}", value, MultipleOf.Value));
                }
                */
                if (MultipleOf.HasValue)
                {
                    throw new NotImplementedException();
                }

                return null;
            }
            catch (Exception ex)
            {
                return new JsonSchemaValidationException(c, ex);
            }
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T o)
        {
            f.Serialize(o);
        }

        public void Deserialize<T, U>(ListTreeNode<T> src, ref U dst) 
            where T : IListTreeItem, IValue<T>
        {
            dst = GenericCast<double, U>.Cast(src.GetDouble());
        }
    }
}
