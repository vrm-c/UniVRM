using System;
using NUnit.Framework;
using Unity.Collections;

namespace UniGLTF
{
    public class NativeArrayTests
    {
#if UNITY_2020_3_OR_NEWER
        [Test]
        public void DisposeTest()
        {
            var array = new NativeArray<byte>(64, Allocator.Persistent);
            var sub = array.GetSubArray(10, 4);

            // SubArray の Dispose が可能になった ! (Unity-2020.3)
            // Assert.Throws<InvalidOperationException>(() => { sub.Dispose(); });
            var cast = array.Reinterpret<int>(1);

            // Dispose可能
            cast.Dispose();

            // ObjectDisposedException に変わった ! (Unity-2020.3)
            Assert.Throws<ObjectDisposedException>(() => { var c = cast[0]; });
            Assert.Throws<ObjectDisposedException>(() => { var a = array[0]; });
            Assert.Throws<ObjectDisposedException>(() => { var s = sub[0]; });
        }
#else
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
#endif
    }
}
