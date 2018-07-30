using UnityEngine;
using System.Linq;


namespace VRM
{
    public static class MeshExtensions
    {
        public static Mesh Copy(this Mesh src)
        {
            var dst = new Mesh();
            dst.name = src.name + "(copy)";
#if UNITY_2017_3_OR_NEWER
            dst.indexFormat = src.indexFormat;
#endif

            dst.vertices = src.vertices;
            dst.normals = src.normals;
            dst.tangents = src.tangents;

            if (src.colors != null && src.colors.Length > 0) dst.colors = src.colors;
            if (src.uv != null && src.uv.Length > 0) dst.uv = src.uv;
            if (src.uv2 != null && src.uv2.Length > 0) dst.uv2 = src.uv2;
            if (src.uv3 != null && src.uv3.Length > 0) dst.uv3 = src.uv3;
            if (src.uv4 != null && src.uv4.Length > 0) dst.uv4 = src.uv4;

            dst.subMeshCount = src.subMeshCount;
            for (int i = 0; i < dst.subMeshCount; ++i)
            {
                dst.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
            }

            return dst;
        }

        public static void ApplyRotationAndScale(this Mesh src, Matrix4x4 m)
        {
            m.SetColumn(3, new Vector4(0, 0, 0, 1));
            src.ApplyMatrix(m);
        }

        public static void ApplyMatrix(this Mesh src, Matrix4x4 m)
        {
            m.SetColumn(3, new Vector4(0, 0, 0, 1));

            src.vertices = src.vertices.Select(x => m.MultiplyPoint(x)).ToArray();
            if (src.normals != null && src.normals.Length > 0)
            {
                src.normals = src.normals.Select(x => m.MultiplyVector(x)).ToArray();
            }
            if (src.tangents != null && src.tangents.Length > 0)
            {
                src.tangents = src.tangents.Select(x =>
                {
                    var t = m.MultiplyVector((Vector3)x);
                    return new Vector4(t.x, t.y, t.z, x.w);
                }).ToArray();
            }
        }
    }
}
