using UnityEngine;
using System.Linq;
using System.Collections.Generic;


namespace UniGLTF.MeshUtility
{
    public static class MeshExtensions
    {
        public static Mesh GetMesh(this Renderer r)
        {
            if (r is SkinnedMeshRenderer smr)
            {
                return smr.sharedMesh;
            }
            if (r is MeshRenderer mr)
            {
                if (mr.TryGetComponent<MeshFilter>(out var mf))
                {
                    return mf.sharedMesh;
                }
            }
            return null;
        }

        public static Mesh Copy(this Mesh src, bool copyBlendShape, string nameSuffix = "(copy)")
        {
            var dst = new Mesh();
            dst.name = src.name + nameSuffix;
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
                Vector3[] tangents = null;
                if (Symbols.VRM_NORMALIZE_BLENDSHAPE_TANGENT)
                {
                    tangents = src.tangents.Select(x => (Vector3)x).ToArray();
                }

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

        public static void ApplyRotationAndScale(this Mesh src, Matrix4x4 m, bool removeTranslation = true)
        {
            if (removeTranslation)
            {
                m.SetColumn(3, new Vector4(0, 0, 0, 1)); // remove translation
            }
            src.ApplyMatrix(m);
        }

        public static void ApplyTranslation(this Mesh src, Vector3 p)
        {
            var m = Matrix4x4.identity;
            m.SetColumn(3, new Vector4(p.x, p.y, p.z, 1));
            src.ApplyMatrix(m);
        }

        public static void ApplyMatrix(this Mesh src, Matrix4x4 m)
        {
            src.vertices = src.vertices.Select(x => m.MultiplyPoint(x)).ToArray();
            if (src.normals != null && src.normals.Length > 0)
            {
                src.normals = src.normals.Select(x => m.MultiplyVector(x.normalized)).ToArray();
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

        class BlendShape
        {
            public readonly string Name;
            public readonly float FrameWeight;
            public readonly Vector3[] Vertices;
            public readonly Vector3[] Normals;
            public readonly Vector3[] Tangents;
            public BlendShape(string name, float frameweight,
                IEnumerable<Vector3> vertices,
                IEnumerable<Vector3> normals,
                IEnumerable<Vector3> tangents)
            {
                Name = name;
                FrameWeight = frameweight;
                Vertices = vertices.ToArray();
                Normals = normals.ToArray();
                Tangents = tangents.ToArray();
            }

            public static BlendShape FromMesh(Mesh mesh, int i, Matrix4x4 m)
            {
                var blendShapePositions = new Vector3[mesh.vertexCount];
                var blendShapeNormals = new Vector3[mesh.vertexCount];
                var blendShapeTangents = new Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(i, 0, blendShapePositions, blendShapeNormals, blendShapeTangents);
                return new BlendShape(
                    mesh.GetBlendShapeName(i), mesh.GetBlendShapeFrameWeight(i, 0),
                    blendShapePositions.Select(x => m.MultiplyPoint(x)),
                    blendShapeNormals.Select(x => m.MultiplyPoint(x)),
                    blendShapeTangents.Select(x => m.MultiplyPoint(x)));
            }
        }

        public static void ApplyMatrixAlsoBlendShapes(this Mesh src, Matrix4x4 m)
        {
            src.vertices = src.vertices.Select(x => m.MultiplyPoint(x)).ToArray();
            if (src.normals != null && src.normals.Length > 0)
            {
                src.normals = src.normals.Select(x => m.MultiplyVector(x.normalized)).ToArray();
            }
            if (src.tangents != null && src.tangents.Length > 0)
            {
                src.tangents = src.tangents.Select(x =>
                {
                    var t = m.MultiplyVector((Vector3)x);
                    return new Vector4(t.x, t.y, t.z, x.w);
                }).ToArray();
            }

            var blendshapes = new List<BlendShape>();
            for (int i = 0; i < src.blendShapeCount; ++i)
            {
                blendshapes.Add(BlendShape.FromMesh(src, i, m));
            }
            src.ClearBlendShapes();
            foreach (var blendshape in blendshapes)
            {
                src.AddBlendShapeFrame(blendshape.Name, blendshape.FrameWeight,
                    blendshape.Vertices,
                    // 法線は import / export 対象
                    blendshape.Normals,
                    // tangent は import / export の扱いが無い
                    null
                    );
            }
        }
    }
}
