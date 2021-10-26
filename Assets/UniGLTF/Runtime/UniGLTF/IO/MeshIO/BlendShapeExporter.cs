using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class BlendShapeExporter
    {
        public static gltfMorphTarget Export(ExportingGltfData  w, Vector3[] positions, Vector3[] normals, bool useSparse)
        {
            var accessorCount = positions.Length;
            if (normals != null && positions.Length != normals.Length)
            {
                throw new Exception();
            }

            int[] sparseIndices = default;
            if (useSparse)
            {
                sparseIndices = Enumerable.Range(0, positions.Length).Where(x => positions[x] != Vector3.zero).ToArray();
                if (sparseIndices.Length == 0)
                {
                    // sparse 対象がすべて [0, 0, 0] の場合
                    // new glTFSparse
                    // {
                    //     count = 0,
                    // }
                    // のようになる。
                    // たぶん、仕様的にはあり。
                    // 解釈できない場合あり。
                    useSparse = false;
                }
            }

            if (useSparse)
            {
                if (sparseIndices == null)
                {
                    throw new Exception();
                }

                // positions
                var positionAccessorIndex = -1;
                if (sparseIndices.Length > 0)
                {
                    var sparseIndicesViewIndex = w.ExtendBufferAndGetViewIndex(sparseIndices);
                    positionAccessorIndex = w.ExtendSparseBufferAndGetAccessorIndex(accessorCount,
                        sparseIndices.Select(x => positions[x]).ToArray(), sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.NONE);
                }

                // normals
                var normalAccessorIndex = -1;
                if (normals != null)
                {
                    var sparseNormalIndices = Enumerable.Range(0, positions.Length).Where(x => normals[x] != Vector3.zero).ToArray();
                    if (sparseNormalIndices.Length > 0)
                    {
                        var sparseNormalIndicesViewIndex = w.ExtendBufferAndGetViewIndex(sparseNormalIndices);
                        normalAccessorIndex = w.ExtendSparseBufferAndGetAccessorIndex(accessorCount,
                            sparseNormalIndices.Select(x => normals[x]).ToArray(), sparseNormalIndices, sparseNormalIndicesViewIndex,
                            glBufferTarget.NONE);
                    }
                }

                return new gltfMorphTarget
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                };
            }
            else
            {
                // position
                var positionAccessorIndex = w.ExtendBufferAndGetAccessorIndex(positions, glBufferTarget.ARRAY_BUFFER);
                w.GLTF.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
                w.GLTF.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

                // normal
                var normalAccessorIndex = -1;
                if (normals != null)
                {
                    normalAccessorIndex = w.ExtendBufferAndGetAccessorIndex(normals, glBufferTarget.ARRAY_BUFFER);
                }

                return new gltfMorphTarget
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                };
            }
        }
    }
}
