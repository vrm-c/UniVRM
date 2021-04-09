using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    [Serializable]
    public struct MeshExportSettings
    {
        //
        // https://github.com/vrm-c/UniVRM/issues/800
        //
        // VertexBuffer を共有バッファ方式にする
        // UniVRM-0.71.0 までの挙動
        //
        public bool UseSharingVertexBuffer;

        // MorphTarget に Sparse Accessor を使う
        public bool UseSparseAccessorForMorphTarget;

        // MorphTarget を Position だけにする(normal とか捨てる)
        public bool ExportOnlyBlendShapePosition;

        // tangent を出力する
        public bool ExportTangents;

        public static MeshExportSettings Default => new MeshExportSettings
        {
            UseSparseAccessorForMorphTarget = false,
            ExportOnlyBlendShapePosition = false,
#if GLTF_EXPORT_TANGENTS
            ExportTangents = true,
#endif            
        };
    }

    class MorphTarget
    {
        readonly string m_name;
        readonly List<Vector3> m_positions;
        readonly List<Vector3> m_normals;

        public MorphTarget(string name, int reserve)
        {
            m_name = name;
            m_positions = new List<Vector3>(reserve);
            m_normals = new List<Vector3>(reserve);
        }
    }


    class VertexBuffer
    {
        readonly IAxisInverter m_inverter;
        readonly int m_indexCount;
        readonly List<Vector3> m_positions;
        readonly List<Vector3> m_normals;
        readonly List<Vector2> m_uv;

        readonly Func<int, int> m_getJointIndex;
        readonly List<UShort4> m_joints;
        readonly List<Vector4> m_weights;

        readonly List<MorphTarget> m_morphs = new List<MorphTarget>();


        public VertexBuffer(IAxisInverter inverter, int reserve, Func<int, int> getJointIndex)
        {
            m_inverter = inverter;
            m_indexCount = reserve;
            m_positions = new List<Vector3>(reserve);
            m_normals = new List<Vector3>(reserve);
            m_uv = new List<Vector2>();

            m_getJointIndex = getJointIndex;
            if (m_getJointIndex != null)
            {
                m_joints = new List<UShort4>(reserve);
                m_weights = new List<Vector4>(reserve);
            }
        }

        public void Push(Vector3 position, Vector3 normal, Vector2 uv)
        {
            m_positions.Add(m_inverter.InvertVector3(position));
            m_normals.Add(m_inverter.InvertVector3(normal));
            m_uv.Add(uv.ReverseUV());
        }

        public void Push(BoneWeight boneWeight)
        {
            m_joints.Add(new UShort4((ushort)boneWeight.boneIndex0, (ushort)boneWeight.boneIndex1, (ushort)boneWeight.boneIndex2, (ushort)boneWeight.boneIndex3));
            m_weights.Add(new Vector4(boneWeight.weight0, boneWeight.weight1, boneWeight.weight2, boneWeight.weight3));
        }

        public glTFPrimitives ToGltf(glTF gltf, int bufferIndex, int materialIndex)
        {
            var indices = new List<uint>(m_indexCount);
            // flip triangles
            for (int i = 0; i < m_indexCount; i += 3)
            {
                // 2, 1, 0, 5, 4, 3, ...
                indices.Add((uint)(i + 2));
                indices.Add((uint)(i + 1));
                indices.Add((uint)(i + 0));
            }

            var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices.ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
            var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_positions.ToArray(), glBufferTarget.ARRAY_BUFFER);
            var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_normals.ToArray(), glBufferTarget.ARRAY_BUFFER);
            var uvAccessorIndex0 = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_uv.ToArray(), glBufferTarget.ARRAY_BUFFER);
            var jointsAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_joints.ToArray(), glBufferTarget.ARRAY_BUFFER);
            var weightAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_weights.ToArray(), glBufferTarget.ARRAY_BUFFER);

            var primitive = new glTFPrimitives
            {
                indices = indicesAccessorIndex,
                attributes = new glTFAttributes
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                    TEXCOORD_0 = uvAccessorIndex0,
                    JOINTS_0 = jointsAccessorIndex,
                    WEIGHTS_0 = weightAccessorIndex,
                },
                material = materialIndex,
                mode = 4,
                targets = null,
            };
            return primitive;
        }
    }

    public static class MeshExporter
    {
        static glTFMesh ExportDividedVertexBuffer(glTF gltf, int bufferIndex,
            MeshWithRenderer unityMesh, List<Material> unityMaterials,
            IAxisInverter axisInverter, MeshExportSettings settings)
        {
            var mesh = unityMesh.Mesh;
            var gltfMesh = new glTFMesh(mesh.name);

            if (settings.ExportTangents)
            {
                // support しない
                throw new NotImplementedException();
            }

            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uv = mesh.uv;
            var boneWeights = mesh.boneWeights;

            Func<int, int> getJointIndex = null;
            if (boneWeights != null && boneWeights.Length == positions.Length)
            {
                getJointIndex = unityMesh.GetJointIndex;
            }

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var indices = mesh.GetIndices(i);

                // index の順に attributes を蓄える                
                var buffer = new VertexBuffer(axisInverter, indices.Length, getJointIndex);
                foreach (var j in indices)
                {
                    buffer.Push(positions[j], normals[j], uv[j]);
                    if (getJointIndex != null)
                    {
                        buffer.Push(boneWeights[j]);
                    }
                }

                var material = unityMesh.Renderer.sharedMaterials[i];
                var materialIndex = -1;
                if (material != null)
                {
                    materialIndex = unityMaterials.IndexOf(material);
                }
                gltfMesh.primitives.Add(buffer.ToGltf(gltf, bufferIndex, materialIndex));
            }

            // morphTarget

            return gltfMesh;
        }

        /// <summary>
        /// primitive 間で vertex を共有する形で Export する。
        /// 
        /// UniVRM-0.71.0 までの挙動
        ///
        /// /// </summary>
        /// <param name="gltf"></param>
        /// <param name="bufferIndex"></param>
        /// <param name="unityMesh"></param>
        /// <param name="unityMaterials"></param>
        /// <param name="axisInverter"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        static glTFMesh ExportSharedVertexBuffer(glTF gltf, int bufferIndex,
            MeshWithRenderer unityMesh, List<Material> unityMaterials,
            IAxisInverter axisInverter, MeshExportSettings settings)
        {
            var mesh = unityMesh.Mesh;
            var materials = unityMesh.Renderer.sharedMaterials;
            var positions = mesh.vertices.Select(axisInverter.InvertVector3).ToArray();
            var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, positions, glBufferTarget.ARRAY_BUFFER);
            gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
            gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

            var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.normals.Select(y => axisInverter.InvertVector3(y.normalized)).ToArray(), glBufferTarget.ARRAY_BUFFER);

            int? tangentAccessorIndex = default;
            if (settings.ExportTangents)
            {
                tangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.tangents.Select(axisInverter.InvertVector4).ToArray(), glBufferTarget.ARRAY_BUFFER);
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="gltf"></param>
        /// <param name="bufferIndex"></param>
        /// <param name="unityMesh"></param>
        /// <param name="unityMaterials"></param>
        /// <param name="settings"></param>
        /// <param name="axisInverter"></param>
        /// <returns></returns>
        public static (glTFMesh mesh, Dictionary<int, int> blendShapeIndexMap) ExportMesh(glTF gltf, int bufferIndex,
            MeshWithRenderer unityMesh, List<Material> unityMaterials,
            MeshExportSettings settings, IAxisInverter axisInverter)
        {
            glTFMesh gltfMesh = default;
            var blendShapeIndexMap = new Dictionary<int, int>();
            if (settings.UseSharingVertexBuffer)
            {
                gltfMesh = ExportSharedVertexBuffer(gltf, bufferIndex, unityMesh, unityMaterials, axisInverter, settings);

                var targetNames = new List<string>();

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
            }
            else
            {
                gltfMesh = ExportDividedVertexBuffer(gltf, bufferIndex, unityMesh, unityMaterials, axisInverter, settings);
            }

            return (gltfMesh, blendShapeIndexMap);
        }
    }
}
