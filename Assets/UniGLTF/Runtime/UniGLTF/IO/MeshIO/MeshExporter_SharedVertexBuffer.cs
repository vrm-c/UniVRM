using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    public static class MeshExporter_SharedVertexBuffer
    {
        /// <summary>
        /// primitive 間で vertex を共有する形で Export する。
        /// UniVRM-0.71.0 以降は、MeshExporterDivided.Export もある。
        /// 
        /// * GLB/GLTF は shared(default) と divided を選択可能
        /// * VRM0 は shared 仕様
        /// * VRM1 は divided 仕様
        /// 
        /// /// </summary>
        /// <param name="gltf"></param>
        /// <param name="bufferIndex"></param>
        /// <param name="unityMesh"></param>
        /// <param name="unityMaterials"></param>
        /// <param name="axisInverter"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static (glTFMesh, Dictionary<int, int> blendShapeIndexMap) Export(ExportingGltfData data,
            MeshExportInfo unityMesh, List<Material> unityMaterials,
            IAxisInverter axisInverter, GltfExportSettings settings)
        {
            var mesh = unityMesh.Mesh;
            var materials = unityMesh.Materials;
            var positions = mesh.vertices.Select(axisInverter.InvertVector3).ToArray();
            var positionAccessorIndex = data.ExtendBufferAndGetAccessorIndex(positions, glBufferTarget.ARRAY_BUFFER);
            data.GLTF.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
            data.GLTF.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

            var normalAccessorIndex = data.ExtendBufferAndGetAccessorIndex(mesh.normals.Select(y => axisInverter.InvertVector3(y.normalized)).ToArray(), glBufferTarget.ARRAY_BUFFER);

            int? tangentAccessorIndex = default;
            if (settings.ExportTangents)
            {
                tangentAccessorIndex = data.ExtendBufferAndGetAccessorIndex(mesh.tangents.Select(axisInverter.InvertVector4).ToArray(), glBufferTarget.ARRAY_BUFFER);
            }

            var uvAccessorIndex0 = data.ExtendBufferAndGetAccessorIndex(mesh.uv.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var uvAccessorIndex1 = data.ExtendBufferAndGetAccessorIndex(mesh.uv2.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);

            var colorAccessorIndex = -1;

            var vColorState = VertexColorUtility.DetectVertexColor(mesh, materials);
            if (vColorState == VertexColorState.ExistsAndIsUsed // VColor使っている
            || vColorState == VertexColorState.ExistsAndMixed // VColorを使っているところと使っていないところが混在(とりあえずExportする)
            )
            {
                // UniUnlit で Multiply 設定になっている
                colorAccessorIndex = data.ExtendBufferAndGetAccessorIndex(mesh.colors, glBufferTarget.ARRAY_BUFFER);
            }

            var boneweights = mesh.boneWeights;
            var weightAccessorIndex = data.ExtendBufferAndGetAccessorIndex(boneweights.Select(y => new Vector4(y.weight0, y.weight1, y.weight2, y.weight3)).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var jointsAccessorIndex = data.ExtendBufferAndGetAccessorIndex(boneweights.Select(y =>
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

            if (tangentAccessorIndex.HasValue)
            {
                attributes.TANGENT = tangentAccessorIndex.Value;
            }

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

                var indicesAccessorIndex = data.ExtendBufferAndGetAccessorIndex(indices.ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
                if (indicesAccessorIndex < 0)
                {
                    // https://github.com/vrm-c/UniVRM/issues/664                    
                    throw new Exception();
                }

                if (j >= materials.Length)
                {
                    Debug.LogWarningFormat("{0}.materials is not enough", unityMesh.Mesh.name);
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

            var blendShapeIndexMap = new Dictionary<int, int>();
            {
                var targetNames = new List<string>();

                int exportBlendShapes = 0;
                for (int j = 0; j < unityMesh.Mesh.blendShapeCount; ++j)
                {
                    var morphTarget = ExportMorphTarget(data,
                        unityMesh.Mesh, j,
                        settings.UseSparseAccessorForMorphTarget,
                        settings.ExportOnlyBlendShapePosition, axisInverter);
                    if (morphTarget.POSITION < 0)
                    {
                        // Skip empty blendShape.
                        // Shift blendShape's index.
                        continue;
                    }

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
            }

            return (gltfMesh, blendShapeIndexMap);
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

        static gltfMorphTarget ExportMorphTarget(ExportingGltfData data,
            Mesh mesh, int blendShapeIndex,
            bool useSparseAccessorForMorphTarget,
            bool exportOnlyBlendShapePosition,
            IAxisInverter axisInverter)
        {
            var blendShapeVertices = mesh.vertices;
            var usePosition = blendShapeVertices != null && blendShapeVertices.Length > 0;

            var blendShapeNormals = mesh.normals;
            var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length;
            // var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length && !exportOnlyBlendShapePosition;

            // var blendShapeTangents = mesh.tangents.Select(y => (Vector3)y).ToArray();
            // //var useTangent = usePosition && blendShapeTangents != null && blendShapeTangents.Length == blendShapeVertices.Length;
            // var useTangent = false;

            var frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
            mesh.GetBlendShapeFrameVertices(blendShapeIndex, frameCount - 1, blendShapeVertices, blendShapeNormals, null);

            //
            // invert axis
            //
            for (int i = 0; i < blendShapeVertices.Length; ++i)
            {
                blendShapeVertices[i] = axisInverter.InvertVector3(blendShapeVertices[i]);
            }
            for (int i = 0; i < blendShapeNormals.Length; ++i)
            {
                blendShapeNormals[i] = axisInverter.InvertVector3(blendShapeNormals[i]);
            }

            var positions = mesh.vertices;
            for (int i = 0; i < positions.Length; ++i)
            {
                positions[i] = axisInverter.InvertVector3(positions[i]);
            }

            var normals = mesh.normals;
            for (int i = 0; i < normals.Length; ++i)
            {
                normals[i] = axisInverter.InvertVector3(normals[i]);
            }

            return BlendShapeExporter.Export(data,
                blendShapeVertices,
                exportOnlyBlendShapePosition && useNormal ? null : blendShapeNormals,
                useSparseAccessorForMorphTarget);
        }
    }
}
