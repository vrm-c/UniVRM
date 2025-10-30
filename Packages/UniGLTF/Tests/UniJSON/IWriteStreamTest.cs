using NUnit.Framework;
using UniJSON;
using System.Linq;
using System.Text;
using System;

namespace UniJSON
{
    public class StoreTests
    {
        [Test]
        public void StringBuilderStoreTest()
        {
            var sb = new StringBuilder();
            var stream = new StringBuilderStore(sb);

            stream.Write("abc");
            Assert.AreEqual("abc", sb.ToString());

            stream.Write("d");
            Assert.AreEqual("abcd", sb.ToString());

            stream.Clear();
            stream.Write("e");
            Assert.AreEqual("e", sb.ToString());
        }

        [Test]
        public void ArrayStoreTest()
        {
            var store = new BytesStore(1);

            store.WriteValues(1, 2, 3);
            Assert.True(new Byte[] { 1, 2, 3 }.SequenceEqual(store.Bytes.ToEnumerable()));

            store.Write(4);
            Assert.True(new Byte[] { 1, 2, 3, 4 }.SequenceEqual(store.Bytes.ToEnumerable()));

            store.Clear();
            store.Write(5);
            Assert.True(new Byte[] { 5 }.SequenceEqual(store.Bytes.ToEnumerable()));
        }
    }
}
