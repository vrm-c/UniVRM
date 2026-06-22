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

        [Test]
        public void FromToRotationMatchesUnityForStandardCasesTest()
        {
            AssertFromToRotationMatchesUnity(new float3(1, 0, 0), new float3(0, 1, 0));
            AssertFromToRotationMatchesUnity(new float3(0, 1, 0), new float3(0, 0, 1));
            AssertFromToRotationMatchesUnity(new float3(1, 2, 3), new float3(4, 5, 6));
            AssertFromToRotationMatchesUnity(new float3(-2, 0.5f, 3), new float3(1, -4, 0.25f));
        }

        [Test]
        public void FromToRotationSameDirectionTest()
        {
            AssertFromToRotation(new float3(1, 0, 0), new float3(2, 0, 0));
            AssertFromToRotation(new float3(1, 2, 3), new float3(2, 4, 6));
        }

        [Test]
        public void FromToRotationOppositeDirectionTest()
        {
            AssertFromToRotation(new float3(1, 0, 0), new float3(-1, 0, 0));
            AssertFromToRotation(new float3(0, 1, 0), new float3(0, -1, 0));
            AssertFromToRotation(new float3(0, 0, 1), new float3(0, 0, -1));
            AssertFromToRotation(new float3(1, 2, 3), new float3(-1, -2, -3));
        }

        [Test]
        public void FromToRotationNearlyOppositeDirectionTest()
        {
            AssertFromToRotation(new float3(1, 0, 0), math.normalize(new float3(-1, 0.0001f, 0)));
            AssertFromToRotation(new float3(1, 2, 3), math.normalize(new float3(-1.0001f, -2, -3)));
        }

        [Test]
        public void FromToRotationZeroVectorTest()
        {
            Assert.That(MathHelper.Approximately(MathHelper.FromToRotation(float3.zero, new float3(0, 1, 0)), quaternion.identity), Is.True);
            Assert.That(MathHelper.Approximately(MathHelper.FromToRotation(new float3(1, 0, 0), float3.zero), quaternion.identity), Is.True);
        }

        [Test]
        public void FromToRotationTinyVectorTest()
        {
            var result = MathHelper.FromToRotation(new float3(1e-12f, 0, 0), new float3(0, 1, 0));
            Assert.That(MathHelper.Approximately(result, quaternion.identity), Is.True);
        }

        private static void AssertFromToRotation(float3 from, float3 to)
        {
            var result = MathHelper.FromToRotation(from, to);
            var rotated = math.mul(result, math.normalize(from));
            var dot = math.dot(math.normalize(rotated), math.normalize(to));
            Assert.That(dot, Is.GreaterThan(0.9999f));
        }

        private static void AssertFromToRotationMatchesUnity(float3 from, float3 to)
        {
            var result = MathHelper.FromToRotation(from, to);
            var expected = Quaternion.FromToRotation(from, to);
            Assert.That(MathHelper.Approximately(result, expected), Is.True);
        }
    }
}
