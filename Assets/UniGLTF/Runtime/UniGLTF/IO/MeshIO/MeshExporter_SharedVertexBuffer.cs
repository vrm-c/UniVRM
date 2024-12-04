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
            AccessorsBounds.UpdatePositionAccessorsBounds(data.Gltf.accessors[positionAccessorIndex], positions);

            var normalAccessorIndex = data.ExtendBufferAndGetAccessorIndex(mesh.normals.Select(y => axisInverter.InvertVector3(y.normalized)).ToArray(), glBufferTarget.ARRAY_BUFFER);

            int? tangentAccessorIndex = default;
            if (settings.ExportTangents)
            {
                tangentAccessorIndex = data.ExtendBufferAndGetAccessorIndex(mesh.tangents.Select(axisInverter.InvertVector4).ToArray(), glBufferTarget.ARRAY_BUFFER);
            }

            var uvAccessorIndex0 = data.ExtendBufferAndGetAccessorIndex(mesh.uv.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);

            var uvAccessorIndex1 = -1;
            if (settings.ExportUvSecondary)
            {
                uvAccessorIndex1 = data.ExtendBufferAndGetAccessorIndex(mesh.uv2.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);
            }

            var colorAccessorIndex = -1;
            var vColorState = VertexColorUtility.DetectVertexColor(mesh, materials);
            if ((settings.ExportVertexColor && mesh.colors != null && mesh.colors.Length == mesh.vertexCount) // vertex color を残す設定
            || vColorState == VertexColorState.ExistsAndIsUsed // VColor使っている
            || vColorState == VertexColorState.ExistsAndMixed // VColorを使っているところと使っていないところが混在(とりあえずExportする)
            )
            {
                // UniUnlit で Multiply 設定になっている
                colorAccessorIndex = data.ExtendBufferAndGetAccessorIndex(mesh.colors, glBufferTarget.ARRAY_BUFFER);
            }

            var boneWeights = mesh.boneWeights;
            var weightAccessorIndex = -1;
            var jointsAccessorIndex = -1;
            if (boneWeights.All(x => x.weight0 == 0 && x.weight1 == 0 && x.weight2 == 0 && x.weight3 == 0))
            {
            }
            else
            {
                weightAccessorIndex = data.ExtendBufferAndGetAccessorIndex(boneWeights.Select(y => new Vector4(y.weight0, y.weight1, y.weight2, y.weight3)).ToArray(), glBufferTarget.ARRAY_BUFFER);
                jointsAccessorIndex = data.ExtendBufferAndGetAccessorIndex(boneWeights.Select(y =>
                    new UShort4(
                        (ushort)unityMesh.GetJointIndex(y.boneIndex0),
                        (ushort)unityMesh.GetJointIndex(y.boneIndex1),
                        (ushort)unityMesh.GetJointIndex(y.boneIndex2),
                        (ushort)unityMesh.GetJointIndex(y.boneIndex3))
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

            var gltfMesh = CreateGLTFMesh(attributes, data, unityMesh, unityMaterials);

            var blendShapeIndexMap = new Dictionary<int, int>();
            {
                var targetNames = new List<string>();

                int exportBlendShapes = 0;
                Vector3[] blendShapePositions = new Vector3[mesh.vertexCount];
                Vector3[] blendShapeNormals = new Vector3[mesh.vertexCount];
                for (int j = 0; j < unityMesh.Mesh.blendShapeCount; ++j)
                {
                    var morphTarget = ExportMorphTarget(data,
                        unityMesh.Mesh, j,
                        blendShapePositions, blendShapeNormals,
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

                gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames, BlendShapeTargetNameLocationFlags.Both);
            }

            return (gltfMesh, blendShapeIndexMap);
        }

        private static glTFMesh CreateGLTFMesh(glTFAttributes attributes, ExportingGltfData data, MeshExportInfo unityMesh, List<Material> unityMaterials)
        {
            var mesh = unityMesh.Mesh;
            var materials = unityMesh.Materials;
            var gltfMesh = new glTFMesh(mesh.name);

            var indices = new List<uint>();
            for (int j = 0; j < mesh.subMeshCount; ++j)
            {
                indices.Clear();
                if (j >= materials.Length)
                {
                    Debug.LogWarningFormat("{0}.materials is not enough", mesh.name);
                    continue;
                }

                var subMesh = mesh.GetSubMesh(j);
                var topologyType = subMesh.topology;
                var materialIndex = unityMaterials.IndexOf(materials[j]);

                var submeshIndices = mesh.GetIndices(j);
                if (submeshIndices.Length == 0)
                {
                    // https://github.com/vrm-c/UniVRM/issues/664                    
                    break;
                }
                else if (submeshIndices.Length < 3)
                {
                    Debug.LogWarningFormat("Invalid primitive of type {0} found", topologyType);
                    continue;
                }

                // Add indices considering the topology type
                switch (topologyType)
                {
                    case MeshTopology.Triangles:
                        if (submeshIndices.Length % 3 != 0)
                            Debug.LogWarningFormat("triangle indices is not multiple of 3");
                        GetTriangleIndices(indices, submeshIndices);
                        break;
                    case MeshTopology.Quads:
                        if (submeshIndices.Length % 4 != 0)
                            Debug.LogWarningFormat("quad indices is not multiple of 4");
                        GetQuadIndices(indices, submeshIndices);
                        break;
                    default:
                    case MeshTopology.Lines:
                    case MeshTopology.LineStrip:
                    case MeshTopology.Points:
                        Debug.LogWarningFormat("Mesh {0} has unsupported topology type {1}.", mesh.name, topologyType);
                        continue;
                }

                var primitive = CreatePrimitives(attributes, data, indices, materialIndex);
                gltfMesh.primitives.Add(primitive);
            }

            return gltfMesh;
        }

        private static glTFPrimitives CreatePrimitives(glTFAttributes attributes, ExportingGltfData data, List<uint> indices, int materialIndex)
        {
            var indicesAccessorIndex = data.ExtendBufferAndGetAccessorIndex(indices.ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
            if (indicesAccessorIndex < 0)
            {
                // https://github.com/vrm-c/UniVRM/issues/664                    
                throw new Exception();
            }
            var primitive = new glTFPrimitives
            {
                attributes = attributes,
                indices = indicesAccessorIndex,
                mode = (int)glTFPrimitives.Mode.TRIANGLES, // triangles ?
                material = materialIndex
            };
            return primitive;
        }

        private static void GetQuadIndices(List<uint> indices, int[] quadIndices)
        {
            for (int i = 0; i < quadIndices.Length - 3; i += 4)
            {
                var i0 = quadIndices[i];
                var i1 = quadIndices[i + 1];
                var i2 = quadIndices[i + 2];
                var i3 = quadIndices[i + 3];

                // flip triangles
                indices.Add((uint)i2);
                indices.Add((uint)i1);
                indices.Add((uint)i0);

                indices.Add((uint)i3);
                indices.Add((uint)i2);
                indices.Add((uint)i0);
            }
        }

        private static void GetTriangleIndices(List<uint> indices, int[] triangleIndices)
        {
            for (int i = 0; i < triangleIndices.Length - 2; i += 3)
            {
                var i0 = triangleIndices[i];
                var i1 = triangleIndices[i + 1];
                var i2 = triangleIndices[i + 2];

                // flip triangle
                indices.Add((uint)i2);
                indices.Add((uint)i1);
                indices.Add((uint)i0);
            }
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
            Vector3[] blendShapeVertices, Vector3[] blendShapeNormals,
            bool useSparseAccessorForMorphTarget,
            bool exportOnlyBlendShapePosition,
            IAxisInverter axisInverter)
        {
            var usePosition = blendShapeVertices != null && blendShapeVertices.Length > 0;

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

            return BlendShapeExporter.Export(data,
                blendShapeVertices,
                exportOnlyBlendShapePosition && useNormal ? null : blendShapeNormals,
                useSparseAccessorForMorphTarget);
        }
    }
}
