using System.Runtime.InteropServices;
using NUnit.Framework;

namespace UniGLTF
{
    public class UnityEngineNumerics
    {
        [Test]
        public void Values()
        {
            var values = new float[16];

            // T
            {
                var um = UnityEngine.Matrix4x4.Translate(new UnityEngine.Vector3(1, 2, 3));
                using (var pin = Pin.Create(new[] { um }))
                {
                    Marshal.Copy(pin.Ptr, values, 0, 16);
                }
                Assert.AreEqual(1, um.m03);
                Assert.AreEqual(2, um.m13);
                Assert.AreEqual(3, um.m23);
                Assert.AreEqual(new UnityEngine.Vector4(1, 2, 3, 1), um.GetColumn(3));
                Assert.AreEqual(new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 2, 3, 1 }, values);

                var v = new UnityEngine.Vector4();
                var m = new UnityEngine.Matrix4x4();
                m.MultiplyVector(v);

                // new UnityEngine.Matrix4x4.mul new UnityEngine.Vector3().mul
            }

            {
                var nm = System.Numerics.Matrix4x4.CreateTranslation(1, 2, 3);
                using (var pin = Pin.Create(new[] { nm }))
                {
                    Marshal.Copy(pin.Ptr, values, 0, 16);
                }
                Assert.AreEqual(1, nm.M41);
                Assert.AreEqual(2, nm.M42);
                Assert.AreEqual(3, nm.M43);
                Assert.AreEqual(new System.Numerics.Vector3(1, 2, 3), nm.Translation);
                Assert.AreEqual(new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 2, 3, 1 }, values);
            }
        }
    }
}
