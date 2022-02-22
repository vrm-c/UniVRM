using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;

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

        public TopologyType Topology = TopologyType.Triangles;

        public List<Submesh> Submeshes { private set; get; } = new List<Submesh>();

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

        // Skin.Normalize
        public void ApplyRotationAndScaling(Matrix4x4 m)
        {
            m.SetColumn(3, new Vector4(0, 0, 0, 1));

            var position = VertexBuffer.Positions.Bytes.Reinterpret<Vector3>(1);
            var normal = VertexBuffer.Normals.Bytes.Reinterpret<Vector3>(1);

            for (int i = 0; i < position.Length; ++i)
            {
                {
                    position[i] = m.MultiplyPoint(position[i]);
                }
                {
                    normal[i] = m.MultiplyVector(normal[i]);
                }
            }
        }
    }
}
