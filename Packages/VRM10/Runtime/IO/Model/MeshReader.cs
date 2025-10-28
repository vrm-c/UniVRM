using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using VrmLib;

namespace UniVRM10
{
    /// <summary>
    /// GLTF -> VrmLib.MeshGroup
    /// </summary>
    public static class MeshReader
    {
        /// <summary>
        /// VertexBufferはひとつでIndexBufferの参照が異なる
        ///
        ///  VertexBuffer
        ///  +----------------------------------+
        ///  |                                  |
        ///  +----------------------------------+
        ///       A         A        A
        ///       |         |        |
        ///  +---------+--------+--------+
        ///  | submesh0|submesh1|submesh2|
        ///  +---------+--------+--------+
        ///  IndexBuffer
        /// </summary>
        static Mesh SharedBufferFromGltf(this glTFMesh x, Vrm10ImportData storage)
        {
            // 先頭を使う
            return FromGltf(storage, x, x.primitives[0], true);
        }

        /// <summary>
        /// IndexBuffer毎に異なるVertexBufferを参照する
        ///
        ///  VertexBuffer
        ///  +--------+ +--------+ +--------+
        ///  |0       | |1       | |2       |
        ///  +--------+ +--------+ +--------+
        ///       A         A        A
        ///       |         |        |
        ///  +---------+--------+--------+
        ///  | submesh0|submesh1|submesh2|
        ///  +---------+--------+--------+
        ///  IndexBuffer
        /// </summary>
        static Mesh FromGltf(this glTFPrimitives primitive, Vrm10ImportData storage, glTFMesh x)
        {
            return FromGltf(storage, x, primitive, false);
        }

        static Mesh FromGltf(Vrm10ImportData storage, glTFMesh x, glTFPrimitives primitive, bool isShared)
        {
            var mesh = new Mesh((TopologyType)primitive.mode)
            {
                VertexBuffer = primitive.attributes.FromGltf(storage)
            };

            if (isShared)
            {
                // create joined index buffer
                mesh.IndexBuffer = storage.CreateAccessor(x.primitives.Select(y => y.indices).ToArray());
            }
            else
            {
                mesh.IndexBuffer = storage.CreateAccessor(primitive.indices);
            }

            if (mesh.IndexBuffer == null)
            {
                var indices = Enumerable.Range(0, mesh.VertexBuffer.Count).ToArray();
                var na = storage.Data.NativeArrayManager.CreateNativeArray(indices);
                mesh.IndexBuffer = new BufferAccessor(storage.Data.NativeArrayManager, na.Reinterpret<byte>(4), AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, na.Length);
            }

            {
                gltf_mesh_extras_targetNames.TryGet(x, out List<string> targetNames);

                for (int i = 0; i < primitive.targets.Count; ++i)
                {
                    var gltfTarget = primitive.targets[i];

                    string targetName = null;
                    {
                        targetName = targetNames[i];
                    }
                    var target = new MorphTarget(targetName)
                    {
                        VertexBuffer = gltfTarget.FromGltf(storage)
                    };

                    // validate count
                    foreach (var kv in target.VertexBuffer)
                    {
                        if (kv.Value.Count != mesh.VertexBuffer.Count)
                        {
                            throw new Exception();
                        }
                    }

                    mesh.MorphTargets.Add(target);
                }
            }

            return mesh;
        }

