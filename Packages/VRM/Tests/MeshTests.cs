using NUnit.Framework;
using UnityEngine;
using UniGLTF.MeshUtility;
using System.Linq;

namespace VRM
{
    public class MeshTests
    {
        public static void MeshEquals(Mesh l, Mesh r)
        {
#if UNITY_2017_3_OR_NEWER
            Assert.AreEqual(l.indexFormat, r.indexFormat);
#endif
            Assert.True(l.vertices.SequenceEqual(r.vertices));
            Assert.True(l.normals.SequenceEqual(r.normals));
            Assert.True(l.tangents.SequenceEqual(r.tangents));
            Assert.True(l.uv.SequenceEqual(r.uv));
            Assert.True(l.uv2.SequenceEqual(r.uv2));
            Assert.True(l.uv3.SequenceEqual(r.uv3));
            Assert.True(l.uv4.SequenceEqual(r.uv4));
            Assert.True(l.colors.SequenceEqual(r.colors));
            Assert.True(l.boneWeights.SequenceEqual(r.boneWeights));
            Assert.True(l.bindposes.SequenceEqual(r.bindposes));

            Assert.AreEqual(l.subMeshCount, r.subMeshCount);
            for (int i = 0; i < l.subMeshCount; ++i)
            {
                Assert.True(l.GetIndices(i).SequenceEqual(r.GetIndices(i)));
            }

            Assert.AreEqual(l.blendShapeCount, r.blendShapeCount);
            for (int i = 0; i < l.blendShapeCount; ++i)
            {
                Assert.AreEqual(l.GetBlendShapeName(i), r.GetBlendShapeName(i));
                Assert.AreEqual(l.GetBlendShapeFrameCount(i), r.GetBlendShapeFrameCount(i));
                Assert.AreEqual(l.GetBlendShapeFrameWeight(i, 0), r.GetBlendShapeFrameWeight(i, 0));

                var lv = l.vertices;
                var ln = l.vertices;
                var lt = l.vertices;
                var rv = r.vertices;
                var rn = r.vertices;
                var rt = r.vertices;
                l.GetBlendShapeFrameVertices(i, 0, lv, ln, lt);
                r.GetBlendShapeFrameVertices(i, 0, rv, rn, rt);
                Assert.True(lv.SequenceEqual(rv));
                Assert.True(ln.SequenceEqual(rn));
                Assert.True(lt.SequenceEqual(rt));
            }
        }

        [Test]
        public void MeshCopyTest()
        {
            var src = new Mesh();
            src.AddBlendShapeFrame("blendShape", 100.0f, null, null, null);

            var dst = src.Copy(true);

            MeshEquals(src, dst);
        }
    }
}
