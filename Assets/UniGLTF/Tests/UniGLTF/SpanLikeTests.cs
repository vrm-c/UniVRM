using System;
using NUnit.Framework;

namespace UniGLTF
{
    public class SpanLikeTests
    {
        [Test]
        [Category("SpanLike")]
        public void CopyTest()
        {
            var src = new int[] { 255 };
            var dst = new byte[4];

            src.ToBytes(new ArraySegment<byte>(dst));

            Assert.AreEqual(0xFF, dst[0]);
            Assert.AreEqual(0, dst[1]);
            Assert.AreEqual(0, dst[2]);
            Assert.AreEqual(0, dst[3]);
        }

        [Test]
        [Category("SpanLike")]
        public void SpanLikeTest()
        {
            var v0 = new UnityEngine.Vector3(1, 2, 3);
            var v1 = new UnityEngine.Vector3(4, 5, 6);
            var positions = new UnityEngine.Vector3[]
            {
                v0,
                v1,
            };
            var span = SpanLike.CopyFrom(positions);

            Assert.AreEqual(2, span.Length);
            Assert.AreEqual(v0, span[0]);
            Assert.AreEqual(v1, span[1]);
        }
    }
}
