using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 座標系を変換した Model により、Mesh, Node, BindMatrices を更新する。
    /// buffer, bufferAccessor の更新もある。
    /// </summary>
    class MeshUpdater
    {
        GltfData _data;
        ArrayByteBuffer _buffer;
        List<glTFBufferView> _bufferViews = new List<glTFBufferView>();
        List<glTFAccessor> _accessors = new List<glTFAccessor>();

        public MeshUpdater(GltfData data)
        {
            _data = data;
            _buffer = new ArrayByteBuffer(new byte[data.Bin.Count]);
        }

        int AddBuffer(ArraySegment<byte> bytes)
        {
            var bufferView = _buffer.Extend(bytes);
            var index = _bufferViews.Count;
            _bufferViews.Add(bufferView);
            return index;
        }

        int AddAccessor<T>(SpanLike<T> span) where T : struct
        {
            var bufferViewIndex = AddBuffer(span.Bytes);
            var accessor = new glTFAccessor
            {
                bufferView = bufferViewIndex,
                count = span.Length,
                byteOffset = 0,
                componentType = glTFExtensions.GetComponentType<T>(),
                type = glTFExtensions.GetAccessorType<T>(),
            };
            var index = _accessors.Count;
            _accessors.Add(accessor);
            return index;
        }

        int? AddAccessor<T>(VrmLib.BufferAccessor buffer) where T : struct
        {
            if (buffer == null)
            {
                return default;
            }
            return AddAccessor(buffer.GetSpan<T>());
        }

        struct MorphAccessor
        {
            public int? Position;
            public int? Normal;
        };

        public (glTF, ArraySegment<byte>) Update(VrmLib.Model model)
        {
            var gltf = _data.GLTF;

            // copy images
            foreach (var image in gltf.images)
            {
                var bytes = _data.GetBytesFromBufferView(image.bufferView);
                image.bufferView = AddBuffer(bytes);
            }

            // update Mesh
            foreach (var (gltfMesh, mesh) in Enumerable.Zip(gltf.meshes, model.MeshGroups, (l, r) => (l, r.Meshes[0])))
            {
                SpanLike<uint> indices;
                switch (mesh.IndexBuffer.Stride)
                {
                    case 1:
                        {
                            // byte
                            var byte_indices = mesh.IndexBuffer.GetSpan<byte>();
                            indices = SpanLike.Create<uint>(byte_indices.Length);
                            for (int i = 0; i < byte_indices.Length; ++i)
                            {
                                indices[i] = byte_indices[i];
                            }
                            break;
                        }

                    case 2:
                        {
                            // ushort
                            var ushort_indices = mesh.IndexBuffer.GetSpan<ushort>();
                            indices = SpanLike.Create<uint>(ushort_indices.Length);
                            for (int i = 0; i < ushort_indices.Length; ++i)
                            {
                                indices[i] = ushort_indices[i];
                            }
                            break;
                        }

                    case 4:
                        {
                            // uint
                            indices = mesh.IndexBuffer.GetSpan<uint>();
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
                var position = AddAccessor<Vector3>(mesh.VertexBuffer.Positions);
                var normal = AddAccessor<Vector3>(mesh.VertexBuffer.Normals);
                var uv = AddAccessor<Vector2>(mesh.VertexBuffer.TexCoords);
                var weights = AddAccessor<Vector4>(mesh.VertexBuffer.Weights);
                var joints = AddAccessor<UShort4>(mesh.VertexBuffer.Joints);
                var color = AddAccessor<Vector4>(mesh.VertexBuffer.Colors);

                var morphTargets = new MorphAccessor[] { };
                if (mesh.MorphTargets != null)
                {
                    morphTargets = mesh.MorphTargets.Select(x => new MorphAccessor
                    {
                        Position = AddAccessor<Vector3>(x.VertexBuffer.Positions),
                        Normal = AddAccessor<Vector3>(x.VertexBuffer.Normals),
                    }).ToArray();
                }

                foreach (var (gltfPrim, submesh) in Enumerable.Zip(gltfMesh.primitives, mesh.Submeshes, (l, r) => (l, r)))
                {
                    var subIndices = indices.Slice(submesh.Offset, submesh.DrawCount);
                    gltfPrim.indices = AddAccessor(subIndices);
                    gltfPrim.attributes.POSITION = position.Value;
                    gltfPrim.attributes.NORMAL = normal.GetValueOrDefault(-1); // たぶん、ありえる
                    gltfPrim.attributes.TANGENT = -1;
                    gltfPrim.attributes.COLOR_0 = color.GetValueOrDefault(-1);
                    gltfPrim.attributes.TEXCOORD_0 = uv.GetValueOrDefault(-1); // ありえる？
                    gltfPrim.attributes.TEXCOORD_1 = -1;
                    gltfPrim.attributes.WEIGHTS_0 = weights.GetValueOrDefault(-1);
                    gltfPrim.attributes.JOINTS_0 = joints.GetValueOrDefault(-1);
                    foreach (var (gltfMorph, morph) in Enumerable.Zip(gltfPrim.targets, morphTargets, (l, r) => (l, r)))
                    {
                        gltfMorph.POSITION = morph.Position.GetValueOrDefault(-1);
                        gltfMorph.NORMAL = morph.Normal.GetValueOrDefault(-1);
                        gltfMorph.TANGENT = -1;
                    }
                }
            }

            // update nodes and remove unused skin
            var skins = gltf.skins.ToArray();
            gltf.skins.Clear();
            foreach (var (gltfNode, node) in Enumerable.Zip(gltf.nodes, model.Nodes, (l, r) => (l, r)))
            {
                gltfNode.translation = node.LocalTranslation.ToFloat3();
                gltfNode.rotation = node.LocalRotation.ToFloat4();
                gltfNode.scale = node.LocalScaling.ToFloat3();
                if (gltfNode.mesh >= 0)
                {
                    if (gltfNode.skin >= 0)
                    {
                        var gltfSkin = skins[gltfNode.skin];
                        // get or create
                        var skinIndex = gltf.skins.IndexOf(gltfSkin);
                        if (skinIndex == -1)
                        {
                            skinIndex = gltf.skins.Count;
                            gltfSkin.inverseBindMatrices = AddAccessor(node.MeshGroup.Skin.InverseMatrices.GetSpan<Matrix4x4>());
                            gltf.skins.Add(gltfSkin);
                        }
                        // update
                        gltfNode.skin = skinIndex;
                    }
                }
                else
                {
                    gltfNode.skin = -1;
                }
            }

            // replace
            gltf.bufferViews = _bufferViews;
            gltf.accessors = _accessors;

            return (gltf, _buffer.Bytes);
        }
    }
}
