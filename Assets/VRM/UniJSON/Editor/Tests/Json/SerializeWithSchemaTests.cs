using NUnit.Framework;

namespace UniJSON
{
    public class SerializeWithSchemaTests
    {
        [JsonSchema(Title="CheckConstraintsTest")]
        public class CheckConstraintsTest
        {
            [JsonSchema(Minimum = 0)]
            public int X;

            [JsonSchema(Minimum = 10)] // Not required, thus ignored when the value violates the constraints
            public int Y;
        }

        [Test]
        public void TestCheckConstraints()
        {
            var obj = new CheckConstraintsTest()
            {
                X = 0,
                Y = 0, // Will be excluded because 0 doesn't satisfy a requirement of "Minimum = 10"
            };

            var s = JsonSchema.FromType<CheckConstraintsTest>();
            {
                var c = new JsonSchemaValidationContext(obj);
                Assert.Null(s.Validator.Validate(c, s));
            }
            var actual = s.Serialize(obj);

            var expected = @"{""X"":0}";

            Assert.AreEqual(expected, actual);
        }
    }
}
