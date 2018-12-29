using NUnit.Framework;
using System;
using System.Linq;

namespace UniJSON.MsgPack
{
    [TestFixture]
    public class ArrayTest
    {
        [Test]
        public void fix_array()
        {
            var f = new MsgPackFormatter();
            // Object[] not supported
            f.Serialize(new[] { 0, 1, false, (Object)null });
            var _bytes = f.GetStoreBytes();
            var bytes = _bytes.Array.Skip(_bytes.Offset).Take(_bytes.Count).ToArray();

            Assert.AreEqual(new Byte[]{
                (Byte)MsgPackType.FIX_ARRAY_0x4,
                (Byte)MsgPackType.POSITIVE_FIXNUM,
                (Byte)MsgPackType.POSITIVE_FIXNUM_0x01,
                (Byte)MsgPackType.FALSE,
                (Byte)MsgPackType.NIL
            }, bytes);

            var parsed = MsgPackParser.Parse(bytes);

            Assert.AreEqual(4, parsed.GetArrayCount());
            Assert.AreEqual(0, parsed[0].GetValue());
            Assert.AreEqual(1, parsed[1].GetValue());
            Assert.False((Boolean)parsed[2].GetValue());
            Assert.AreEqual(null, parsed[3].GetValue());
        }

        [Test]
        public void array16()
        {
            var f = new MsgPackFormatter();
            var data = Enumerable.Range(0, 20).Select(x => (Object)x).ToArray();
            f.Serialize(data);
            var bytes = f.GetStoreBytes();

            var value = MsgPackParser.Parse(bytes);
            Assert.IsTrue(value.IsArray());
            Assert.AreEqual(20, value.GetArrayCount());
            for (int i = 0; i < 20; ++i)
            {
                Assert.AreEqual(i, value[i].GetValue());
            }
        }

        [Test]
        public void array129()
        {
            {
                var i128 = Enumerable.Range(0, 128).ToArray();
                var f = new MsgPackFormatter();
                f.Serialize(i128);
                var bytes128 = f.GetStoreBytes();
                var deserialized = MsgPackParser.Parse(bytes128);
                Assert.AreEqual(128, deserialized.GetArrayCount());
                for (int i = 0; i < i128.Length; ++i)
                {
                    Assert.AreEqual(i128[i], deserialized[i].GetValue());
                }
            }

            {
                var i129 = Enumerable.Range(0, 129).ToArray();
                var f = new MsgPackFormatter();
                f.Serialize(i129);
                var bytes129 = f.GetStoreBytes();
                var deserialized = MsgPackParser.Parse(bytes129);
                Assert.AreEqual(129, deserialized.GetArrayCount());
                for (int i = 0; i < i129.Length; ++i)
                {
                    Assert.AreEqual(i129[i], deserialized[i].GetValue());
                }
            }
        }

        [Test]
        public void ReadTest()
        {
            var data = new int[] { -108, 0, 1, -90, 108, 111, 103, 103, 101, 114, -110, -91, 69, 114, 114, 111, 114, -94, 101, 50 }
            .Select(x => (Byte)x).ToArray();
            var parsed = MsgPackParser.Parse(data);
            Assert.True(parsed.IsArray());
        }
    }
}
