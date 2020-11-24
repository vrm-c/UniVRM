using NUnit.Framework;
using System.Linq;


namespace UniJSON
{
    public class Utf8StringTests
    {
        [Test]
        public void Utf8StringTest()
        {
            var abc = Utf8String.From("abc");
            var ab = Utf8String.From("ab");
            var bc = Utf8String.From("bc");

            Assert.True(abc.StartsWith(ab));
            Assert.False(ab.StartsWith(abc));

            Assert.True(abc.EndsWith(bc));
            Assert.False(bc.EndsWith(abc));

            Assert.AreEqual(Utf8String.From("abbc"), ab.Concat(bc));

            Assert.AreEqual(2, abc.IndexOf((byte)'c'));

            int pos;
            abc.TrySearchAscii((byte)'c', 0, out pos);
            Assert.AreEqual(2, pos);

            abc.TrySearchAscii((byte)'c', 1, out pos);
            Assert.AreEqual(2, pos);
        }

        [Test]
        public void ShortUtf8Test()
        {
            var a0 = Utf8String4.Create("a");
            Assert.AreEqual("a", a0);
            var a1 = Utf8String4.Create(new byte[] { (byte)'a', 0x00 });
            Assert.AreEqual(a0, a1);
            var a2 = Utf8String4.Create("５");
            Assert.AreEqual(3, a2.ByteLength);
        }

        [Test]
        public void QuoteTest()
        {
            {
                var value = Utf8String.From("ho５日本語ge");
                var quoted = Utf8String.From("\"ho５日本語ge\"");
                Assert.AreEqual(quoted, JsonString.Quote(value));
                Assert.AreEqual(value, JsonString.Unquote(quoted));
            }

            {
                var value = Utf8String.From("fuga\n  ho５日本語ge");
                var quoted = Utf8String.From("\"fuga\\n  ho５日本語ge\"");
                Assert.AreEqual(quoted, JsonString.Quote(value));
                Assert.AreEqual(value, JsonString.Unquote(quoted));
            }
        }

        [Test]
        public void SplitTest()
        {
            {
                var value = Utf8String.From("a/５/c");
                var split = value.Split((byte)'/').ToArray();
                Assert.AreEqual(3, split.Length);
                Assert.AreEqual(split[0], Utf8String.From("a"));
                Assert.AreEqual(split[1], Utf8String.From("５"));
                Assert.AreEqual(split[2], Utf8String.From("c"));
            }
            {
                var value = Utf8String.From("/a/５/c/");
                var split = value.Split((byte)'/').ToArray();
                Assert.AreEqual(4, split.Length);
                Assert.AreEqual(split[0], Utf8String.From(""));
                Assert.AreEqual(split[1], Utf8String.From("a"));
                Assert.AreEqual(split[2], Utf8String.From("５"));
                Assert.AreEqual(split[3], Utf8String.From("c"));
            }
        }

        [Test]
        public void SplitIntegerTest()
        {
            Assert.AreEqual("1", Utf8String.From("1 ").SplitInteger().ToString());
            Assert.AreEqual("123", Utf8String.From("123").SplitInteger().ToString());
            Assert.Catch(() => Utf8String.From(" 1").SplitInteger());
            Assert.AreEqual("+12", Utf8String.From("+12\n").SplitInteger().ToString());
            Assert.AreEqual("-123", Utf8String.From("-123\n").SplitInteger().ToString());
        }

        [Test]
        public void AtoiTest()
        {
            Assert.AreEqual(1234, Utf8String.From("1234").ToInt32());
        }

        [Test]
        public void ToCharTest()
        {
            {
                // 1byte
                var c = 'A';
                Assert.AreEqual(1, Utf8String.From(c.ToString()).GetFirst().CurrentByteLength);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Unicode);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Char);
            }
            {
                // 2byte
                var c = '¢';
                Assert.AreEqual(2, Utf8String.From(c.ToString()).GetFirst().CurrentByteLength);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Unicode);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Char);
            }
            {
                // 3byte
                var c = '５';
                Assert.AreEqual(3, Utf8String.From(c.ToString()).GetFirst().CurrentByteLength);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Unicode);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Char);
            }
            {
                var c = '仡';
                Assert.AreEqual(3, Utf8String.From(c.ToString()).GetFirst().CurrentByteLength);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Unicode);
                Assert.AreEqual(c, Utf8String.From(c.ToString()).GetFirst().Char);
            }
            {
                // emoji
                var s = "😃";
                Assert.AreEqual(4, Utf8String.From(s).GetFirst().CurrentByteLength);
                Assert.AreEqual(0x1F603, Utf8String.From(s).GetFirst().Unicode);
                Assert.Catch(() =>
                {
                    var a = Utf8String.From(s).GetFirst().Char;
                });
            }
        }

        [Test]
        public void FromStringTest()
        {
            var buffer = new byte[12];

            {
                var src = "abc";
                var utf8 = Utf8String.From(src, buffer);
                Assert.AreEqual(3, utf8.ByteLength);
                Assert.AreEqual(src, utf8.ToString());
            }
            {
                var src = "¢";
                var utf8 = Utf8String.From(src, buffer);
                Assert.AreEqual(2, utf8.ByteLength);
                Assert.AreEqual(src, utf8.ToString());
            }
            {
                var src = "５";
                var utf8 = Utf8String.From(src, buffer);
                Assert.AreEqual(3, utf8.ByteLength);
                Assert.AreEqual(src, utf8.ToString());
            }
        }
    }
}
