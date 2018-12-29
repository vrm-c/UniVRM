using NUnit.Framework;
using System;


namespace UniJSON.MsgPack
{
    [TestFixture]
    public class BooleanTest
    {
        [Test]
        public void nil()
        {
            {
                var f = new MsgPackFormatter();
                f.Null();
                var bytes = f.GetStoreBytes();
                Assert.AreEqual(new Byte[] { 0xC0 }, bytes.ToEnumerable());

                var parsed = MsgPackParser.Parse(bytes);
                Assert.True(parsed.IsNull());
            }
        }

        [Test]
        public void True()
        {
            var f = new MsgPackFormatter();
            f.Value(true);
            var bytes = f.GetStoreBytes();
            Assert.AreEqual(new Byte[] { 0xC3 }, bytes.ToEnumerable());

            var value = MsgPackParser.Parse(bytes);
            var j = value.GetBoolean();
            Assert.AreEqual(true, j);
        }

        [Test]
        public void False()
        {
            var f = new MsgPackFormatter();
            f.Value(false);
            var bytes = f.GetStoreBytes();
            Assert.AreEqual(new Byte[] { 0xC2 }, bytes.ToEnumerable());

            var value = MsgPackParser.Parse(bytes);
            var j = value.GetBoolean();
            Assert.AreEqual(false, j);
        }
    }
}
