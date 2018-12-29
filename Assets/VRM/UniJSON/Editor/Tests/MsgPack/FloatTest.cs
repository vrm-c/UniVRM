using NUnit.Framework;
using System;
using System.Linq;


namespace UniJSON.MsgPack
{
    [TestFixture]
    public class FloatTest
    {
        [Test]
        public void Float32()
        {
            var i = 1.1f;
            var float_be = new byte[]
            {
                (Byte)MsgPackType.FLOAT, 0x3f, 0x8c, 0xcc, 0xcd
            };

            var f = new MsgPackFormatter();
            f.Value(i);
            var bytes = f.GetStoreBytes();

            var value = MsgPackParser.Parse(bytes);
            var body = value.Value.Bytes;
            Assert.AreEqual(float_be, body.ToEnumerable().ToArray());

            Assert.AreEqual(i, value.GetValue());
        }

        [Test]
        public void Float64()
        {
            var i = 1.1;
            var double_be = new Byte[]{
                (Byte)MsgPackType.DOUBLE, 0x3f, 0xf1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9a,
            };

            var f = new MsgPackFormatter();
            f.Value(i);
            var bytes = f.GetStoreBytes();

            var value = MsgPackParser.Parse(bytes);
            var body = value.Value.Bytes;
            Assert.AreEqual(double_be, body.ToEnumerable().ToArray());

            Assert.AreEqual(i, value.GetValue());
        }
    }
}