        static VertexBuffer FromGltf(this glTFAttributes attributes,
            Vrm10ImportData storage)
        {
            var b = new VertexBuffer();

            if (storage.TryCreateAccessor(attributes.POSITION, out BufferAccessor position))
            {
                b.Add(VertexBuffer.PositionKey, position);
            }
            else
            {
                // position required
                throw new Exception();
            }

            if (storage.TryCreateAccessor(attributes.NORMAL, out BufferAccessor normal)) b.Add(VertexBuffer.NormalKey, normal);
            if (storage.TryCreateAccessor(attributes.COLOR_0, out BufferAccessor color)) b.Add(VertexBuffer.ColorKey, color);
            if (storage.TryCreateAccessor(attributes.TEXCOORD_0, out BufferAccessor tex0)) b.Add(VertexBuffer.TexCoordKey, tex0);
            if (storage.TryCreateAccessor(attributes.TEXCOORD_1, out BufferAccessor tex1)) b.Add(VertexBuffer.TexCoordKey2, tex1);
            // if(storage.TryCreateAccessor(attributes.TANGENT, out BufferAccessor tangent))b.Add(VertexBuffer.TangentKey, tangent);
            if (storage.TryCreateAccessor(attributes.WEIGHTS_0, out BufferAccessor weights)) b.Add(VertexBuffer.WeightKey, weights);
            if (storage.TryCreateAccessor(attributes.JOINTS_0, out BufferAccessor joints)) b.Add(VertexBuffer.JointKey, joints);

            return b;
        }

        static VertexBuffer FromGltf(this gltfMorphTarget target, Vrm10ImportData storage)
        {
            var b = new VertexBuffer();
            storage.CreateBufferAccessorAndAdd(target.POSITION, b, VertexBuffer.PositionKey);
            storage.CreateBufferAccessorAndAdd(target.NORMAL, b, VertexBuffer.NormalKey);
            storage.CreateBufferAccessorAndAdd(target.TANGENT, b, VertexBuffer.TangentKey);
            return b;
        }

        static bool HasSameVertexBuffer(this glTFPrimitives lhs, glTFPrimitives rhs)
        {
            if (lhs.attributes.POSITION != rhs.attributes.POSITION) return false;
            if (lhs.attributes.NORMAL != rhs.attributes.NORMAL) return false;
            if (lhs.attributes.TEXCOORD_0 != rhs.attributes.TEXCOORD_0) return false;
            if (lhs.attributes.TEXCOORD_1 != rhs.attributes.TEXCOORD_1) return false;
            if (lhs.attributes.COLOR_0 != rhs.attributes.COLOR_0) return false;
            if (lhs.attributes.WEIGHTS_0 != rhs.attributes.WEIGHTS_0) return false;
            if (lhs.attributes.JOINTS_0 != rhs.attributes.JOINTS_0) return false;
            return true;
        }

        static bool AllPrimitivesHasSameVertexBuffer(this glTFMesh m)
        {
            if (m.primitives.Count <= 1)
            {
                return true;
            }

            var first = m.primitives[0];
            for (int i = 1; i < m.primitives.Count; ++i)
            {
                if (!first.HasSameVertexBuffer(m.primitives[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static MeshGroup FromGltf(this glTFMesh x, Vrm10ImportData storage)
        {
            var group = new MeshGroup(x.name);

            if (x.primitives.Count == 1)
            {
                var primitive = x.primitives[0];
                var mesh = primitive.FromGltf(storage, x);
                var materialIndex = primitive.material;

                mesh.Submeshes.Add(
                    new Submesh(0, mesh.IndexBuffer.Count, materialIndex));

                group.Meshes.Add(mesh);
            }
            else if (!x.AllPrimitivesHasSameVertexBuffer())
            {
                int offset = 0;
                foreach (var primitive in x.primitives)
                {
                    var mesh = primitive.FromGltf(storage, x);
                    var materialIndex = primitive.material;

                    mesh.Submeshes.Add(
                        new Submesh(offset, mesh.IndexBuffer.Count, materialIndex));
                    offset += mesh.IndexBuffer.Count;

                    group.Meshes.Add(mesh);
                }
            }
            else
            {
                //
                // obsolete
                //
                // for VRM

                var mesh = x.SharedBufferFromGltf(storage);
                int offset = 0;
                foreach (var primitive in x.primitives)
                {
                    var materialIndex = primitive.material;
                    var count = storage.Gltf.accessors[primitive.indices].count;
                    mesh.Submeshes.Add(
                        new Submesh(offset, count, materialIndex));
                    offset += count;
                }

                group.Meshes.Add(mesh);
            }

            return group;
        }
    }
}
