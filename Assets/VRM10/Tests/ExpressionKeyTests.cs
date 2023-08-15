using NUnit.Framework;

namespace UniVRM10.Test
{
    public sealed class ExpressionKeyTests
    {
        [Test]
        public void InvalidExpressionKey()
        {
            Assert.Catch(() => ExpressionKey.CreateCustom(""));
            Assert.Catch(() => ExpressionKey.CreateCustom(null));
            Assert.Catch(() => ExpressionKey.CreateFromPreset(ExpressionPreset.custom));
        }

        [Test]
        public void ExpressionKeyEquality()
        {
            var happy = ExpressionKey.CreateFromPreset(ExpressionPreset.happy);
            Assert.AreEqual(happy, ExpressionKey.CreateFromPreset(ExpressionPreset.happy));
            Assert.AreNotEqual(happy, ExpressionKey.CreateFromPreset(ExpressionPreset.sad));
            Assert.AreNotEqual(happy, ExpressionKey.CreateFromPreset(ExpressionPreset.aa));
            Assert.AreNotEqual(happy, ExpressionKey.CreateCustom("happy"));
            Assert.AreNotEqual(happy, ExpressionKey.CreateCustom("my_custom"));

            var custom = ExpressionKey.CreateCustom("my_custom");
            Assert.AreEqual(custom, ExpressionKey.CreateCustom("my_custom"));
            Assert.AreNotEqual(custom, ExpressionKey.CreateCustom("my_custom_2"));
            Assert.AreNotEqual(custom, ExpressionKey.CreateFromPreset(ExpressionPreset.happy));
        }

        [Test]
        public void ExpressionKeyName()
        {
            var happy = ExpressionKey.CreateFromPreset(ExpressionPreset.happy);
            Assert.AreEqual("happy", happy.Name);

            var custom = ExpressionKey.CreateCustom("my_custom");
            Assert.AreEqual("my_custom", custom.Name);
        }
    }
}