using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace UniJSON.MsgPack
{
    [TestFixture]
    public class MapTest
    {
        [Test]
        public void fix_map()
        {
            var f = new MsgPackFormatter();
            f.BeginMap(2);
            f.Key("0"); f.Value(1);
            f.Key("2"); f.Value(3);
            f.EndMap();
            var bytes =
            f.GetStoreBytes();
            ;

            Assert.AreEqual(new Byte[]{
                0x82,  // map2

                0xa1, 0x30, // "0"
                0x01, // 1

                0xa1, 0x32, // "2"
                0x03 // 3
            }, bytes.ToEnumerable());

            var value = MsgPackParser.Parse(bytes);

            Assert.AreEqual(2, value.GetObjectCount());
            Assert.AreEqual(1, value["0"].GetValue());
            Assert.AreEqual(3, value["2"].GetValue());
        }

        [Test]
        public void map16()
        {
            var w = new MsgPackFormatter();
            int size = 18;
            w.BeginMap(size);
            for (int i = 0; i < size; ++i)
            {
                w.Value(i.ToString());
                w.Value(i + 5);
            }
            var bytes = w.GetStoreBytes().ToEnumerable().ToArray();


            var expected = new Byte[]{
                    0xde, // map18
                    0x0, 0x12, // 18

                    0xa1, 0x30, // "0"
                    0x5,

                    0xa1, 0x31, // "1"
                    0x6,

                    0xa1, 0x32, // "2"
                    0x7,

                    0xa1, 0x33, // "3"
                    0x8,

                    0xa1, 0x34, // "4"
                    0x9,

                    0xa1, 0x35, // "5"
                    0xa,

                    0xa1, 0x36, // "6"
                    0xb,

                    0xa1, 0x37, // "7"
                    0xc,

                    0xa1, 0x38, // "8"
                    0xd,

                    0xa1, 0x39, // "9"
                    0xe,

                    0xa2, 0x31, 0x30, // "10"
                    0xf,

                    0xa2, 0x31, 0x31, // "11"
                    0x10,

                    0xa2, 0x31, 0x32, // "12"
                    0x11,

                    0xa2, 0x31, 0x33, // "13"
                    0x12,

                    0xa2, 0x31, 0x34, // "14"
                    0x13,

                    0xa2, 0x31, 0x35, // "15"
                    0x14,

                    0xa2, 0x31, 0x36, // "16"
                    0x15,

                    0xa2, 0x31, 0x37, // "17",
                    0x16
            };

            Assert.AreEqual(expected, bytes);

            var value = MsgPackParser.Parse(bytes);

            Assert.AreEqual(15, value["10"].GetValue());
        }
    }
}
