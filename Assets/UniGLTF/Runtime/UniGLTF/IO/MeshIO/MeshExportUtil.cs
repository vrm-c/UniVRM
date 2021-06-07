using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    /// <summary>
    /// https://github.com/vrm-c/UniVRM/issues/800
    /// 
    /// 頂点バッファ分割の補助クラス。
    /// </summary>
    public static class MeshExportUtil
    {
        public class BlendShapeBuffer
        {
            readonly Vector3[] m_positions;
            readonly Vector3[] m_normals;

            public BlendShapeBuffer(int reserve)
            {
                m_positions = new Vector3[reserve];
                m_normals = new Vector3[reserve];
            }

            public void Set(int index, Vector3 position, Vector3 normal)
            {
                m_positions[index] = position;
                m_normals[index] = normal;
            }

            public gltfMorphTarget ToGltf(glTF gltf, int gltfBuffer, bool useNormal, bool useSparse)
            {
                return BlendShapeExporter.Export(gltf, gltfBuffer,
                    m_positions,
                    useNormal ? m_normals : null,
                    useSparse);
            }
        }

        public class VertexBuffer
        {
            /// <summary>
            /// SubMeshで分割するので index が変わる。対応表
            /// </summary>
            /// <typeparam name="int"></typeparam>
            /// <typeparam name="int"></typeparam>
            /// <returns></returns>
            readonly Dictionary<int, int> m_vertexIndexMap = new Dictionary<int, int>();
            public bool ContainsTriangle(int v0, int v1, int v2)
            {
                if (!m_vertexIndexMap.ContainsKey(v0))
                {
                    return false;
                }
                if (!m_vertexIndexMap.ContainsKey(v1))
                {
                    return false;
                }
                if (!m_vertexIndexMap.ContainsKey(v2))
                {
                    return false;
                }
                return true;
            }

            readonly List<Vector3> m_positions;
            readonly List<Vector3> m_normals;
            readonly List<Vector2> m_uv;

            readonly Func<int, int> m_getJointIndex;
            readonly List<UShort4> m_joints;
            readonly List<Vector4> m_weights;

            public VertexBuffer(int vertexCount, Func<int, int> getJointIndex)
            {
                m_positions = new List<Vector3>(vertexCount);
                m_normals = new List<Vector3>(vertexCount);
                m_uv = new List<Vector2>();

                m_getJointIndex = getJointIndex;
                if (m_getJointIndex != null)
                {
                    m_joints = new List<UShort4>(vertexCount);
                    m_weights = new List<Vector4>(vertexCount);
                }
            }

            public void Push(int index, Vector3 position, Vector3 normal, Vector2 uv)
            {
                var newIndex = m_positions.Count;
                m_vertexIndexMap.Add(index, newIndex);

                m_positions.Add(position);
                m_normals.Add(normal);
                m_uv.Add(uv);
            }

            public void Push(BoneWeight boneWeight)
            {
                m_joints.Add(new UShort4((ushort)boneWeight.boneIndex0, (ushort)boneWeight.boneIndex1, (ushort)boneWeight.boneIndex2, (ushort)boneWeight.boneIndex3));
                m_weights.Add(new Vector4(boneWeight.weight0, boneWeight.weight1, boneWeight.weight2, boneWeight.weight3));
            }

            public glTFPrimitives ToGltfPrimitive(glTF gltf, int bufferIndex, int materialIndex, IEnumerable<int> indices)
            {
                var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices.Select(x => (uint)m_vertexIndexMap[x]).ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
                var positions = m_positions.ToArray();
                var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, positions, glBufferTarget.ARRAY_BUFFER);
                var normals = m_normals.ToArray();
                var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, normals, glBufferTarget.ARRAY_BUFFER);
                var uvAccessorIndex0 = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_uv.ToArray(), glBufferTarget.ARRAY_BUFFER);

                int? jointsAccessorIndex = default;
                if (m_joints != null)
                {
                    jointsAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_joints.ToArray(), glBufferTarget.ARRAY_BUFFER);
                }
                int? weightAccessorIndex = default;
                if (m_weights != null)
                {
                    weightAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_weights.ToArray(), glBufferTarget.ARRAY_BUFFER);
                }

                var primitive = new glTFPrimitives
                {
                    indices = indicesAccessorIndex,
                    attributes = new glTFAttributes
                    {
                        POSITION = positionAccessorIndex,
                        NORMAL = normalAccessorIndex,
                        TEXCOORD_0 = uvAccessorIndex0,
                        JOINTS_0 = jointsAccessorIndex.GetValueOrDefault(-1),
                        WEIGHTS_0 = weightAccessorIndex.GetValueOrDefault(-1),
                    },
                    material = materialIndex,
                    mode = 4,
                };

                return primitive;
            }
        }
    }
}
