using NUnit.Framework;
using System;
using System.Linq;

namespace UniJSON
{
    public class JsonValueTests
    {
        // TODO: Add tests for values which have other types

        [Test]
        public void NaNTest()
        {
            {
                var v = new JsonValue(Utf8String.From("NaN"), ValueNodeType.NaN, -1);
                Assert.AreEqual("NaN", v.ToString());
                Assert.AreEqual(Double.NaN, v.GetValue<double>());
            }
        }

        [Test]
        public void InfinityTest()
        {
            {
                var v = new JsonValue(Utf8String.From("Infinity"), ValueNodeType.Infinity, -1);
                Assert.AreEqual("Infinity", v.ToString());
                Assert.AreEqual(Double.PositiveInfinity, v.GetValue<double>());
            }
        }

        [Test]
        public void MinusInfinityTest()
        {
            {
                var v = new JsonValue(Utf8String.From("-Infinity"), ValueNodeType.MinusInfinity, -1);
                Assert.AreEqual("-Infinity", v.ToString());
                Assert.AreEqual(Double.NegativeInfinity, v.GetValue<double>());
            }
        }
    }
}