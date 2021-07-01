using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class MatrixTests
    {
        static void AssertFloat(float expected, float value, string template, float epsilon = 1e-5f)
        {
            Assert.True(Mathf.Abs(value - expected) < epsilon, string.Format(template, expected, value));
        }

        static void AssertQuaternion(in Quaternion expected, in Quaternion value, string message)
        {
            AssertFloat(expected.x, value.x, $"{message}" + ".X: {0} != {1}");
            AssertFloat(expected.y, value.y, $"{message}" + ".Y: {0} != {1}");
            AssertFloat(expected.z, value.z, $"{message}" + ".Z: {0} != {1}");
            AssertFloat(expected.w, value.w, $"{message}" + ".W: {0} != {1}");
        }

        static void AssertVec3(in Vector3 expected, in Vector3 value, string message)
        {
            AssertFloat(expected.x, value.x, $"{message}" + ".X: {0} != {1}");
            AssertFloat(expected.y, value.y, $"{message}" + ".Y: {0} != {1}");
            AssertFloat(expected.z, value.z, $"{message}" + ".Z: {0} != {1}");
        }

        static void AssertExtract(in Vector3 t, in Quaternion r, in Vector3 s)
        {
            var m = Matrix4x4.TRS(t, r, s);
            var (et, er, es) = m.Extract();
            AssertVec3(t, et, "T");
            AssertQuaternion(r, er, "R");
            AssertVec3(s, es, "S");
        }

        [Test]
        public void ExtractTest()
        {
            AssertExtract(new Vector3(1, 2, 3), Quaternion.Euler(0, 90, -90), new Vector3(1, 2, 3));
        }

        [Test]
        public void ExtractMirror_MinusX()
        {
            AssertExtract(new Vector3(1, 2, 3), Quaternion.Euler(0, 90, -90), new Vector3(-1, 2, 3));
        }

#if false
        // TODO:
        [Test]
        public void ExtractMirror_MinusY()
        {

            AssertExtract(new Vector3(1, 2, 3), Quaternion.Euler(0, 90, -90), new Vector3(1, -2, 3));
        }

        [Test]
        public void ExtractMirror_MinusZ()
        {
            AssertExtract(new Vector3(1, 2, 3), Quaternion.Euler(0, 90, -90), new Vector3(1, 2, -3));
        }

        [Test]
        public void ExtractMirror_MinusXYZ()
        {
            AssertExtract(new Vector3(1, 2, 3), Quaternion.Euler(0, 90, -90), new Vector3(-1, -2, -3));
        }
#endif
    }
}
