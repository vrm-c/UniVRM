using NUnit.Framework;
using System;
using System.Linq;

namespace UniJSON.MsgPack
{
    [TestFixture]
    public class RawTest
    {
        [Test]
        public void fix_raw()
        {
            var src = new Byte[] { 0, 1, 2 };
            var f = new MsgPackFormatter();
            f.Value(src);
            var bytes = f.GetStoreBytes();

            var v = MsgPackParser.Parse(bytes).Value.GetBody();
            Assert.True(src.SequenceEqual(v.ToEnumerable()));
        }

        [Test]
        public void raw16()
        {
            var src = Enumerable.Range(0, 50).Select(x => (Byte)x).ToArray();
            var f = new MsgPackFormatter();
            f.Value(src);
            var bytes = f.GetStoreBytes();

            var v = MsgPackParser.Parse(bytes).Value.GetBody();
            Assert.True(src.SequenceEqual(v.ToEnumerable()));
        }
    }
}
