using System;
using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class MeshDataTests
    {
        /// <summary>
        /// shared
        /// 3 2
        /// +-+
        /// |/|
        /// +-+
        /// 0 1
        ///
        /// divided
        ///   2
        ///   +
        ///  /|
        /// +-+
        /// 0 1
        /// 4 3
        /// +-+
        /// |/
        /// +
        /// 5
        /// </summary>
        static byte[] CreateTestData(bool shared, bool hasNormal)
        {
            var data = new ExportingGltfData();
            data.Gltf.asset.version = "2.0";
            var mesh = new glTFMesh();
            data.Gltf.meshes.Add(mesh);

            if (shared)
            {
                var positions = new Vector3[]
                {
                    new Vector3(),
                    new Vector3(),
                    new Vector3(),
                    new Vector3(),
                };
                var normals = new Vector3[]
                {
                    new Vector3(),
                    new Vector3(),
                    new Vector3(),
                    new Vector3(),
                };

                var position = data.ExtendBufferAndGetAccessorIndex(positions);
                var normal = data.ExtendBufferAndGetAccessorIndex(normals);
                {
                    var prim = new glTFPrimitives
                    {
                        attributes = new glTFAttributes
                        {
                            POSITION = position,
                        },
                        indices = data.ExtendBufferAndGetAccessorIndex(new uint[] { 0, 1, 2 }),
                    };
                    mesh.primitives.Add(prim);
                    if (hasNormal)
                    {
                        prim.attributes.NORMAL = normal;
                    }
                }
                {
                    var prim = new glTFPrimitives
                    {
                        attributes = new glTFAttributes
                        {
                            POSITION = position,
                        },
                        indices = data.ExtendBufferAndGetAccessorIndex(new uint[] { 2, 3, 0 }),
                    };
                    mesh.primitives.Add(prim);
                    if (hasNormal)
                    {
                        prim.attributes.NORMAL = normal;
                    }
                }
            }
            else
            {
                {
                    var positions = new Vector3[]
                    {
                        new Vector3(),
                        new Vector3(),
                        new Vector3(),
                    };
                    var position = data.ExtendBufferAndGetAccessorIndex(positions);
                    var prim = new glTFPrimitives
                    {
                        attributes = new glTFAttributes
                        {
                            POSITION = position,
                        },
                        indices = data.ExtendBufferAndGetAccessorIndex(new uint[] { 0, 1, 2 }),
                    };
                    if (hasNormal)
                    {
                        var normals = new Vector3[]
                        {
                            new Vector3(),
                            new Vector3(),
                            new Vector3(),
                        };
                        var normal = data.ExtendBufferAndGetAccessorIndex(normals);
                        prim.attributes.NORMAL = normal;
                    }
                    mesh.primitives.Add(prim);
                }
                {
                    var positions = new Vector3[]
                    {
                        new Vector3(),
                        new Vector3(),
                        new Vector3(),
                    };
                    var position = data.ExtendBufferAndGetAccessorIndex(positions);
                    var prim = new glTFPrimitives
                    {
                        attributes = new glTFAttributes
                        {
                            POSITION = position,
                        },
                        indices = data.ExtendBufferAndGetAccessorIndex(new uint[] { 0, 1, 2 }),
                    };
                    if (hasNormal)
                    {
                        var normals = new Vector3[]
                        {
                            new Vector3(),
                            new Vector3(),
                            new Vector3(),
                        };
                        var normal = data.ExtendBufferAndGetAccessorIndex(normals);
                        prim.attributes.NORMAL = normal;
                    }
                    mesh.primitives.Add(prim);
                }
            }
            return data.ToGlbBytes();
        }

        [Test]
        public void SharedHasNormalTest()
        {
            var glb = CreateTestData(true, true);
            using (var parsed = new GlbBinaryParser(glb, "test").Parse())
            {
                Assert.True(MeshData.HasSharedVertexBuffer(parsed.GLTF.meshes[0]));
                using (var data = new MeshData(6, 6))
                {
                    data.LoadFromGltf(parsed, 0, new ReverseZ());
                    Assert.True(data.HasNormal);
                }
            }
        }

        [Test]
        public void SharedNotHasNormalTest()
        {
            var glb = CreateTestData(true, false);
            using (var parsed = new GlbBinaryParser(glb, "test").Parse())
            {
                Assert.True(MeshData.HasSharedVertexBuffer(parsed.GLTF.meshes[0]));
                using (var data = new MeshData(6, 6))
                {
                    data.LoadFromGltf(parsed, 0, new ReverseZ());
                    Assert.False(data.HasNormal);
                }
            }
        }

        [Test]
        public void DividedHasNormalTest()
        {
            var glb = CreateTestData(false, true);
            using (var parsed = new GlbBinaryParser(glb, "test").Parse())
            {
                Assert.False(MeshData.HasSharedVertexBuffer(parsed.GLTF.meshes[0]));
                using (var data = new MeshData(6, 6))
                {
                    data.LoadFromGltf(parsed, 0, new ReverseZ());
                    Assert.True(data.HasNormal);
                }
            }
        }

        [Test]
        public void DividedNotHasNormalTest()
        {
            var glb = CreateTestData(false, false);
            using (var parsed = new GlbBinaryParser(glb, "test").Parse())
            {
                Assert.False(MeshData.HasSharedVertexBuffer(parsed.GLTF.meshes[0]));
                using (var data = new MeshData(6, 6))
                {
                    data.LoadFromGltf(parsed, 0, new ReverseZ());
                    Assert.False(data.HasNormal);
                }
            }
        }
    }
}
