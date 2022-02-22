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
        public void Vector3Test()
        {
            var v = new Vector3(1, 2, 3);
            var r = v.ReverseZ();
            Assert.AreEqual(new Vector3(1, 2, -3), r);
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
    }
}
