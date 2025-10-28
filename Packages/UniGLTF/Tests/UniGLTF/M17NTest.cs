using NUnit.Framework;
using UniGLTF.M17N;
using UnityEngine;

namespace UniGLTF
{
    public class M17NTest
    {
        enum M17NTestEnum
        {
            Foo,
            Bar,
            Baz,
        }

        [Test]
        public void SimpleMsgTest()
        {
            Assert.AreEqual("Foo", M17NTestEnum.Foo.Msg());
            Assert.AreEqual("Bar", M17NTestEnum.Bar.Msg());
            Assert.AreEqual("Baz", M17NTestEnum.Baz.Msg());

            // test caching
            Assert.AreEqual("Foo", M17NTestEnum.Foo.Msg());
            Assert.AreEqual("Bar", M17NTestEnum.Bar.Msg());
            Assert.AreEqual("Baz", M17NTestEnum.Baz.Msg());
        }
    }
}
