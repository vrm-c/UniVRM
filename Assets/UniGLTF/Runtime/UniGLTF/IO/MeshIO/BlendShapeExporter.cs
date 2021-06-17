using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class BlendShapeExporter
    {
        public static gltfMorphTarget Export(glTF gltf, int gltfBuffer, Vector3[] positions, Vector3[] normals, bool useSparse)
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
                    var sparseIndicesViewIndex = gltf.ExtendBufferAndGetViewIndex(gltfBuffer, sparseIndices);
                    positionAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(gltfBuffer, accessorCount,
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
                        var sparseNormalIndicesViewIndex = gltf.ExtendBufferAndGetViewIndex(gltfBuffer, sparseNormalIndices);
                        normalAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(gltfBuffer, accessorCount,
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
                var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(gltfBuffer, positions, glBufferTarget.ARRAY_BUFFER);
                gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
                gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

                // normal
                var normalAccessorIndex = -1;
                if (normals != null)
                {
                    normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(gltfBuffer, normals, glBufferTarget.ARRAY_BUFFER);
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
