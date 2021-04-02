using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace VrmLib
{
    public static class MeshExtensions
    {
        // Skin.Normalize
        public static void ApplyRotationAndScaling(this Mesh mesh, Matrix4x4 m)
        {
            m.Translation = Vector3.Zero;

            var position = SpanLike.Wrap<Vector3>(mesh.VertexBuffer.Positions.Bytes);
            var normal = SpanLike.Wrap<Vector3>(mesh.VertexBuffer.Normals.Bytes);

            for (int i = 0; i < position.Length; ++i)
            {
                {
                    var dst = Vector4.Transform(new Vector4(position[i], 1), m);
                    position[i] = new Vector3(dst.X, dst.Y, dst.Z);
                }
                {
                    var dst = Vector4.Transform(new Vector4(normal[i], 0), m);
                    normal[i] = new Vector3(dst.X, dst.Y, dst.Z);
                }
            }
        }

        // joint index の調整
        static void FixSkinJoints(SpanLike<SkinJoints> joints, SpanLike<Vector4> weights, int[] jointIndexMap)
        {
            for (int i = 0; i < joints.Length; ++i)
            {
                var j = joints[i];
                var w = weights[i];

                var sum = w.X + w.Y + w.Z + w.W;
                if (sum == 0)
                {
                    throw new Exception("zero weight");
                }
                var factor = 1.0f / sum;

                {
                    var index = jointIndexMap[j.Joint0];
                    j.Joint0 = (ushort)(index >= 0 ? index : 0);
                    w.X *= factor;
                }

                {
                    var index = jointIndexMap[j.Joint1];
                    j.Joint1 = (ushort)(index >= 0 ? index : 0);
                    w.X *= factor;
                }

                {
                    var index = jointIndexMap[j.Joint2];
                    j.Joint2 = (ushort)(index >= 0 ? index : 0);
                    w.X *= factor;
                }

                {
                    var index = jointIndexMap[j.Joint3];
                    j.Joint3 = (ushort)(index >= 0 ? index : 0);
                    w.X *= factor;
                }

                weights[i] = w;
                joints[i] = j;
            }
        }

        /// weightを付与して、matrixを適用する
        static void FixNoSkinJoints(SpanLike<SkinJoints> joints, SpanLike<Vector4> weights, int jointIndex,
            SpanLike<Vector3> positions, SpanLike<Vector3> normals, Matrix4x4 matrix)
        {
            for (int i = 0; i < joints.Length; ++i)
            {
                var j = new SkinJoints
                {
                    Joint0 = (ushort)jointIndex
                };
                joints[i] = j;

                var w = new Vector4
                {
                    X = 1.0f,
                };
                weights[i] = w;

                {
                    // position
                    var src = positions[i];
                    var dst = Vector4.Transform(new Vector4(src, 1), matrix);
                    positions[i] = new Vector3(dst.X, dst.Y, dst.Z);
                }
                {
                    // normal
                    var src = normals[i];
                    var dst = Vector4.Transform(new Vector4(src, 0), matrix);
                    normals[i] = new Vector3(dst.X, dst.Y, dst.Z);
                }
            }
        }

        // Meshを連結する。SingleMesh で使う
        public static void Append(this Mesh mesh, VertexBuffer vertices, BufferAccessor indices,
            List<Submesh> submeshes,
            List<MorphTarget> targets,
            int[] jointIndexMap,
            int rootIndex = -1,
            Matrix4x4 matrix = default(Matrix4x4))
        {
            var lastCount = mesh.VertexBuffer != null ? mesh.VertexBuffer.Count : 0;

            // index buffer
            if (mesh.IndexBuffer == null)
            {
                var accessor = new BufferAccessor(new ArraySegment<byte>(new byte[0]), AccessorValueType.UNSIGNED_INT, indices.AccessorType, 0);
                mesh.IndexBuffer = accessor;

                mesh.IndexBuffer.Append(indices, 0);
            }
            else
            {
                mesh.IndexBuffer.Append(indices, lastCount);
            }

            {
                var submeshOffset = mesh.SubmeshTotalDrawCount;
                for (int i = 0; i < submeshes.Count; ++i)
                {
                    var submesh = submeshes[i];
                    mesh.Submeshes.Add(new Submesh(submeshOffset, submesh.DrawCount, submesh.Material));
                    submeshOffset += submesh.DrawCount;
                }
            }

            // vertex buffer
            var vertexOffset = 0;
            if (mesh.VertexBuffer == null)
            {
                mesh.VertexBuffer = vertices;
            }
            else
            {
                vertexOffset = mesh.VertexBuffer.Count;
                mesh.VertexBuffer.Append(vertices);
            }

            if (jointIndexMap != null && mesh.VertexBuffer.Joints != null && mesh.VertexBuffer.Weights != null)
            {
                // JOINT index 参照の修正
                var joints = SpanLike.Wrap<SkinJoints>(mesh.VertexBuffer.Joints.Bytes).Slice(vertexOffset);
                var weights = SpanLike.Wrap<Vector4>(mesh.VertexBuffer.Weights.Bytes).Slice(vertexOffset);
                FixSkinJoints(joints, weights, jointIndexMap);
            }
            else
            {
                var position = SpanLike.Wrap<Vector3>(mesh.VertexBuffer.Positions.Bytes).Slice(vertexOffset);
                var normal = SpanLike.Wrap<Vector3>(mesh.VertexBuffer.Normals.Bytes).Slice(vertexOffset);
                var joints = mesh.VertexBuffer.GetOrCreateJoints().Slice(vertexOffset);
                var weights = mesh.VertexBuffer.GetOrCreateWeights().Slice(vertexOffset);
                // Nodeの姿勢を反映して
                // JOINT と WEIGHT を追加する
                FixNoSkinJoints(joints, weights, rootIndex, position, normal, matrix);
            }

            // morph target
            foreach (var target in targets)
            {
                if (string.IsNullOrEmpty(target.Name))
                {
                    continue;
                }

                foreach (var kv in target.VertexBuffer)
                {
                    if (kv.Value.Count != target.VertexBuffer.Count)
                    {
                        throw new Exception("different length");
                    }
                }

                var found = mesh.MorphTargets.FirstOrDefault(x => x.Name == target.Name);
                if (found == null)
                {
                    // targetの前に0を足す
                    found = new MorphTarget(target.Name);
                    found.VertexBuffer = target.VertexBuffer.CloneWithOffset(lastCount);
                    mesh.MorphTargets.Add(found);

                    foreach (var kv in found.VertexBuffer)
                    {
                        if (kv.Value.Count != mesh.VertexBuffer.Count)
                        {
                            throw new Exception();
                        }
                    }
                }
                else
                {
                    found.VertexBuffer.Resize(lastCount);

                    // foundの後ろにtargetを足す
                    found.VertexBuffer.Append(target.VertexBuffer);

                    foreach (var kv in found.VertexBuffer)
                    {
                        if (kv.Value.Count != mesh.VertexBuffer.Count)
                        {
                            throw new Exception();
                        }
                    }
                }
            }
        }

        class SubmeshReorderMapper
        {
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

            public SubmeshReorderMapper(IEnumerable<ValueTuple<int, Triangle>> submeshTriangles)
            {
                foreach (var t in submeshTriangles)
                {
                    Indices.Add(t.Item2.Vertex0);
                    Indices.Add(t.Item2.Vertex1);
                    Indices.Add(t.Item2.Vertex2);
                }
            }
        }

        public static string IntegrateSubmeshes(this Mesh mesh)
        {
            var before = mesh.Submeshes.Count;

            // 同じMaterialのMeshが連続するようにIndexを並べ替える
            var sorted = new List<Submesh>();
            var source = mesh.Submeshes.ToList(); // copy
            while (source.Any())
            {
                var first = source[0];
                source.RemoveAt(0);

                sorted.Add(first);
                sorted.AddRange(source.Where(x => x.Material == first.Material)); // 同じMaterialを追加
                source.RemoveAll(x => x.Material == first.Material); // 同じMaterialを削除
            }
            // Submeshを並べ替えて連続するものを連結する
            mesh.ReorderSubmeshes(sorted);

            return $"({before} => {mesh.Submeshes.Count})";
        }

        struct Vertex : IComparable<Vertex>, IEquatable<Vertex>
        {
            public static float PositionThreshold = float.Epsilon;

            public static float NormalAngleThreshold = 100.0f;

            public static float UVThreshold = 0.0001f;

            /// <summary>
            /// 他の頂点の統合先となって、残るフラグ
            /// </summary>
            public bool IsMergeTarget
            {
                get
                {
                    return Index == targetIndex;
                }
            }

            /// <summary>
            /// 統合先の頂点Index
            /// 統合後も残るはtargetIndex == Index
            /// </summary>
            public int targetIndex;

            public int Index;

            public Vector3 Position;

            public Vector3? Normal;

            public Vector2? UV;

            public SkinJoints? Joints;

            public Vector4? Weight;

            public Vector4? Color;

            // 必要に応じて同一判定に使う頂点パラメータを追加する

            public Vertex(int index, Vector3 position, Vector3? normal, Vector2? uv, SkinJoints? joints, Vector4? weight, Vector4? color)
            {
                Index = index;
                Position = position;
                Normal = normal;
                UV = uv;
                Joints = joints;
                Weight = weight;
                Color = color;
                targetIndex = -1;
            }

            /// <summary>
            /// 座標ソートのときの比較基準を記述
            /// 必ずしもX軸基準である必要は特にない
            /// </summary>        
            public int CompareTo(Vertex other)
            {
                if (this.Position.X == other.Position.X) return 0;
                else if (this.Position.X > other.Position.X) return 1;
                else return -1;
            }

            public bool Equals(Vertex other)
            {
                if (Vector3.Distance(this.Position, other.Position) > PositionThreshold) return false;

                if (this.Normal.HasValue && other.Normal.HasValue)
                {
                    var v = this.Normal.Value;
                    var u = other.Normal.Value;
                    var angle = (Math.Acos(Vector3.Dot(v, u) / (v.Length() * u.Length()))) * (180 / Math.PI);
                    if (angle > NormalAngleThreshold) return false;
                }

                if ((this.UV.HasValue && other.UV.HasValue) && Vector2.Distance(this.UV.Value, other.UV.Value) > UVThreshold) return false;

                if ((this.Joints.HasValue && other.Joints.HasValue) && !(this.Joints.Value.Equals(other.Joints.Value))) return false;

                return true;
            }
        }

#if false
        /// <summary>
        /// 同一位置にある頂点を統合する
        /// </summary>
        public static string MergeSameVertices(this Mesh mesh)
        {
            var indices = mesh.IndexBuffer.GetAsIntArray();

            var positions = SpanLike.CreateVector3(mesh.VertexBuffer.Positions.Bytes);
            var normals = mesh.VertexBuffer.Normals != null ? SpanLike.CreateVector3(mesh.VertexBuffer.Normals.Bytes) : default;
            var UVs = mesh.VertexBuffer.TexCoords != null ? SpanLike.CreateVector2(mesh.VertexBuffer.TexCoords.Bytes) : default;
            var joints = mesh.VertexBuffer.Joints != null ? SpanLike.CreateSkinJoints(mesh.VertexBuffer.Joints.Bytes) : default;
            var weights = mesh.VertexBuffer.Weights != null ? SpanLike.CreateVector4(mesh.VertexBuffer.Weights.Bytes) : default;
            var colors = mesh.VertexBuffer.Colors != null ? SpanLike.CreateVector4(mesh.VertexBuffer.Colors.Bytes) : default;

            int before = positions.Length;

            var verts = new Vertex[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];

                Vector3? normal = null;
                if (normals.Length > 0) normal = normals[i];

                Vector2? uv = null;
                if (UVs.Length > 0) uv = UVs[i];

                SkinJoints? joint = null;
                if (joints.Length > 0) joint = joints[i];

                Vector4? weight = null;
                if (weights.Length > 0) weight = weights[i];

                Vector4? color = null;
                if (colors.Length > 0) color = colors[i];
                verts[i] = new Vertex(i, pos, normal, uv, joint, weight, color);
            }

            Array.Sort(verts);

            // var vertices = new Span<Vertex>(verts);
            var vertices = new ArraySegment<Vertex>(verts);

            var stride = 200; // 総当り比較するブロック数。小さいほど計算は早いが余裕を持たせてある程度の値を確保している

            for (int i = 0; i < vertices.Count; i += (stride))
            {
                stride = Math.Min(stride, vertices.Count - i);
                var dividedIndices = vertices.Slice(i, stride);
                FindMergeTargetVertices(ref dividedIndices);
            }

            // 統合する頂点の組み合わせを作成
            var mergePairVatexDictionary = new Dictionary<int, int>(); // key: remove vertex index , value : merge target vertex index
            foreach (var v in vertices)
            {
                if (v.IsMergeTarget) continue;
                if (v.targetIndex > 0)
                {
                    if (mergePairVatexDictionary.ContainsKey(v.Index)) continue;
                    mergePairVatexDictionary.Add(v.Index, v.targetIndex);
                }
            }

            // 統合されて消える頂点ID群を作成
            var removeIndexList = new List<int>(); // index補正用
            var removeIndexHashSet = new HashSet<int>();
            foreach (var v in vertices)
            {
                if (v.IsMergeTarget) continue;
                if (v.targetIndex > 0)
                {
                    removeIndexList.Add(v.Index);
                    removeIndexHashSet.Add(v.Index);
                }
            }
            removeIndexList.Sort();

            int resizedLength = positions.Length - removeIndexHashSet.Count();

            var indexOffsetArray = new int[indices.Length];

            int offset = 0;
            int currentIndex = 0;

            // 統合後の頂点IDの補正値を計算
            foreach (var index in removeIndexList)
            {
                for (int i = currentIndex; i <= index; i++)
                {
                    indexOffsetArray[i] = offset;
                }
                currentIndex = index + 1;
                offset++;
            }
            for (int i = currentIndex; i < indices.Length; i++)
            {
                indexOffsetArray[i] = offset;
            }

            // merge vertex index
            for (int i = 0; i < indices.Length; i++)
            {
                if (mergePairVatexDictionary.ContainsKey(indices[i]))
                {
                    var newIndex = mergePairVatexDictionary[indices[i]];
                    var correctedIndex = newIndex - indexOffsetArray[newIndex];

                    if (correctedIndex < 0 || correctedIndex >= resizedLength)
                    {
                        throw new IndexOutOfRangeException("recalculated vertex index are invalid after vertex merged");
                    }
                    indices[i] = correctedIndex;
                }
                else
                {
                    var correctedIndex = indices[i] - indexOffsetArray[indices[i]];

                    if (correctedIndex < 0 || correctedIndex > resizedLength)
                    {
                        throw new IndexOutOfRangeException("recalculated vertex index are invalid after vertex merged");
                    }

                    indices[i] = correctedIndex;
                }
            }

            if (positions.Length > ushort.MaxValue)
            {
                mesh.IndexBuffer.Assign(indices.AsSpan());
            }
            else
            {
                mesh.IndexBuffer.AssignAsShort(indices.AsSpan());
            }

            // 統合後、平均値を割り当てるパラメータは更新する
            foreach (var v in vertices)
            {
                if (v.IsMergeTarget)
                {
                    if (normals != null) normals[v.Index] = v.Normal.Value;
                    if (colors != null) colors[v.Index] = v.Color.Value;
                }
            }

            // merge vertex 
            var newPositions = new Vector3[resizedLength];
            var newNormals = normals != null ? new Vector3[resizedLength] : null;
            var newJoints = joints != null ? new SkinJoints[resizedLength] : null;
            var newWeights = weights != null ? new Vector4[resizedLength] : null;
            var newUVs = UVs != null ? new Vector2[resizedLength] : null;
            var newColors = colors != null ? new Vector4[resizedLength] : null;

            for (int i = 0, targetIndex = 0; i < positions.Length; i++)
            {
                if (removeIndexHashSet.Contains(i)) continue;
                newPositions[targetIndex] = positions[i];
                if (normals != null) newNormals[targetIndex] = normals[i];
                if (joints != null) newJoints[targetIndex] = joints[i];
                if (weights != null) newWeights[targetIndex] = weights[i];
                if (UVs != null) newUVs[targetIndex] = UVs[i];
                if (colors != null) newColors[targetIndex] = colors[i];

                targetIndex++;
            }

            mesh.VertexBuffer.Positions.Assign(newPositions.AsSpan());
            if (normals != null) mesh.VertexBuffer.Normals.Assign(newNormals.AsSpan());
            if (joints != null) mesh.VertexBuffer.Joints.Assign(newJoints.AsSpan());
            if (weights != null) mesh.VertexBuffer.Weights.Assign(newWeights.AsSpan());
            if (UVs != null) mesh.VertexBuffer.TexCoords.Assign(newUVs.AsSpan());
            if (colors != null) mesh.VertexBuffer.Colors.Assign(newColors.AsSpan());
            mesh.VertexBuffer.RemoveTangent();
            mesh.VertexBuffer.ValidateLength();

            //
            // merge morph vertex buffer
            //
            Span<Vector3> morphPositions = null;
            Span<Vector3> morphNormals = null;
            Span<Vector2> morphUVs = null;
            foreach (var morph in mesh.MorphTargets)
            {
                morphPositions = morph.VertexBuffer.Positions != null ? morph.VertexBuffer.Positions.GetSpan<Vector3>() : null;
                morphNormals = morph.VertexBuffer.Normals != null ? morph.VertexBuffer.Normals.GetSpan<Vector3>() : null;
                morphUVs = morph.VertexBuffer.TexCoords != null ? morph.VertexBuffer.TexCoords.GetSpan<Vector2>() : null;

                for (int i = 0, targetIndex = 0; i < positions.Length; i++)
                {
                    if (removeIndexHashSet.Contains(i)) continue;

                    if (morphPositions != null) newPositions[targetIndex] = morphPositions[i];
                    if (morphNormals != null) newNormals[targetIndex] = morphNormals[i];
                    if (morphUVs != null) newUVs[targetIndex] = morphUVs[i];
                    targetIndex++;
                }

                if (morphPositions != null) morph.VertexBuffer.Positions.Assign(newPositions.AsSpan());
                if (morphNormals != null) morph.VertexBuffer.Normals.Assign(newNormals.AsSpan());
                if (morphUVs != null) morph.VertexBuffer.Normals.Assign(newUVs.AsSpan());
                morph.VertexBuffer.RemoveTangent();
                morph.VertexBuffer.ValidateLength(morph.Name);
            }

            return $"({before} => {resizedLength})";
        }

        /// <summary>
        /// 統合できる同一とみなせる頂点を探す
        /// </summary>
        private static void FindMergeTargetVertices(SpanLike<Vertex> vertices)
        {            
            var sameVertices = new HashSet<Vertex>();
            for (int i = 0; i < vertices.Length; i++)
            {
                sameVertices.Clear();
                var v0 = vertices[i];
                int minIndex = v0.Index;
                if (v0.targetIndex > 0) continue;
                for (int j = i + 1; j < vertices.Length; j++)
                {
                    var v1 = vertices[j];

                    if (v1.targetIndex > 0) continue;

                    if (!v0.Equals(v1)) continue;

                    if (!sameVertices.Contains(v0)) sameVertices.Add(v0);
                    if (!sameVertices.Contains(v1))
                    {
                        sameVertices.Add(v1);
                        if (v1.Index < minIndex) minIndex = v1.Index;
                    }
                }

                if (sameVertices.Count() >= 2)
                {
                    foreach (var v in sameVertices)
                    {
                        v.targetIndex = minIndex;

                        if (v.IsMergeTarget)
                        {
                            Vector3? n = Vector3.Zero;
                            Vector4? c = Vector4.Zero;

                            foreach (var vert in sameVertices)
                            {
                                if (vert.Normal.HasValue) n += vert.Normal;
                                if (vert.Color.HasValue) c += vert.Color;

                            }
                            if (n.HasValue) Vector3.Normalize(n.Value);
                            if (c.HasValue) c = c.Value / sameVertices.Count();


                            v.Normal = n;
                            v.Color = c;
                        }
                    }
                }
            }

            return;
        }
#endif

        static void ReorderSubmeshes(this Mesh mesh, List<Submesh> order)
        {
            if (mesh.Submeshes.Count != order.Count)
            {
                throw new Exception();
            }
            mesh.Submeshes.Clear();
            mesh.Submeshes.AddRange(order);

            // index buffer を作り直す
            mesh.IndexBuffer = new SubmeshReorderMapper(mesh.Triangles).CreateIndexBuffer();

            // submesh の offset を正規化する
            var offset = 0;
            for (int i = 0; i < mesh.Submeshes.Count; ++i)
            {
                var submesh = mesh.Submeshes[i];
                submesh.Offset = offset;
                offset += submesh.DrawCount;
            }

            {
                // 連続して同じMaterialのSubMeshを連結する
                mesh.Submeshes.Clear();
                int current = -1;
                foreach (var submesh in order)
                {
                    if (current != submesh.Material)
                    {
                        current = submesh.Material;
                        mesh.Submeshes.Add(submesh);
                    }
                    else
                    {
                        mesh.Submeshes.Last().DrawCount += submesh.DrawCount;
                    }
                }
            }
        }
    }
}