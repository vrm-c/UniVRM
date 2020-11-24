using NUnit.Framework;
using System;


namespace UniJSON.MsgPack
{
    public class TimeTests
    {
        [Test]
        public void TimeTest()
        {
            var f = new MsgPackFormatter();

            {
                f.GetStore().Clear();
                var time = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
                f.Value(time);

                var bytes = f.GetStoreBytes().ArrayOrCopy();
                unchecked
                {
                    Assert.AreEqual(new byte[]
                    {
                (byte)MsgPackType.FIX_EXT_4, (byte)-1, 0, 0, 0, 0
                    }, bytes);
                }
                var parsed = MsgPackParser.Parse(bytes).GetValue();
                Assert.AreEqual(time, parsed);
            }

            {
                var time = new DateTimeOffset(2018, 12, 8, 2, 12, 15, TimeSpan.Zero);
                Assert.AreEqual(1544235135, time.ToUnixTimeSeconds());
                f.GetStore().Clear();
                f.Value(time);
                var bytes = f.GetStoreBytes().ArrayOrCopy();
                var parsed = MsgPackParser.Parse(bytes).GetValue();
                Assert.AreEqual(time, parsed);
            }

            {
                f.GetStore().Clear();
                Assert.Catch(() =>
                {
                    f.Value(new DateTimeOffset());
                });
            }
        }
    }
}
