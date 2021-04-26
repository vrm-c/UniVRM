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
            readonly List<Vector3> m_positions;
            readonly List<Vector3> m_normals;

            public BlendShapeBuffer(int reserve)
            {
                m_positions = new List<Vector3>(reserve);
                m_normals = new List<Vector3>(reserve);
            }

            public void Push(Vector3 position, Vector3 normal)
            {
                m_positions.Add(position);
                m_normals.Add(normal);
            }

            public gltfMorphTarget ToGltf(glTF gltf, int bufferIndex, bool useNormal)
            {
                var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_positions.ToArray(), glBufferTarget.ARRAY_BUFFER);
                var normalAccessorIndex = -1;
                if (useNormal)
                {
                    normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_normals.ToArray(), glBufferTarget.ARRAY_BUFFER);
                }
                return new gltfMorphTarget
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                };
            }
        }

        public class VertexBuffer
        {
            readonly List<Vector3> m_positions;
            readonly List<Vector3> m_normals;
            readonly List<Vector2> m_uv;

            readonly Func<int, int> m_getJointIndex;
            readonly List<UShort4> m_joints;
            readonly List<Vector4> m_weights;

            public VertexBuffer(int reserve, Func<int, int> getJointIndex)
            {
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
                m_positions.Add(position);
                m_normals.Add(normal);
                m_uv.Add(uv);
            }

            public void Push(BoneWeight boneWeight)
            {
                m_joints.Add(new UShort4((ushort)boneWeight.boneIndex0, (ushort)boneWeight.boneIndex1, (ushort)boneWeight.boneIndex2, (ushort)boneWeight.boneIndex3));
                m_weights.Add(new Vector4(boneWeight.weight0, boneWeight.weight1, boneWeight.weight2, boneWeight.weight3));
            }

            public glTFPrimitives ToGltf(glTF gltf, int bufferIndex, int materialIndex, IReadOnlyList<uint> indices)
            {
                var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices.ToArray(), glBufferTarget.ELEMENT_ARRAY_BUFFER);
                var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_positions.ToArray(), glBufferTarget.ARRAY_BUFFER);
                var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, m_normals.ToArray(), glBufferTarget.ARRAY_BUFFER);
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
