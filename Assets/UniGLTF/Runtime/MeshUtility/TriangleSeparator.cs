using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    class TriangleSeparator
    {
        bool[] VertexHasBlendShape;

        public bool ShouldSplit
        {
            get
            {
                var count = VertexHasBlendShape.Count(x => x);
                // すべて true か false の場合は分割しない
                return count > 0 && count < VertexHasBlendShape.Length;
            }
        }

        public bool TriangleHasBlendShape(int i0, int i1, int i2)
        {
            return VertexHasBlendShape[i0]
            || VertexHasBlendShape[i1]
            || VertexHasBlendShape[i2];
        }

        public bool TriangleHasNotBlendShape(int i0, int i1, int i2)
        {
            return !TriangleHasBlendShape(i0, i1, i2);
        }

        public TriangleSeparator(int vertexCount)
        {
            VertexHasBlendShape = new bool[vertexCount];
        }

        public void CheckPositions(IReadOnlyList<Vector3> positions)
        {
            for (int i = 0; i < positions.Count; ++i)
            {
                if (positions[i] != Vector3.zero)
                {
                    VertexHasBlendShape[i] = true;
                }
            }
        }
    }
}