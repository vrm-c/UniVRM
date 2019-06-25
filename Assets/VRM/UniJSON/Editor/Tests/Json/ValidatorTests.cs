using NUnit.Framework;
using System.Collections.Generic;

namespace UniJSON
{
    public class ValidatorTests
    {
        [Test]
        public void IntValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonIntValidator();
                v.Maximum = 0;
                Assert.NotNull(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.Maximum = 0;
                v.ExclusiveMaximum = true;
                Assert.NotNull(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.Minimum = 0;
                Assert.Null(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0));
                Assert.NotNull(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.Minimum = 0;
                v.ExclusiveMinimum = true;
                Assert.Null(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0));
                Assert.NotNull(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.MultipleOf = 4;
                Assert.Null(v.Validate(c, 4));
                Assert.NotNull(v.Validate(c, 5));
            }

            Assert.True(c.IsEmpty());
        }

        [Test]
        public void NumberValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonNumberValidator();
                v.Maximum = 0.1;
                Assert.NotNull(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0.1));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonNumberValidator();
                v.Maximum = 0.1;
                v.ExclusiveMaximum = true;
                Assert.NotNull(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0.1));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonNumberValidator();
                v.Minimum = 0.1;
                Assert.Null(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0.1));
                Assert.NotNull(v.Validate(c, -1));
            }
            {
                var v = new JsonNumberValidator();
                v.Minimum = 0.1;
                v.ExclusiveMinimum = true;
                Assert.Null(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0.1));
                Assert.NotNull(v.Validate(c, -1));
            }

            Assert.True(c.IsEmpty());
        }

        [Test]
        public void BoolValidator()
        {
            // ???
        }

        [Test]
        public void StringValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonStringValidator();
                Assert.Null(v.Validate(c, ""));
                Assert.Null(v.Validate(c, "a"));
            }

            {
                var v = new JsonStringValidator();
                v.MinLength = 1;
                Assert.Null(v.Validate(c, "a"));
                Assert.NotNull(v.Validate(c, ""));
            }
            {
                var v = new JsonStringValidator();
                v.MaxLength = 1;
                Assert.Null(v.Validate(c, "a"));
                Assert.NotNull(v.Validate(c, "ab"));
            }
            {
                var v = new JsonStringValidator();
                v.Pattern = new System.Text.RegularExpressions.Regex("abc");
                Assert.Null(v.Validate(c, "abc"));
                Assert.NotNull(v.Validate(c, "ab"));
            }
            {
                var v = new JsonStringValidator();
                v.Pattern = new System.Text.RegularExpressions.Regex("ab+");
                Assert.Null(v.Validate(c, "abb"));
                Assert.Null(v.Validate(c, "ab"));
                Assert.NotNull(v.Validate(c, "a"));
            }

            Assert.True(c.IsEmpty());
        }

        [Test]
        public void StringEnumValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = JsonStringEnumValidator.Create(new string[] { "a", "b" }, EnumSerializationType.AsString);
                Assert.Null(v.Validate(c, "a"));
                Assert.NotNull(v.Validate(c, "c"));
            }

            Assert.True(c.IsEmpty());
        }

        [Test]
        public void IntEnumValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonIntEnumValidator();
                v.Values = new int[] { 1, 2 };
                Assert.Null(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 3));
            }

            Assert.True(c.IsEmpty());
        }

        [Test]
        public void ArrayValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonArrayValidator();
                v.MaxItems = 1;
                Assert.Null(v.Validate(c, new object[] { 0 }));
                Assert.NotNull(v.Validate(c, new object[] { 0, 1 }));
            }

            {
                var v = new JsonArrayValidator();
                v.MinItems = 1;
                Assert.Null(v.Validate(c, new object[] { 0 }));
                Assert.NotNull(v.Validate(c, new object[] { }));
            }

            Assert.True(c.IsEmpty());
        }

        class Hoge
        {
            [JsonSchema(Required = true, Minimum = 1)]
            public int Value;
        }

        [Test]
        public void ObjectValidator()
        {
            var c = new JsonSchemaValidationContext("test");
            {
                var s = JsonSchema.FromType<Hoge>();
                Assert.Null(s.Validator.Validate(c, new Hoge { Value = 1 }));
                Assert.NotNull(s.Validator.Validate(c, new Hoge { Value = 0 }));
            }

            Assert.True(c.IsEmpty());
        }

        class NotRequired
        {
            [JsonSchema(Minimum = 1)]
            public int Value;
        }

        [Test]
        public void ObjectValidatorForNotRequired()
        {
            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = false, // Default behaviour
                };

                var s = JsonSchema.FromType<NotRequired>();
                // An error is not returned because Value is not 'Required' and the diagnosis is not enabled
                Assert.Null(s.Validator.Validate(c, new NotRequired { Value = 0 }));

                Assert.True(c.IsEmpty());
            }

            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<NotRequired>();
                Assert.NotNull(s.Validator.Validate(c, new NotRequired { Value = 0 }));

                Assert.True(c.IsEmpty());
            }
        }

        class NotRequiredWithIgnorable
        {
            [JsonSchema(Minimum = 2, ExplicitIgnorableValue = -1)]
            public int Value;
        }

        [Test]
        public void ObjectValidatorForNotRequiredWithIgnorable()
        {
            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = false, // Default behaviour
                };

                var s = JsonSchema.FromType<NotRequiredWithIgnorable>();
                // An error is not returned because Value is not 'Required' and the diagnosis is not enabled
                Assert.Null(s.Validator.Validate(c, new NotRequiredWithIgnorable { Value = 0 }));

                Assert.True(c.IsEmpty());
            }

            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<NotRequiredWithIgnorable>();
                Assert.NotNull(s.Validator.Validate(c, new NotRequiredWithIgnorable { Value = 0 }));

                Assert.True(c.IsEmpty());
            }

            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<NotRequiredWithIgnorable>();
                // An error is NOT returned even though diagnosis is enabled because of an ignorable value is matched
                Assert.Null(s.Validator.Validate(c, new NotRequiredWithIgnorable { Value = -1 }));

                Assert.True(c.IsEmpty());
            }
        }

        [Test]
        public void DictionaryValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var s = JsonSchema.FromType<Dictionary<string, int>>();
                Assert.True(s.Validator is JsonDictionaryValidator<int>);

                var v = s.Validator as JsonDictionaryValidator<int>;
                v.MinProperties = 1;
                v.AdditionalProperties = JsonSchema.FromType<int>();
                (v.AdditionalProperties.Validator as JsonIntValidator).Minimum = 0;

                Assert.Null(s.Validator.Validate(c, new Dictionary<string, int>
                {
                    {"POSITION", 0}
                }));

                var result = s.Validator.Validate(c, new Dictionary<string, int>
                {
                    {"POSITION", -1}
                });
                Assert.NotNull(result);
            }

            Assert.True(c.IsEmpty());
        }

        class HasDictionary
        {
            public Dictionary<string, float> primitiveProperties = new Dictionary<string, float>();
            // TODO: fix
            // public Dictionary<string, Nested> nestedProperties = new Dictionary<string, Nested>();
        }

        [Test]
        public void HasDictionaryObjectValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var s = JsonSchema.FromType<HasDictionary>();
                Assert.Null(s.Validator.Validate(c, new HasDictionary()));
            }

            Assert.True(c.IsEmpty());
        }

        class HasArrayObject
        {
            [ItemJsonSchema(Minimum = 0.0, Maximum = 1.0)]
            public float[] xs;
        }

        [Test]
        public void HasArrayObjectValidator()
        {
            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<HasArrayObject>();

                Assert.Null(s.Validator.Validate(c, new HasArrayObject { xs = new float[] { } }));
                Assert.Null(s.Validator.Validate(c, new HasArrayObject { xs = new float[] { 0.5f } }));
                Assert.NotNull(s.Validator.Validate(c, new HasArrayObject { xs = new float[] { 1.5f } }));

                Assert.True(c.IsEmpty());
            }
        }

        class HasListObject
        {
            [ItemJsonSchema(Minimum = 0.0, Maximum = 1.0)]
            public List<float> xs;
        }

        [Test]
        public void HasListObjectValidator()
        {
            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<HasListObject>();

                Assert.Null(s.Validator.Validate(c, new HasListObject { xs = new List<float> { } }));
                Assert.Null(s.Validator.Validate(c, new HasListObject { xs = new List<float> { 0.5f } }));
                Assert.NotNull(s.Validator.Validate(c, new HasListObject { xs = new List<float> { 1.5f } }));

                Assert.True(c.IsEmpty());
            }
        }

        class HasRequiredListObject
        {
            [JsonSchema(Required = true, MinItems = 1)]
            [ItemJsonSchema(Minimum = 0)]
            public int[] xs;
        }

        [Test]
        public void HasRequiredListObjectValidator()
        {
            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<HasRequiredListObject>();

                Assert.NotNull(s.Validator.Validate(c, new HasRequiredListObject()));
                Assert.NotNull(s.Validator.Validate(c, new HasRequiredListObject { xs = new int[] {} }));
                Assert.NotNull(s.Validator.Validate(c, new HasRequiredListObject { xs = new int[] { -1 } }));
                Assert.Null(s.Validator.Validate(c, new HasRequiredListObject { xs = new int[] { 0 } }));

                Assert.True(c.IsEmpty());
            }
        }

        class HasRequiredStringObject
        {
            [JsonSchema(Required = true)]
            public string s;
        }

        [Test]
        public void HasRequiredStringObjectValidator()
        {
            {
                var c = new JsonSchemaValidationContext("test")
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };

                var s = JsonSchema.FromType<HasRequiredStringObject>();

                Assert.NotNull(s.Validator.Validate(c, new HasRequiredStringObject()));
                Assert.Null(s.Validator.Validate(c, new HasRequiredStringObject { s = "" }));

                Assert.True(c.IsEmpty());
            }
        }
    }
}
