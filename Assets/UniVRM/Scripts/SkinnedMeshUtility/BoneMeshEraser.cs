using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace VRM
{
    public static class BoneMeshEraser
    {
        [Serializable]
        public struct EraseBone
        {
            public Transform Bone;
            public bool Erase;

            public override string ToString()
            {
                return Bone.name + ":" + Erase;
            }
        }

        static int ExcludeTriangles(int[] triangles, BoneWeight[] bws, int[] exclude)
        {
            int count = 0;
            if (bws != null && bws.Length>0)
            {
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    var a = triangles[i];
                    var b = triangles[i + 1];
                    var c = triangles[i + 2];

                    {
                        var bw = bws[a];
                        if (bw.weight0 > 0 && exclude.Contains(bw.boneIndex0)) continue;
                        if (bw.weight1 > 0 && exclude.Contains(bw.boneIndex1)) continue;
                        if (bw.weight2 > 0 && exclude.Contains(bw.boneIndex2)) continue;
                        if (bw.weight3 > 0 && exclude.Contains(bw.boneIndex3)) continue;
                    }
                    {
                        var bw = bws[b];
                        if (bw.weight0 > 0 && exclude.Contains(bw.boneIndex0)) continue;
                        if (bw.weight1 > 0 && exclude.Contains(bw.boneIndex1)) continue;
                        if (bw.weight2 > 0 && exclude.Contains(bw.boneIndex2)) continue;
                        if (bw.weight3 > 0 && exclude.Contains(bw.boneIndex3)) continue;
                    }
                    {
                        var bw = bws[c];
                        if (bw.weight0 > 0 && exclude.Contains(bw.boneIndex0)) continue;
                        if (bw.weight1 > 0 && exclude.Contains(bw.boneIndex1)) continue;
                        if (bw.weight2 > 0 && exclude.Contains(bw.boneIndex2)) continue;
                        if (bw.weight3 > 0 && exclude.Contains(bw.boneIndex3)) continue;
                    }

                    triangles[count++] = a;
                    triangles[count++] = b;
                    triangles[count++] = c;
                }
            }
            return count;
        }

        public static Mesh CreateErasedMesh(Mesh src, int[] eraseBoneIndices)
        {
            /*
            Debug.LogFormat("{0} exclude: {1}", 
                src.name,
                String.Join(", ", eraseBoneIndices.Select(x => x.ToString()).ToArray())
                );
            */
            var mesh = new Mesh();
            mesh.name = src.name + "(erased)";

#if UNITY_2017_3_OR_NEWER
            mesh.indexFormat = src.indexFormat;
#endif

            mesh.vertices = src.vertices;
            mesh.normals = src.normals;
            mesh.uv = src.uv;
            mesh.tangents = src.tangents;
            mesh.boneWeights = src.boneWeights;
            mesh.bindposes = src.bindposes;
            mesh.subMeshCount = src.subMeshCount;
            for (int i = 0; i < src.subMeshCount; ++i)
            {
                var indices = src.GetIndices(i);
                var count = ExcludeTriangles(indices, mesh.boneWeights, eraseBoneIndices);
                var dst = new int[count];
                Array.Copy(indices, 0, dst, 0, count);
                mesh.SetIndices(dst, MeshTopology.Triangles, i);
            }

            return mesh;
        }

        public static int IndexOf(this Transform[] list, Transform target)
        {
            for (int i = 0; i < list.Length; ++i)
            {
                if (list[i] == target)
                {
                    return i;
                }
            }
            return -1;
        }

        public static IEnumerable<Transform> Ancestor(this Transform t)
        {
            yield return t;

            if (t.parent != null)
            {
                foreach (var x in Ancestor(t.parent))
                {
                    yield return x;
                }
            }
        }
    }
}
