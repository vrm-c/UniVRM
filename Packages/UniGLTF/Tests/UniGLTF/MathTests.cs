using NUnit.Framework;
using UniGLTF.Runtime.Utils;
using Unity.Mathematics;
using UnityEngine;
namespace UniGLTF
{
    public class MathTests
    {
        private readonly float4x4 _matrix = new float4x4(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);
        private readonly float3 _vector1 = new float3(1, 2, 3);
        private readonly float3 _vector2 = new float3(4, 5, 6);

        [Test]
        public void MultiplyPoint3x4Test()
        {
            var result = MathHelper.MultiplyPoint3x4(_matrix, _vector1);
            var expected = ((Matrix4x4)_matrix).MultiplyPoint3x4(_vector1);
            Assert.That(MathHelper.Approximately(result, expected), Is.True);
        }

        [Test]
        public void MultiplyPointTest()
        {
            var result = MathHelper.MultiplyPoint(_matrix, _vector1);
            var expected = ((Matrix4x4)_matrix).MultiplyPoint(_vector1);
            Assert.That(MathHelper.Approximately(result, expected), Is.True);
        }

        [Test]
        public void MultiplyVectorTest()
        {
            var result = MathHelper.MultiplyVector(_matrix, _vector1);
            var expected = ((Matrix4x4)_matrix).MultiplyVector(_vector1);
            Assert.That(MathHelper.Approximately(result, expected), Is.True);
        }

        [Test]
        public void FromToRotationTest()
        {
            var result = MathHelper.FromToRotation(_vector1, _vector2);
            var expected = Quaternion.FromToRotation(_vector1, _vector2);
            Assert.That(MathHelper.Approximately(result, expected), Is.True);
        }
    }
}
