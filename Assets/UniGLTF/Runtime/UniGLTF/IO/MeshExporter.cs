using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    [Serializable]
    public struct MeshExportSettings
    {
        // MorphTarget に Sparse Accessor を使う
        public bool UseSparseAccessorForMorphTarget;

        // MorphTarget を Position だけにする(normal とか捨てる)
        public bool ExportOnlyBlendShapePosition;

        public static MeshExportSettings Default => new MeshExportSettings
        {
            UseSparseAccessorForMorphTarget = false,
            ExportOnlyBlendShapePosition = false,
        };
    }

    public static class MeshExporter
    {
        static glTFMesh ExportPrimitives(glTF gltf, int bufferIndex, Mesh mesh, Material[] meshMaterials, BoneWeight[] boneWeights, int[] jointIndexMap,
            List<Material> exportedMaterials)
        {
            var positions = mesh.vertices.Select(y => y.ReverseZ()).ToArray();
            var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, positions, glBufferTarget.ARRAY_BUFFER);
            gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
            gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

            var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.normals.Select(y => y.normalized.ReverseZ()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#if GLTF_EXPORT_TANGENTS
            var tangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.tangents.Select(y => y.ReverseZ()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#endif
            var uvAccessorIndex0 = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.uv.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var uvAccessorIndex1 = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.uv2.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);

            var colorAccessorIndex = -1;

            var vColorState = MeshExportInfo.DetectVertexColor(mesh, meshMaterials);
            if (vColorState == MeshExportInfo.VertexColorState.ExistsAndIsUsed // VColor使っている
            || vColorState == MeshExportInfo.VertexColorState.ExistsAndMixed // VColorを使っているところと使っていないところが混在(とりあえずExportする)
            )
            {
                // UniUnlit で Multiply 設定になっている
                colorAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.colors, glBufferTarget.ARRAY_BUFFER);
            }

            var weightAccessorIndex = -1;
            var jointsAccessorIndex = -1;

            if (boneWeights != null && jointIndexMap != null)
            {
                weightAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneWeights.Select(y => new Vector4(y.weight0, y.weight1, y.weight2, y.weight3)).ToArray(), glBufferTarget.ARRAY_BUFFER);
                jointsAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneWeights.Select(y =>
                    new UShort4(
                        (ushort)jointIndexMap[y.boneIndex0],
                        (ushort)jointIndexMap[y.boneIndex1],
                        (ushort)jointIndexMap[y.boneIndex2],
                        (ushort)jointIndexMap[y.boneIndex3])
                ).ToArray(), glBufferTarget.ARRAY_BUFFER);
            }

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
            if (uvAccessorIndex0 != -1)
            {
                attributes.TEXCOORD_0 = uvAccessorIndex0;
            }
            if (uvAccessorIndex1 != -1)
            {
                attributes.TEXCOORD_1 = uvAccessorIndex1;
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
            var indices = new List<uint>();
            for (var j = 0; j < mesh.subMeshCount; ++j)
            {
                indices.Clear();

                var triangles = mesh.GetIndices(j);
                if (triangles.Length == 0)
                {
                    // https://github.com/vrm-c/UniVRM/issues/664                    
                    continue;
                }

                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var i0 = triangles[i];
                    var i1 = triangles[i + 1];
                    var i2 = triangles[i + 2];

                    // flip triangle
                    indices.Add((uint)i2);
                    indices.Add((uint)i1);
                    indices.Add((uint)i0);
                }

                var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices.ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
                if (indicesAccessorIndex < 0)
                {
                    // https://github.com/vrm-c/UniVRM/issues/664                    
                    throw new Exception();
                }

                var primitives = new glTFPrimitives
                {
                    attributes = attributes,
                    indices = indicesAccessorIndex,
                    mode = 4, // triangles ?
                };

                if (meshMaterials != null)
                {
                    primitives.material = exportedMaterials.IndexOf(meshMaterials[j]);
                }

                gltfMesh.primitives.Add(primitives);
                
            }
            return gltfMesh;
        }

        static glTFMesh ExportPrimitives(glTF gltf, int bufferIndex,
            MeshWithRenderer unityMesh, List<Material> unityMaterials,
            IAxisInverter axisInverter)
        {
            var mesh = unityMesh.Mesh;
            var materials = unityMesh.Renderer.sharedMaterials;
            var positions = mesh.vertices.Select(axisInverter.InvertVector3).ToArray();
            var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, positions, glBufferTarget.ARRAY_BUFFER);
            gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
            gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

            var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.normals.Select(y => axisInverter.InvertVector3(y.normalized)).ToArray(), glBufferTarget.ARRAY_BUFFER);
