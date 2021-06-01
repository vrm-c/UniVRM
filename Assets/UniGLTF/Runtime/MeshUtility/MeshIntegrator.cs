using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrator
    {
        public struct SubMesh
        {
            public List<int> Indices;
            public Material Material;
        }

        public class BlendShape
        {
            public int VertexOffset;
            public string Name;
            public float FrameWeight;
            public Vector3[] Positions;
            public Vector3[] Normals;
            public Vector3[] Tangents;
        }
        
//        public List<SkinnedMeshRenderer> Renderers { get; private set; }
        public List<Vector3> Positions { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<Vector2> UV { get; private set; }
        public List<Vector4> Tangents { get; private set; }
        public List<BoneWeight> BoneWeights { get; private set; }

        public List<SubMesh> SubMeshes
        {
            get;
            private set;
        }

        public List<Matrix4x4> BindPoses { get; private set; }
        public List<Transform> Bones { get; private set; }

        public List<BlendShape> BlendShapes { get; private set; }
        public void AddBlendShapesToMesh(Mesh mesh)
        {
            Dictionary<string, BlendShape> map = new Dictionary<string, BlendShape>();

            foreach (var x in BlendShapes)
            {
                BlendShape bs = null;
                if (!map.TryGetValue(x.Name, out bs))
                {
                    bs = new BlendShape();
                    bs.Positions = new Vector3[Positions.Count];
                    bs.Normals = new Vector3[Normals.Count];
                    bs.Tangents = new Vector3[Tangents.Count];
                    bs.Name = x.Name;
                    bs.FrameWeight = x.FrameWeight;
                    map.Add(x.Name, bs);
                }

                var j = x.VertexOffset;
                for (int i = 0; i < x.Positions.Length; ++i, ++j)
                {
                    bs.Positions[j] = x.Positions[i];
                    bs.Normals[j] = x.Normals[i];
                    bs.Tangents[j] = x.Tangents[i];
                }
            }

            foreach (var kv in map)
            {
                //Debug.LogFormat("AddBlendShapeFrame: {0}", kv.Key);
                mesh.AddBlendShapeFrame(kv.Key, kv.Value.FrameWeight,
                    kv.Value.Positions, kv.Value.Normals, kv.Value.Tangents);
            }
        }

        public MeshIntegrator()
        {
//            Renderers = new List<SkinnedMeshRenderer>();

            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            UV = new List<Vector2>();
            Tangents = new List<Vector4>();
            BoneWeights = new List<BoneWeight>();

            SubMeshes = new List<SubMesh>();

            BindPoses = new List<Matrix4x4>();
            Bones = new List<Transform>();

            BlendShapes = new List<BlendShape>();
        }

        static BoneWeight AddBoneIndexOffset(BoneWeight bw, int boneIndexOffset)
        {
            if (bw.weight0 > 0) bw.boneIndex0 += boneIndexOffset;
            if (bw.weight1 > 0) bw.boneIndex1 += boneIndexOffset;
            if (bw.weight2 > 0) bw.boneIndex2 += boneIndexOffset;
            if (bw.weight3 > 0) bw.boneIndex3 += boneIndexOffset;
            return bw;
        }

        public void Push(MeshRenderer renderer)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogWarningFormat("{0} has no mesh filter", renderer.name);
                return;
            }
            var mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarningFormat("{0} has no mesh", renderer.name);
                return;
            }

            var indexOffset = Positions.Count;
            var boneIndexOffset = Bones.Count;

            Positions.AddRange(mesh.vertices
                .Select(x => renderer.transform.TransformPoint(x))
            );
            Normals.AddRange(mesh.normals
                .Select(x => renderer.transform.TransformVector(x))
            );
            UV.AddRange(mesh.uv);
            Tangents.AddRange(mesh.tangents
                .Select(t =>
                {
                    var v = renderer.transform.TransformVector(t.x, t.y, t.z);
                    return new Vector4(v.x, v.y, v.z, t.w);
                })
            );

            var self = renderer.transform;
            var bone = self.parent;
            if (bone == null)
            {
                Debug.LogWarningFormat("{0} is root gameobject.", self.name);
                return;
            }
            var bindpose = bone.worldToLocalMatrix;
            
            BoneWeights.AddRange(Enumerable.Range(0, mesh.vertices.Length)
                .Select(x => new BoneWeight()
                {
                    boneIndex0 = Bones.Count,
                    weight0 = 1,
                })
            );
            
            BindPoses.Add(bindpose);
            Bones.Add(bone);
            
            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var indices = mesh.GetIndices(i).Select(x => x + indexOffset);
                var mat = renderer.sharedMaterials[i];
                var sameMaterialSubMeshIndex = SubMeshes.FindIndex(x => ReferenceEquals(x.Material, mat));
                if (sameMaterialSubMeshIndex >= 0)
                {
                    SubMeshes[sameMaterialSubMeshIndex].Indices.AddRange(indices);
                }
                else
                {
                    SubMeshes.Add(new SubMesh
                    {
                        Indices = indices.ToList(),
                        Material = mat,
                    });
                }
            }
        }

        public void Push(SkinnedMeshRenderer renderer)
        {
            var mesh = renderer.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarningFormat("{0} has no mesh", renderer.name);
                return;
            }

//            Renderers.Add(renderer);

            var indexOffset = Positions.Count;
            var boneIndexOffset = Bones.Count;

            Positions.AddRange(mesh.vertices);
            Normals.AddRange(mesh.normals);
            UV.AddRange(mesh.uv);
            Tangents.AddRange(mesh.tangents);

            if (mesh.vertexCount == mesh.boneWeights.Length)
            {
                BoneWeights.AddRange(mesh.boneWeights.Select(x => AddBoneIndexOffset(x, boneIndexOffset)).ToArray());
                BindPoses.AddRange(mesh.bindposes);
                Bones.AddRange(renderer.bones);
            }
            else
            {
                // Bone Count 0 の SkinnedMeshRenderer
                var rigidBoneWeight = new BoneWeight
                {
                    boneIndex0 = boneIndexOffset,
                    weight0 = 1f,
                };
                BoneWeights.AddRange(Enumerable.Range(0, mesh.vertexCount).Select(x => rigidBoneWeight).ToArray());
                BindPoses.Add(renderer.transform.localToWorldMatrix);
                Bones.Add(renderer.transform);
            }

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var indices = mesh.GetIndices(i).Select(x => x + indexOffset);
                var mat = renderer.sharedMaterials[i];
                var sameMaterialSubMeshIndex = SubMeshes.FindIndex(x => ReferenceEquals(x.Material, mat));
                if (sameMaterialSubMeshIndex >= 0)
                {
                    SubMeshes[sameMaterialSubMeshIndex].Indices.AddRange(indices);
                }
                else
                {
                    SubMeshes.Add(new SubMesh
                    {
                        Indices = indices.ToList(),
                        Material = mat,
                    });
                }
            }

            for (int i = 0; i < mesh.blendShapeCount; ++i)
            {
                var positions = (Vector3[])mesh.vertices.Clone();
                var normals = (Vector3[])mesh.normals.Clone();
                var tangents = mesh.tangents.Select(x => (Vector3)x).ToArray();

                mesh.GetBlendShapeFrameVertices(i, 0, positions, normals, tangents);
                BlendShapes.Add(new BlendShape
                {
                    VertexOffset = indexOffset,
                    FrameWeight = mesh.GetBlendShapeFrameWeight(i, 0),
                    Name = mesh.GetBlendShapeName(i),
                    Positions = positions,
                    Normals = normals,
                    Tangents = tangents,
                });
            }
        }
    }
}