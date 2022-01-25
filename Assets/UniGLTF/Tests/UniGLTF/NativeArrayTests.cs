using System;
using NUnit.Framework;
using Unity.Collections;

namespace UniGLTF
{
    public class NativeArrayTests
    {
        [Test]
        public void DisposeTest()
        {
            var array = new NativeArray<byte>(64, Allocator.Persistent);
            var sub = array.GetSubArray(10, 4);
            Assert.Throws<InvalidOperationException>(() => { sub.Dispose(); });
            var cast = array.Reinterpret<int>(1);

            // Dispose可能
            cast.Dispose();

            // Disposed
            Assert.Throws<InvalidOperationException>(() => { var c = cast[0]; });
            Assert.Throws<InvalidOperationException>(() => { var a = array[0]; });
            Assert.Throws<InvalidOperationException>(() => { var s = sub[0]; });
        }
    }
}
