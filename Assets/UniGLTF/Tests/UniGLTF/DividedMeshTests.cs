using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class DividedMeshTests
    {
        /// <summary>
        /// positions: [
        ///   {1, 1, 0}    
        ///   {1, 1, 1}    
        ///   {1, 1, 2}    
        ///   {1, 1, 3}    
        ///   {1, 1, 4}    
        ///   {1, 1, 5}    
        /// ]
        /// submesh
        ///     0 1 2
        /// submesh
        ///     3 4 5
        /// </summary>
        [Test]
        public void ExportDividedMeshTest()
        {
            var gltf = new glTF();
            gltf.AddBuffer(new UniGLTF.ArrayByteBuffer());

            {
                var buffer = new UniGLTF.MeshExporterDivided.VertexBuffer(6, null);
                buffer.Push(new Vector3(1, 1, 0), Vector3.up, Vector2.zero);
                buffer.Push(new Vector3(1, 1, 1), Vector3.up, Vector2.zero);
                buffer.Push(new Vector3(1, 1, 2), Vector3.up, Vector2.zero);
                var (prim, count) = buffer.ToGltfPrimitive(gltf, 0, 0, new[] { 0, 1, 2 }, 0);
                Assert.AreEqual(3, count);
            }

            {
                var buffer = new UniGLTF.MeshExporterDivided.VertexBuffer(6, null);
                buffer.Push(new Vector3(1, 1, 3), Vector3.up, Vector2.zero);
                buffer.Push(new Vector3(1, 1, 4), Vector3.up, Vector2.zero);
                buffer.Push(new Vector3(1, 1, 5), Vector3.up, Vector2.zero);
                var (prim, count) = buffer.ToGltfPrimitive(gltf, 0, 0, new[] { 3, 4, 5 }, 3);
                Assert.AreEqual(3, count);
            }
        }
    }
}
