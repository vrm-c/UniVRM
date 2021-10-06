using System;
using System.Runtime.InteropServices;
using System.Numerics;
using NUnit.Framework;

namespace UniGLTF
{
    public class NumericTests
    {
        [Test]
        [Category("Numerics")]
        public void QuaternionTest()
        {
            var orgAxis = Vector3.Normalize(new Vector3(1, 2, 3));
            var orgAngle = 90 * NumericsExtensions.TO_RAD;
            var q = Quaternion.CreateFromAxisAngle(orgAxis, orgAngle);
            var (axis, angle) = q.GetAxisAngle();
            Assert.AreEqual(orgAxis, axis);
            Assert.AreEqual(orgAngle, angle);
        }

        [Test]
        [Category("Numerics")]
        public void Vector4Test()
        {
            var bytes = new byte[4 * 4];
            {
                var span = SpanLike.Wrap<Single>(new ArraySegment<byte>(bytes));
                span[0] = 1.0f;
            }

            {
                var span = SpanLike.Wrap<Vector4>(new ArraySegment<byte>(bytes));
                Assert.AreEqual(1.0f, span[0].X);
            }

            {
                var span = SpanLike.Wrap<Quaternion>(new ArraySegment<byte>(bytes));
                Assert.AreEqual(1.0f, span[0].X);
            }
        }

        [Test]
        [Category("Numerics")]
        public void Vector3Test()
        {
            var v = new Vector3(1, 2, 3);
            var r = v.ReverseZ();
            Assert.AreEqual(new Vector3(1, 2, -3), r);
        }

        [Test]
        [Category("Numerics")]
        public void UVTest()
        {
            var v = new Vector2(0.1f, 0.2f);
            var r = v.UVVerticalFlip();
            Assert.AreEqual(new Vector2(0.1f, 0.8f), r);
        }

        [Test]
        [Category("Numerics")]
        public void MatrixTranslationTest()
        {
            var t = new Vector3(1, 2, 3);
            var m = Matrix4x4.CreateTranslation(t);
            var ex = m.ExtractPosition();
            Assert.AreEqual(t, ex);
        }

        [Test]
        [Category("Numerics")]
        public void MatrixRotationTest()
        {
            var orgAxis = Vector3.Normalize(new Vector3(1, 2, 3));
            var orgAngle = 90 * NumericsExtensions.TO_RAD;
            var q = Quaternion.CreateFromAxisAngle(orgAxis, orgAngle);
            var m = Matrix4x4.CreateFromQuaternion(q);
            var ex = m.ExtractRotation();
            var (axis, angle) = ex.GetAxisAngle();
            Assert.True(orgAxis.NearlyEqual(axis));
            Assert.True(orgAngle.NearlyEqual(angle));
            Assert.True(q.NearlyEqual(ex));
        }

        [Test]
        [Category("Numerics")]
        public void MatrixScaleTest()
        {
            var s = new Vector3(2, 3, 4);
            var m = Matrix4x4.CreateScale(s);
            var ex = m.ExtractScale();
            Assert.AreEqual(s, ex);
        }

        [Test]
        [Category("Numerics")]
        public void MatrixTest()
        {
            var t = new Vector3(1, 2, 3);
            var orgAxis = Vector3.Normalize(new Vector3(1, 2, 3));
            var orgAngle = 90 * NumericsExtensions.TO_RAD;
            var r = Quaternion.CreateFromAxisAngle(orgAxis, orgAngle);
            var s = new Vector3(2, 3, 4);
            var m = NumericsExtensions.FromTRS(t, r, s);
            var (tt, rr, ss) = m.Decompose();
            Assert.True(s.NearlyEqual(ss));
            Assert.True(r.NearlyEqual(rr));
            Assert.AreEqual(t, tt);
        }
    }
}