#if GLTF_EXPORT_TANGENTS
            var tangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.tangents.Select(axisInverter.InvertVector4()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#endif
            var uvAccessorIndex0 = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.uv.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var uvAccessorIndex1 = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.uv2.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);

            var colorAccessorIndex = -1;

            var vColorState = MeshExportInfo.DetectVertexColor(mesh, materials);
            if (vColorState == MeshExportInfo.VertexColorState.ExistsAndIsUsed // VColor使っている
            || vColorState == MeshExportInfo.VertexColorState.ExistsAndMixed // VColorを使っているところと使っていないところが混在(とりあえずExportする)
            )
            {
                // UniUnlit で Multiply 設定になっている
                colorAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.colors, glBufferTarget.ARRAY_BUFFER);
            }

            var boneweights = mesh.boneWeights;
            var weightAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneweights.Select(y => new Vector4(y.weight0, y.weight1, y.weight2, y.weight3)).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var jointsAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneweights.Select(y =>
                new UShort4(
                    (ushort)unityMesh.GetJointIndex(y.boneIndex0),
                    (ushort)unityMesh.GetJointIndex(y.boneIndex1),
                    (ushort)unityMesh.GetJointIndex(y.boneIndex2),
                    (ushort)unityMesh.GetJointIndex(y.boneIndex3))
                ).ToArray(), glBufferTarget.ARRAY_BUFFER);

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
            if (uvAccessorIndex0 != -1)
            {
                attributes.TEXCOORD_0 = uvAccessorIndex0;
            }
            if (uvAccessorIndex1 != -1)
            {
                attributes.TEXCOORD_1 = uvAccessorIndex1;
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
            var indices = new List<uint>();
            for (int j = 0; j < mesh.subMeshCount; ++j)
            {
                indices.Clear();

                var triangles = mesh.GetIndices(j);
                if (triangles.Length == 0)
                {
                    // https://github.com/vrm-c/UniVRM/issues/664                    
                    continue;
                }

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    var i0 = triangles[i];
                    var i1 = triangles[i + 1];
                    var i2 = triangles[i + 2];

                    // flip triangle
                    indices.Add((uint)i2);
                    indices.Add((uint)i1);
                    indices.Add((uint)i0);
                }

                var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices.ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
                if (indicesAccessorIndex < 0)
                {
                    // https://github.com/vrm-c/UniVRM/issues/664                    
                    throw new Exception();
                }

                if (j >= materials.Length)
                {
                    Debug.LogWarningFormat("{0}.materials is not enough", unityMesh.Renderer.name);
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
            bool exportOnlyBlendShapePosition,
            IAxisInverter axisInverter)
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

                    blendShapeVertices = sparseIndices.Select(x => axisInverter.InvertVector3(blendShapeVertices[x])).ToArray();
                    blendShapePositionAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeVertices,
                        sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.NONE);
                }

                if (useNormal)
                {
                    blendShapeNormals = sparseIndices.Select(x => axisInverter.InvertVector3(blendShapeNormals[x])).ToArray();
                    blendShapeNormalAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeNormals,
                        sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.NONE);
                }

                if (useTangent)
                {
                    blendShapeTangents = sparseIndices.Select(x => axisInverter.InvertVector3(blendShapeTangents[x])).ToArray();
                    blendShapeTangentAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeTangents, sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.NONE);
                }
            }
            else
            {
                for (int i = 0; i < blendShapeVertices.Length; ++i) blendShapeVertices[i] = axisInverter.InvertVector3(blendShapeVertices[i]);
                if (usePosition)
                {
                    blendShapePositionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeVertices,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useNormal)
                {
                    for (int i = 0; i < blendShapeNormals.Length; ++i) blendShapeNormals[i] = axisInverter.InvertVector3(blendShapeNormals[i]);
                    blendShapeNormalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeNormals,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useTangent)
                {
                    for (int i = 0; i < blendShapeTangents.Length; ++i) blendShapeTangents[i] = axisInverter.InvertVector3(blendShapeTangents[i]);
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

        public static IEnumerable<(Mesh, glTFMesh, Dictionary<int, int>)> ExportMeshes(glTF gltf, int bufferIndex,
            List<MeshWithRenderer> unityMeshes, List<Material> unityMaterials,
            MeshExportSettings settings, IAxisInverter axisInverter)
        {
            foreach (var unityMesh in unityMeshes)
            {
                var gltfMesh = ExportPrimitives(gltf, bufferIndex,
                    unityMesh, unityMaterials, axisInverter);

                var targetNames = new List<string>();

                var blendShapeIndexMap = new Dictionary<int, int>();
                int exportBlendShapes = 0;
                for (int j = 0; j < unityMesh.Mesh.blendShapeCount; ++j)
                {
                    var morphTarget = ExportMorphTarget(gltf, bufferIndex,
                        unityMesh.Mesh, j,
                        settings.UseSparseAccessorForMorphTarget,
                        settings.ExportOnlyBlendShapePosition, axisInverter);
                    if (morphTarget.POSITION < 0 && morphTarget.NORMAL < 0 && morphTarget.TANGENT < 0)
                    {
                        continue;
                    }

                    // maybe skip
                    var blendShapeName = unityMesh.Mesh.GetBlendShapeName(j);
                    blendShapeIndexMap.Add(j, exportBlendShapes++);
                    targetNames.Add(blendShapeName);

                    //
                    // all primitive has same blendShape
                    //
                    for (int k = 0; k < gltfMesh.primitives.Count; ++k)
                    {
                        gltfMesh.primitives[k].targets.Add(morphTarget);
                    }
                }

                gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames);

                yield return (unityMesh.Mesh, gltfMesh, blendShapeIndexMap);
            }
        }

        public static (glTFMesh gltfMesh, Dictionary<int, int> blendShapeIndexMap) ExportMesh(glTF gltf, int bufferIndex, Mesh mesh, Renderer renderer, List<Material> exportedMaterials, MeshExportSettings meshExportSettings, IAxisInverter axisInverter)
        {
            var meshMaterials = default(Material[]);
            if (renderer != null)
            {
                meshMaterials = renderer.sharedMaterials;
            }

            var boneWeights = default(BoneWeight[]);
            var jointIndexMap = default(int[]);
            if (renderer is SkinnedMeshRenderer skin)
            {
                var bones = skin.bones;
                var uniqueBones = bones.Distinct().ToArray();
                jointIndexMap = new int[bones.Length];
                for (var i = 0; i < bones.Length; i++)
                {
                    jointIndexMap[i] = Array.IndexOf(uniqueBones, bones[i]);
                }

                boneWeights = mesh.boneWeights;
            }

            var gltfMesh = ExportPrimitives(gltf, bufferIndex, mesh, meshMaterials, boneWeights, jointIndexMap, exportedMaterials);

            var targetNames = new List<string>();

            var blendShapeIndexMap = new Dictionary<int, int>();
            int exportBlendShapes = 0;
            for (int j = 0; j < mesh.blendShapeCount; ++j)
            {
                var morphTarget = ExportMorphTarget(gltf, bufferIndex,
                    mesh, j,
                    meshExportSettings.UseSparseAccessorForMorphTarget,
                    meshExportSettings.ExportOnlyBlendShapePosition, axisInverter);
                if (morphTarget.POSITION < 0 && morphTarget.NORMAL < 0 && morphTarget.TANGENT < 0)
                {
                    continue;
                }

                // maybe skip
                var blendShapeName = mesh.GetBlendShapeName(j);
                blendShapeIndexMap.Add(j, exportBlendShapes++);
                targetNames.Add(blendShapeName);

                //
                // all primitive has same blendShape
                //
                for (int k = 0; k < gltfMesh.primitives.Count; ++k)
                {
                    gltfMesh.primitives[k].targets.Add(morphTarget);
                }
            }

            gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames);

            return (gltfMesh, blendShapeIndexMap);
        }
    }
}
