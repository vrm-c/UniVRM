using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    class BlendShapeReport
    {
        string m_name;
        int m_count;
        struct BlendShapeStat
        {
            public int Index;
            public string Name;
            public int VertexCount;
            public int NormalCount;
            public int TangentCount;

            public override string ToString()
            {
                return string.Format("[{0}]{1}: {2}, {3}, {4}\n", Index, Name, VertexCount, NormalCount, TangentCount);
            }
        }
        List<BlendShapeStat> m_stats = new List<BlendShapeStat>();
        public int Count
        {
            get { return m_stats.Count; }
        }
        public BlendShapeReport(Mesh mesh)
        {
            m_name = mesh.name;
            m_count = mesh.vertexCount;
        }
        public void SetCount(int index, string name, int v, int n, int t)
        {
            m_stats.Add(new BlendShapeStat
            {
                Index = index,
                Name = name,
                VertexCount = v,
                NormalCount = n,
                TangentCount = t,
            });
        }
        public override string ToString()
        {
            return String.Format("NormalizeSkinnedMesh: {0}({1}verts)\n{2}",
                m_name,
                m_count,
                String.Join("", m_stats.Select(x => x.ToString()).ToArray()));
        }
    }
}