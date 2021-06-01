using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniGLTF.MeshUtility;
using UnityEngine;

namespace VRM
{
    public class NormalizeTests
    {
        class BoneMap
        {
            public List<Transform> SrcBones = new List<Transform>();
            public List<Transform> DstBones = new List<Transform>();
            public Dictionary<Transform, Transform> Map = new Dictionary<Transform, Transform>();

            public void Add(GameObject src, GameObject dst)
            {
                SrcBones.Add(src?.transform);
                if (dst != null)
                {
                    DstBones.Add(dst.transform);
                }
                if (src != null)
                {
                    Map.Add(src?.transform, dst?.transform);
                }
            }

            public IEnumerable<BoneWeight> CreateBoneWeight(int vertexCount)
            {
                int j = 0;
                for (int i = 0; i < vertexCount; ++i)
                {
                    yield return new BoneWeight
                    {
                        boneIndex0 = j++,
                        boneIndex1 = j++,
                        boneIndex2 = j++,
                        boneIndex3 = j++,
                        weight0 = 0.25f,
                        weight1 = 0.25f,
                        weight2 = 0.25f,
                        weight3 = 0.25f,
                    };
                }
            }
        }

        [Test]
        public void MapBoneWeightTest()
        {
            {
                var map = new BoneMap();
                map.Add(new GameObject("a"), new GameObject("A"));
                map.Add(new GameObject("b"), new GameObject("B"));
                map.Add(new GameObject("c"), new GameObject("C"));
                map.Add(new GameObject("d"), new GameObject("D"));
                map.Add(null, new GameObject("null"));
                // map.Add(new GameObject("c"), null); // ありえないので Exception にしてある
                var boneWeights = map.CreateBoneWeight(64).ToArray();
                var newBoneWeight = BoneNormalizer.MapBoneWeight(boneWeights, map.Map,
                    map.SrcBones.ToArray(), map.DstBones.ToArray());

                // 正常系
                // exception が出なければよい
            }

            {
                var map = new BoneMap();
                map.Add(new GameObject("a"), new GameObject("A"));
                map.Add(new GameObject("b"), new GameObject("B"));
                map.Add(new GameObject("c"), new GameObject("C"));
                map.Add(new GameObject("d"), new GameObject("D"));
                map.Add(null, new GameObject("null"));
                // map.Add(new GameObject("c"), null); // ありえないので Exception にしてある
                var boneWeights = map.CreateBoneWeight(64).ToArray();
                var newBoneWeight = BoneNormalizer.MapBoneWeight(boneWeights, map.Map,
                    map.SrcBones.ToArray(), map.DstBones.ToArray());

                // 4 つめが 0 になる
                Assert.AreEqual(0, newBoneWeight[1].boneIndex0);
                Assert.AreEqual(0, newBoneWeight[1].weight0);
                // 5 つめ以降が 0 になる。out of range
                Assert.AreEqual(0, newBoneWeight[1].boneIndex1);
                Assert.AreEqual(0, newBoneWeight[1].weight1);
            }
        }
    }
}
