using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    class MeshSplitter
    {
        public static ValueTuple<
            List<ValueTuple<int, Triangle>>,
            List<ValueTuple<int, Triangle>>> SplitTriangles(Mesh src)
        {
            var triangles = src.Triangles.ToArray();
            var morphUseCount = new int[triangles.Length];

            // 各モーフ
            foreach (var morph in src.MorphTargets)
            {
                if (morph.VertexBuffer.Count == 0)
                {
                    // 無効
                    continue;
                }
                // POSITIONが使っているtriangleのカウンターをアップさせる
                var positions = SpanLike.Wrap<Vector3>(morph.VertexBuffer.Positions.Bytes);
                for (int i = 0; i < triangles.Length; ++i)
                {
                    ref var triangle = ref triangles[i];
                    if (positions[triangle.Item2.Vertex0] != Vector3.Zero)
                    {
                        ++morphUseCount[i];
                    }
                    if (positions[triangle.Item2.Vertex1] != Vector3.Zero)
                    {
                        ++morphUseCount[i];
                    }
                    if (positions[triangle.Item2.Vertex2] != Vector3.Zero)
                    {
                        ++morphUseCount[i];
                    }
                }
            }

            var withTriangles = new List<ValueTuple<int, Triangle>>();
            var withoutTriangles = new List<ValueTuple<int, Triangle>>();
            for (int i = 0; i < triangles.Length; ++i)
            {
                if (morphUseCount[i] > 0)
                {
                    // モーフで使われている
                    withTriangles.Add(triangles[i]);
                }
                else
                {
                    // モーフで使われない
                    withoutTriangles.Add(triangles[i]);
                }
            }

            return (withTriangles, withoutTriangles);
        }

        public static ValueTuple<
           List<ValueTuple<int, Triangle>>,
           List<ValueTuple<int, Triangle>>> SplitTrianglesByBoneIndices(Mesh src, HashSet<int> targetBoneIndices)
        {
            var triangles = src.Triangles.ToArray();
            var isBodyMeshTriangle = new bool[triangles.Length];

            var skinJointsSpan = SpanLike.Wrap<SkinJoints>(src.VertexBuffer.Joints.Bytes);
            var boneWeightSpan = SpanLike.Wrap<Vector4>(src.VertexBuffer.Weights.Bytes);

            for (int i = 0; i < triangles.Length; ++i)
            {
                ref var triangle = ref triangles[i];

                var skinJoints0 = skinJointsSpan[triangle.Item2.Vertex0];
                var boneWeights0 = boneWeightSpan[triangle.Item2.Vertex0];

                if (boneWeights0.X > 0 && targetBoneIndices.Contains(skinJoints0.Joint0)) continue;
                if (boneWeights0.Y > 0 && targetBoneIndices.Contains(skinJoints0.Joint1)) continue;
                if (boneWeights0.Z > 0 && targetBoneIndices.Contains(skinJoints0.Joint2)) continue;
                if (boneWeights0.W > 0 && targetBoneIndices.Contains(skinJoints0.Joint3)) continue;

                var skinJoints1 = skinJointsSpan[triangle.Item2.Vertex1];
                var boneWeights1 = boneWeightSpan[triangle.Item2.Vertex1];

                if (boneWeights1.X > 0 && targetBoneIndices.Contains(skinJoints1.Joint0)) continue;
                if (boneWeights1.Y > 0 && targetBoneIndices.Contains(skinJoints1.Joint1)) continue;
                if (boneWeights1.Z > 0 && targetBoneIndices.Contains(skinJoints1.Joint2)) continue;
                if (boneWeights1.W > 0 && targetBoneIndices.Contains(skinJoints1.Joint3)) continue;

                var skinJoints2 = skinJointsSpan[triangle.Item2.Vertex2];
                var boneWeights2 = boneWeightSpan[triangle.Item2.Vertex2];

                if (boneWeights2.X > 0 && targetBoneIndices.Contains(skinJoints2.Joint0)) continue;
                if (boneWeights2.Y > 0 && targetBoneIndices.Contains(skinJoints2.Joint1)) continue;
                if (boneWeights2.Z > 0 && targetBoneIndices.Contains(skinJoints2.Joint2)) continue;
                if (boneWeights2.W > 0 && targetBoneIndices.Contains(skinJoints2.Joint3)) continue;

                isBodyMeshTriangle[i] = true;
            }

            var bodyTriangles = new List<ValueTuple<int, Triangle>>();
            var headTriangles = new List<ValueTuple<int, Triangle>>();

            for (int i = 0; i < triangles.Length; ++i)
            {
                if (isBodyMeshTriangle[i])
                {
                    //　胴体メッシュ
                    bodyTriangles.Add(triangles[i]);
                }
                else
                {
                    // 頭メッシュ
                    headTriangles.Add(triangles[i]);
                }
            }

            return (headTriangles, bodyTriangles);
        }

        class VertexReorderMapper
        {
            public readonly List<int> IndexMap = new List<int>();

            public BufferAccessor MapBuffer<T>(BufferAccessor srcBuffer) where T : struct
            {
                var src = SpanLike.Wrap<T>(srcBuffer.Bytes);
                var dstBytes = new byte[srcBuffer.Stride * IndexMap.Count];
                var dst = SpanLike.Wrap<T>(new ArraySegment<byte>(dstBytes));
                for (int i = 0; i < IndexMap.Count; ++i)
                {
                    dst[i] = src[IndexMap[i]];
                }
                return new BufferAccessor(new ArraySegment<byte>(dstBytes), srcBuffer.ComponentType, srcBuffer.AccessorType, IndexMap.Count);
            }

            public VertexBuffer Map(VertexBuffer src)
            {
                var dst = new VertexBuffer();
                foreach (var (semantic, buffer) in src)
                {
                    BufferAccessor mapped = null;
                    if (buffer.ComponentType == AccessorValueType.FLOAT)
                    {
                        switch (buffer.AccessorType)
                        {
                            case AccessorVectorType.VEC2:
                                mapped = MapBuffer<Vector2>(buffer);
                                break;

                            case AccessorVectorType.VEC3:
                                mapped = MapBuffer<Vector3>(buffer);
                                break;

                            case AccessorVectorType.VEC4:
                                mapped = MapBuffer<Vector4>(buffer);
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else if (buffer.ComponentType == AccessorValueType.UNSIGNED_SHORT)
                    {
                        if (buffer.AccessorType == AccessorVectorType.VEC4)
                        {
                            mapped = MapBuffer<SkinJoints>(buffer);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    dst.Add(semantic, mapped);
                }

                return dst;
            }

            public readonly List<int> Indices = new List<int>();

            public ArraySegment<byte> IndexBytes
            {
                get
                {
                    var bytes = new byte[Indices.Count * 4];
                    var span = SpanLike.Wrap<Int32>(new ArraySegment<byte>(bytes));
                    for (int i = 0; i < span.Length; ++i)
                    {
                        span[i] = Indices[i];
                    }
                    return new ArraySegment<byte>(bytes);
                }
            }

            public BufferAccessor CreateIndexBuffer()
            {
                return new BufferAccessor(IndexBytes,
                AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, Indices.Count);
            }

            void PushIndex(int src)
            {
                int index = IndexMap.IndexOf(src);
                if (index == -1)
                {
                    // index to src map
                    index = IndexMap.Count;
                    IndexMap.Add(src);
                }
                Indices.Add(index);
            }

            public VertexReorderMapper(IEnumerable<ValueTuple<int, Triangle>> submeshTriangles)
            {
                foreach (var t in submeshTriangles)
                {
                    PushIndex(t.Item2.Vertex0);
                    PushIndex(t.Item2.Vertex1);
                    PushIndex(t.Item2.Vertex2);
                }
            }
        }
        public static Mesh SeparateMesh(Mesh src, IEnumerable<ValueTuple<int, Triangle>> submeshTriangles,
            bool includeMorphTarget = false)
        {
            var mapper = new VertexReorderMapper(submeshTriangles);

            var mesh = new Mesh
            {
                Topology = TopologyType.Triangles,
                IndexBuffer = mapper.CreateIndexBuffer(),
                VertexBuffer = mapper.Map(src.VertexBuffer),
            };

            var offset = 0;
            foreach (var triangles in submeshTriangles.GroupBy(x => x.Item1, x => x).OrderBy(x => x.Key))
            {
                var count = triangles.Count() * 3;
                mesh.Submeshes.Add(new Submesh(offset, count, src.Submeshes[triangles.Key].Material));
                offset += count;
            }

            if (includeMorphTarget)
            {
                foreach (var target in src.MorphTargets)
                {
                    mesh.MorphTargets.Add(new MorphTarget(target.Name)
                    {
                        VertexBuffer = mapper.Map(target.VertexBuffer)
                    });
                }
            }

            return mesh;
        }
    }
}