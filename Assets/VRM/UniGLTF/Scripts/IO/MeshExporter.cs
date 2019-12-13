using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public struct MeshWithRenderer
    {
        public Mesh Mesh;
        [Obsolete("Use Renderer")]
        public Renderer Rendererer { get { return Renderer; } set { Renderer = value; } }
        public Renderer Renderer;
    }

    public static class MeshExporter
    {
        static glTFMesh ExportPrimitives(glTF gltf, int bufferIndex,
            string rendererName,
            Mesh mesh, Material[] materials,
            List<Material> unityMaterials)
        {
            var positions = mesh.vertices.Select(y => y.ReverseZ()).ToArray();
            var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, positions, glBufferTarget.ARRAY_BUFFER);
            gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
            gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

            var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.normals.Select(y => y.normalized.ReverseZ()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#if GLTF_EXPORT_TANGENTS
            var tangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.tangents.Select(y => y.ReverseZ()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#endif
            var uvAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.uv.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var colorAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.colors, glBufferTarget.ARRAY_BUFFER);

            var boneweights = mesh.boneWeights;
            var weightAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneweights.Select(y => new Vector4(y.weight0, y.weight1, y.weight2, y.weight3)).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var jointsAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneweights.Select(y => new UShort4((ushort)y.boneIndex0, (ushort)y.boneIndex1, (ushort)y.boneIndex2, (ushort)y.boneIndex3)).ToArray(), glBufferTarget.ARRAY_BUFFER);

            var attributes = new glTFAttributes
            {
                POSITION = positionAccessorIndex,
            };
            if (normalAccessorIndex != -1)
            {
                attributes.NORMAL = normalAccessorIndex;
            }
#if GLTF_EXPORT_TANGENTS
            if (tangentAccessorIndex != -1)
            {
                attributes.TANGENT = tangentAccessorIndex;
            }
#endif
            if (uvAccessorIndex != -1)
            {
                attributes.TEXCOORD_0 = uvAccessorIndex;
            }
            if (colorAccessorIndex != -1)
            {
                attributes.COLOR_0 = colorAccessorIndex;
            }
            if (weightAccessorIndex != -1)
            {
                attributes.WEIGHTS_0 = weightAccessorIndex;
            }
            if (jointsAccessorIndex != -1)
            {
                attributes.JOINTS_0 = jointsAccessorIndex;
            }

            var gltfMesh = new glTFMesh(mesh.name);
            for (int j = 0; j < mesh.subMeshCount; ++j)
            {
                var indices = TriangleUtil.FlipTriangle(mesh.GetIndices(j)).Select(y => (uint)y).ToArray();
                var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices, glBufferTarget.ELEMENT_ARRAY_BUFFER);

                if (j >= materials.Length)
                {
                    Debug.LogWarningFormat("{0}.materials is not enough", rendererName);
                    break;
                }

                gltfMesh.primitives.Add(new glTFPrimitives
                {
                    attributes = attributes,
                    indices = indicesAccessorIndex,
                    mode = 4, // triangles ?
                    material = unityMaterials.IndexOf(materials[j])
                });
            }
            return gltfMesh;
        }

        static bool UseSparse(
            bool usePosition, Vector3 position,
            bool useNormal, Vector3 normal,
            bool useTangent, Vector3 tangent
            )
        {
            var useSparse =
            (usePosition && position != Vector3.zero)
            || (useNormal && normal != Vector3.zero)
            || (useTangent && tangent != Vector3.zero)
            ;
            return useSparse;
        }

        static gltfMorphTarget ExportMorphTarget(glTF gltf, int bufferIndex,
            Mesh mesh, int j,
            bool useSparseAccessorForMorphTarget,
            bool exportOnlyBlendShapePosition)
        {
            var blendShapeVertices = mesh.vertices;
            var usePosition = blendShapeVertices != null && blendShapeVertices.Length > 0;

            var blendShapeNormals = mesh.normals;
            var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length;
            // var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length && !exportOnlyBlendShapePosition;

            var blendShapeTangents = mesh.tangents.Select(y => (Vector3)y).ToArray();
            //var useTangent = usePosition && blendShapeTangents != null && blendShapeTangents.Length == blendShapeVertices.Length;
            var useTangent = false;

            var frameCount = mesh.GetBlendShapeFrameCount(j);
            mesh.GetBlendShapeFrameVertices(j, frameCount - 1, blendShapeVertices, blendShapeNormals, null);

            var blendShapePositionAccessorIndex = -1;
            var blendShapeNormalAccessorIndex = -1;
            var blendShapeTangentAccessorIndex = -1;
            if (useSparseAccessorForMorphTarget)
            {
                var accessorCount = blendShapeVertices.Length;
                var sparseIndices = Enumerable.Range(0, blendShapeVertices.Length)
                    .Where(x => UseSparse(
                        usePosition, blendShapeVertices[x],
                        useNormal, blendShapeNormals[x],
                        useTangent, blendShapeTangents[x]))
                    .ToArray()
                    ;

                if (sparseIndices.Length == 0)
                {
                    usePosition = false;
                    useNormal = false;
                    useTangent = false;
                }
                else
                {
                    Debug.LogFormat("Sparse {0}/{1}", sparseIndices.Length, mesh.vertexCount);
                }
                /*
                var vertexSize = 12;
                if (useNormal) vertexSize += 12;
                if (useTangent) vertexSize += 24;
                var sparseBytes = (4 + vertexSize) * sparseIndices.Length;
                var fullBytes = (vertexSize) * blendShapeVertices.Length;
                Debug.LogFormat("Export sparse: {0}/{1}bytes({2}%)",
                    sparseBytes, fullBytes, (int)((float)sparseBytes / fullBytes)
                    );
                    */

                var sparseIndicesViewIndex = -1;
                if (usePosition)
                {
                    sparseIndicesViewIndex = gltf.ExtendBufferAndGetViewIndex(bufferIndex, sparseIndices);

                    blendShapeVertices = sparseIndices.Select(x => blendShapeVertices[x].ReverseZ()).ToArray();
                    blendShapePositionAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeVertices,
                        sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useNormal)
                {
                    blendShapeNormals = sparseIndices.Select(x => blendShapeNormals[x].ReverseZ()).ToArray();
                    blendShapeNormalAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeNormals,
                        sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useTangent)
                {
                    blendShapeTangents = sparseIndices.Select(x => blendShapeTangents[x].ReverseZ()).ToArray();
                    blendShapeTangentAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeTangents, sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.ARRAY_BUFFER);
                }
            }
            else
            {
                for (int i = 0; i < blendShapeVertices.Length; ++i) blendShapeVertices[i] = blendShapeVertices[i].ReverseZ();
                if (usePosition)
                {
                    blendShapePositionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeVertices,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useNormal)
                {
                    for (int i = 0; i < blendShapeNormals.Length; ++i) blendShapeNormals[i] = blendShapeNormals[i].ReverseZ();
                    blendShapeNormalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeNormals,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useTangent)
                {
                    for (int i = 0; i < blendShapeTangents.Length; ++i) blendShapeTangents[i] = blendShapeTangents[i].ReverseZ();
                    blendShapeTangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeTangents,
                        glBufferTarget.ARRAY_BUFFER);
                }
            }

            if (blendShapePositionAccessorIndex != -1)
            {
                gltf.accessors[blendShapePositionAccessorIndex].min = blendShapeVertices.Aggregate(blendShapeVertices[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
                gltf.accessors[blendShapePositionAccessorIndex].max = blendShapeVertices.Aggregate(blendShapeVertices[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();
            }

            return new gltfMorphTarget
            {
                POSITION = blendShapePositionAccessorIndex,
                NORMAL = blendShapeNormalAccessorIndex,
                TANGENT = blendShapeTangentAccessorIndex,
            };
        }

        public static void ExportMeshes(glTF gltf, int bufferIndex,
            List<MeshWithRenderer> unityMeshes, List<Material> unityMaterials,
            bool useSparseAccessorForMorphTarget,
            bool exportOnlyBlendShapePosition)
        {
            for (int i = 0; i < unityMeshes.Count; ++i)
            {
                var x = unityMeshes[i];
                var mesh = x.Mesh;
                var materials = x.Renderer.sharedMaterials;

                var gltfMesh = ExportPrimitives(gltf, bufferIndex,
                    x.Renderer.name,
                    mesh, materials, unityMaterials);

                for (int j = 0; j < mesh.blendShapeCount; ++j)
                {
                    var morphTarget = ExportMorphTarget(gltf, bufferIndex,
                        mesh, j,
                        useSparseAccessorForMorphTarget,
                        exportOnlyBlendShapePosition);

                    //
                    // all primitive has same blendShape
                    //
                    for (int k = 0; k < gltfMesh.primitives.Count; ++k)
                    {
                        gltfMesh.primitives[k].targets.Add(morphTarget);
                        gltfMesh.primitives[k].extras.targetNames.Add(mesh.GetBlendShapeName(j));
                    }
                }

                gltf.meshes.Add(gltfMesh);
            }
        }
    }
}
