#pragma warning disable 0649
using NUnit.Framework;


namespace UniJSON
{
    public class SchemaTests
    {
        /// <summary>
        /// http://json-schema.org/examples.html
        /// </summary>
        [JsonSchema(Title="Person")]
        public class Person
        {
            [JsonSchema(Required = true)]
            public string firstName;

            [JsonSchema(Required = true)]
            public string lastName;

            [JsonSchema(Description = "Age in years", Minimum = 0)]
            public int age;
        }

        [Test]
        public void CreateFromClass()
        {
            var s = JsonSchema.FromType<Person>();
            Assert.AreEqual("Person", s.Title);

            var v = s.Validator as JsonObjectValidator;
            Assert.AreEqual("Age in years", v.Properties["age"].Description);
            Assert.AreEqual(new[] { "firstName", "lastName" }, v.Required);

            var f = new JsonFormatter(2);
            s.ToJson(f);
            var json = f.ToString();

            var parsed = JsonParser.Parse(json);
            Assert.AreEqual(0, parsed["properties"]["age"]["minimum"].GetInt32());
        }

        public enum ProjectionType
        {
            Perspective,
            Orthographic
        }

        class EnumStringTest
        {
            [JsonSchema(EnumSerializationType =EnumSerializationType.AsLowerString)]
            public ProjectionType type;
        }

        class EnumIntTest
        {
            [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt)]
            public ProjectionType type;
        }

        [Test]
        public void TestEnumAsString()
        {
            var json = @"
{
    ""type"": ""object"",
    ""properties"": {

        ""type"": {

            ""anyOf"": [
            {
                ""enum"": [ ""perspective"" ]
            },
            {
                ""enum"": [ ""orthographic"" ]
            },
            {
                ""type"": ""string""
            }
            ]

        }
        
    }
}
";

            var fromJson = new JsonSchema();
            fromJson.Parse(null, JsonParser.Parse(json), "enum test");

            var fromType = JsonSchema.FromType<EnumStringTest>();

            Assert.AreEqual(fromJson, fromType);
        }

        [Test]
        public void TestEnumAsInt()
        {
            var json = @"
{
    ""type"": ""object"",
    ""properties"": {

        ""type"": {

            ""anyOf"": [
            {
                ""enum"": [ 0 ]
            },
            {
                ""enum"": [ 1 ]
            },
            {
                ""type"": ""integer""
            }
            ]

        }
        
    }
}
";

            var fromJson = new JsonSchema();
            fromJson.Parse(null, JsonParser.Parse(json), "enum test");

            var fromType = JsonSchema.FromType<EnumIntTest>();

            Assert.AreEqual(fromJson, fromType);
        }

    }
}
