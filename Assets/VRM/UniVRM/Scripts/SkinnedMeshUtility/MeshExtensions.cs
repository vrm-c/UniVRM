using UnityEngine;
using System.Linq;


namespace VRM
{
    public static class MeshExtensions
    {
        public static Mesh Copy(this Mesh src, bool copyBlendShape)
        {
            var dst = new Mesh();
            dst.name = src.name + "(copy)";
#if UNITY_2017_3_OR_NEWER
            dst.indexFormat = src.indexFormat;
#endif

            dst.vertices = src.vertices;
            dst.normals = src.normals;
            dst.tangents = src.tangents;
            dst.colors = src.colors;
            dst.uv = src.uv;
            dst.uv2 = src.uv2;
            dst.uv3 = src.uv3;
            dst.uv4 = src.uv4;
            dst.boneWeights = src.boneWeights;
            dst.bindposes = src.bindposes;

            dst.subMeshCount = src.subMeshCount;
            for (int i = 0; i < dst.subMeshCount; ++i)
            {
                dst.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
            }

            dst.RecalculateBounds();

            if (copyBlendShape)
            {
                var vertices = src.vertices;
                var normals = src.normals;
#if VRM_NORMALIZE_BLENDSHAPE_TANGENT
                var tangents = src.tangents.Select(x => (Vector3)x).ToArray();
#else
                Vector3[] tangents = null;
#endif

                for (int i = 0; i < src.blendShapeCount; ++i)
                {
                    src.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);
                    dst.AddBlendShapeFrame(
                        src.GetBlendShapeName(i),
                        src.GetBlendShapeFrameWeight(i, 0),
                        vertices,
                        normals,
                        tangents
                        );
                }
            }

            return dst;
        }

        public static void ApplyRotationAndScale(this Mesh src, Matrix4x4 m)
        {
            m.SetColumn(3, new Vector4(0, 0, 0, 1)); // remove translation
            src.ApplyMatrix(m);
        }

        public static void ApplyMatrix(this Mesh src, Matrix4x4 m)
        {
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
