using System.Numerics;

namespace VrmLib
{
    public static class MeshFactory
    {
        public static Mesh CreateQuadrangle()
        {
            var mesh = new Mesh();
            mesh.IndexBuffer = BufferAccessor.Create(new int[] { 0, 1, 2, 2, 3, 0 });
            mesh.VertexBuffer = new VertexBuffer();
            mesh.VertexBuffer.Add(VertexBuffer.PositionKey, new Vector3[]{
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0),
                });
            mesh.VertexBuffer.Add(VertexBuffer.TexCoordKey, new Vector2[]{
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                });
            mesh.Submeshes.Add(new Submesh(0, 6, new Material("SCREEN")));
            return mesh;
        }
    }
}