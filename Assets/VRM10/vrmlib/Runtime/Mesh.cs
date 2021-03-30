using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VrmLib
{
    public enum TopologyType : int
    {
        Points = 0,
        Lines = 1,
        LineLoop = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 6,
    }

    public struct Triangle
    {
        public int Vertex0;
        public int Vertex1;
        public int Vertex2;

        public Triangle(int v0, int v1, int v2)
        {
            Vertex0 = v0;
            Vertex1 = v1;
            Vertex2 = v2;
        }
    }

    public class Submesh
    {
        public int Offset;
        public int DrawCount;
        public int Material;

        public override string ToString()
        {
            return $"{Material}({DrawCount})";
        }

        public Submesh(int material) : this(0, 0, material)
        {
        }

        public Submesh(int offset, int drawCount, int material)
        {
            Offset = offset;
            DrawCount = drawCount;
            Material = material;
        }
    }

    public class Mesh
    {
        public VertexBuffer VertexBuffer;

        public BufferAccessor IndexBuffer;

        /// <summary>
        /// indicesの最大値が65535未満(-1を避ける)ならばushort 型で、
        /// そうでなければ int型で IndexBufferを代入する
        /// </summary>
        public void AssignIndexBuffer(SpanLike<int> indices)
        {
            bool isInt = false;
            foreach (var i in indices)
            {
                if (i >= short.MaxValue)
                {
                    isInt = true;
                    break;
                }
            }

            if (isInt)
            {
                if (IndexBuffer.Stride != 4)
                {
                    IndexBuffer.ComponentType = AccessorValueType.UNSIGNED_INT;
                    if (IndexBuffer.AccessorType != AccessorVectorType.SCALAR)
                    {
                        throw new Exception();
                    }
                }
                // 変換なし
                IndexBuffer.Assign(indices);
            }
            else
            {
                // int to ushort
                IndexBuffer.AssignAsShort(indices);
            }
        }

        public TopologyType Topology = TopologyType.Triangles;

        public List<Submesh> Submeshes { private set; get; } = new List<Submesh>();

        public int SubmeshTotalDrawCount => Submeshes.Sum(x => x.DrawCount);

        public IEnumerable<Triangle> GetTriangles(int i)
        {
            var indices = IndexBuffer.GetAsIntArray();

            var submesh = Submeshes[i];
            var submeshEnd = submesh.Offset + submesh.DrawCount;
            for (int j = submesh.Offset; j < submeshEnd; j += 3)
            {
                var triangle = new Triangle(
                    indices[j],
                    indices[j + 1],
                    indices[j + 2]
                );
                yield return triangle;
            }
        }

        public IEnumerable<ValueTuple<int, Triangle>> Triangles
        {
            get
            {
                if (Topology != TopologyType.Triangles)
                {
                    throw new InvalidOperationException();
                }

                var indices = IndexBuffer.GetAsIntArray();

                for (int i = 0; i < Submeshes.Count; ++i)
                {
                    var submesh = Submeshes[i];
                    var submeshEnd = submesh.Offset + submesh.DrawCount;
                    for (int j = submesh.Offset; j < submeshEnd; j += 3)
                    {
                        var triangle = new Triangle(
                            indices[j],
                            indices[j + 1],
                            indices[j + 2]
                        );
                        yield return (i, triangle);
                    }
                }
            }
        }

        bool GetSubmeshOverlapped<T>() where T : struct
        {
            var indices = IndexBuffer.GetSpan<ushort>();
            var offset = 0;
            var max = 0;
            foreach (var x in Submeshes)
            {
                var submeshIndices = indices.Slice(offset, x.DrawCount);
                var currentMax = 0;
                foreach (var y in submeshIndices)
                {
                    if (y < max)
                    {
                        return true;
                    }
                    currentMax = Math.Max(y, currentMax);
                }
                offset += x.DrawCount;
                max = currentMax;
            }
            return false;
        }

        public bool IsSubmeshOverlapped
        {
            get
            {
                if (Submeshes.Count <= 1)
                {
                    return false;
                }

                switch (IndexBuffer.ComponentType)
                {
                    case AccessorValueType.UNSIGNED_SHORT:
                        return GetSubmeshOverlapped<ushort>();

                    case AccessorValueType.UNSIGNED_INT:
                        return GetSubmeshOverlapped<uint>();

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public List<MorphTarget> MorphTargets = new List<MorphTarget>();

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var key in VertexBuffer.Select(x => x.Key))
            {
                sb.Append($"[{key}]");
            }
            if (IndexBuffer != null)
            {
                switch (Topology)
                {
                    case TopologyType.Triangles:
                        sb.Append($" {IndexBuffer.Count / 3} tris");
                        break;

                    default:
                        sb.Append($" topology: {Topology}");
                        break;
                }
            }
            if (MorphTargets.Any())
            {
                sb.Append($", {MorphTargets.Count} morphs");
                foreach (var kv in MorphTargets[0].VertexBuffer)
                {
                    sb.Append($"[{kv.Key}]");
                }
            }

            var byteLength = VertexBuffer.ByteLength + IndexBuffer.ByteLength + MorphTargets.Sum(x => x.VertexBuffer.ByteLength);

            sb.Append($": expected {byteLength / 1000 / 1000} MB");

            return sb.ToString();
        }

        public Mesh(TopologyType topology = TopologyType.Triangles)
        {
            Topology = topology;
        }

        public void RemoveUnusedSubmesh()
        {
            Submeshes = Submeshes.Where(x => x.DrawCount != 0).ToList();
        }
    }

    public class MeshGroup: GltfId
    {
        public readonly string Name;

        public readonly List<Mesh> Meshes = new List<Mesh>();

        public Skin Skin;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            if (Skin != null)
            {
                sb.Append("(skinned)");
            }
            var isFirst = true;
            foreach (var mesh in Meshes)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(mesh);
            }
            return sb.ToString();
        }

        public MeshGroup(string name)
        {
            Name = name;
        }
    }
}
