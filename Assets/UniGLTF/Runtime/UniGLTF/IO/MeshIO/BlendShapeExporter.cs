using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public struct SparseBase
    {
        public readonly Vector3[] Positions;
        public readonly Vector3[] Normals;

        public SparseBase(Vector3[] positions, Vector3[] normals)
        {
            Positions = positions;
            Normals = normals;
        }
    }

    public static class BlendShapeExporter
    {
        public static gltfMorphTarget Export(glTF gltf, int gltfBuffer, Vector3[] positions, Vector3[] normals, SparseBase? sparseBase)
        {
            if (sparseBase.HasValue)
            {
                throw new NotImplementedException();
            }
            else
            {
                // position
                var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(gltfBuffer, positions, glBufferTarget.ARRAY_BUFFER);
                gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();

                // normal
                var normalAccessorIndex = -1;
                if (normals != null)
                {
                    normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(gltfBuffer, normals, glBufferTarget.ARRAY_BUFFER);
                }
                gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

                return new gltfMorphTarget
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                };
            }
        }
    }
}
