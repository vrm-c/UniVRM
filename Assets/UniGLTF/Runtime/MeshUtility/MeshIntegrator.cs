using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrator
    {
        public enum BlendShapeOperation
        {
            // No BlendShape(drop if mesh has blendshape)
            None,
            // Use blendShape(keep blendshape)
            Use,
            // Integrate to two mesh that is with blendShape and is without blendshape
            Split,
        }

        struct SubMesh
        {
            public List<int> Indices;
            public Material Material;
        }

        class BlendShape
        {
            public int VertexOffset;
            public string Name;
            public float FrameWeight;
            public Vector3[] Positions;
            public Vector3[] Normals;
            public Vector3[] Tangents;
        }

        MeshIntegrationResult Result { get; } = new MeshIntegrationResult();
        List<Vector3> Positions { get; } = new List<Vector3>();
        List<Vector3> Normals { get; } = new List<Vector3>();
        List<Vector2> UV { get; } = new List<Vector2>();
        List<Vector4> Tangents { get; } = new List<Vector4>();
        List<BoneWeight> BoneWeights { get; } = new List<BoneWeight>();
        List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
        List<Matrix4x4> _BindPoses { get; } = new List<Matrix4x4>();
        List<Transform> _Bones { get; } = new List<Transform>();
        List<BlendShape> BlendShapes { get; } = new List<BlendShape>();
        void AddBlendShapesToMesh(Mesh mesh)
        {
            Dictionary<string, BlendShape> map = new Dictionary<string, BlendShape>();

            foreach (var x in BlendShapes)
            {
                BlendShape bs = null;
                if (!map.TryGetValue(x.Name, out bs))
                {
                    bs = new BlendShape();
                    //  arrays size must match mesh vertex count
                    bs.Positions = new Vector3[Positions.Count];
                    bs.Normals = new Vector3[Positions.Count];
                    bs.Tangents = new Vector3[Positions.Count];
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

        int AddBoneIfUnique(Transform bone, Matrix4x4? pose = default)
        {
            var index = _Bones.IndexOf(bone);
            if (index == -1)
            {
                index = _Bones.Count;
                _Bones.Add(bone);
                _BindPoses.Add(pose.HasValue ? pose.Value : bone.worldToLocalMatrix);
            }
            return index;
        }

        void AddBoneIfUnique(Transform[] bones, Matrix4x4[] bindPoses, int boneIndex, float weight, Action<int, float> setter)
        {
            if (boneIndex < 0 || boneIndex >= bones.Length || weight <= 0)
            {
                setter(0, 0);
                return;
            }
            var t = bones[boneIndex];
            setter(AddBoneIfUnique(t, bindPoses[boneIndex]), weight);
        }

        void Push(MeshRenderer renderer)
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
            Result.SourceMeshRenderers.Add(renderer);
            Result.Sources.Add(mesh);

            var indexOffset = Positions.Count;

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
            var boneIndex = AddBoneIfUnique(bone);

            BoneWeights.AddRange(Enumerable.Range(0, mesh.vertices.Length)
                .Select(x => new BoneWeight()
                {
                    boneIndex0 = boneIndex,
                    weight0 = 1,
                })
            );


            for (int i = 0; i < mesh.subMeshCount && i < renderer.sharedMaterials.Length; ++i)
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
            if (renderer == null)
            {
                // Debug.LogWarningFormat("{0} was destroyed", renderer);
                return;
            }

            var mesh = renderer.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarningFormat("{0} has no mesh", renderer.name);
                return;
            }
            Result.SourceSkinnedMeshRenderers.Add(renderer);
            Result.Sources.Add(mesh);

            var indexOffset = Positions.Count;
            // var boneIndexOffset = Bones.Count;

            Positions.AddRange(mesh.vertices);
            Normals.AddRange(mesh.normals);
            UV.AddRange(mesh.uv);
            Tangents.AddRange(mesh.tangents);

            if (mesh.vertexCount == mesh.boneWeights.Length)
            {
                // AddBone  AddBoneIndexOffset(x, boneIndexOffset)                
                BoneWeights.AddRange(mesh.boneWeights.Select(x =>
                {
                    var bw = new BoneWeight();
                    AddBoneIfUnique(renderer.bones, mesh.bindposes, x.boneIndex0, x.weight0, (i, w) => { bw.boneIndex0 = i; bw.weight0 = w; });
                    AddBoneIfUnique(renderer.bones, mesh.bindposes, x.boneIndex1, x.weight1, (i, w) => { bw.boneIndex1 = i; bw.weight1 = w; });
                    AddBoneIfUnique(renderer.bones, mesh.bindposes, x.boneIndex2, x.weight2, (i, w) => { bw.boneIndex2 = i; bw.weight2 = w; });
                    AddBoneIfUnique(renderer.bones, mesh.bindposes, x.boneIndex3, x.weight3, (i, w) => { bw.boneIndex3 = i; bw.weight3 = w; });
                    return bw;
                }).ToArray());
            }
            else
            {
                // Bone Count 0 の SkinnedMeshRenderer
                var rigidBoneWeight = new BoneWeight
                {
                    boneIndex0 = AddBoneIfUnique(renderer.transform),
                    weight0 = 1f,
                };
                BoneWeights.AddRange(Enumerable.Range(0, mesh.vertexCount).Select(x => rigidBoneWeight).ToArray());
            }

            for (int i = 0; i < mesh.subMeshCount && i < renderer.sharedMaterials.Length; ++i)
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
                //  arrays size must match mesh vertex count
                var positions = new Vector3[mesh.vertexCount];
                var normals = new Vector3[mesh.vertexCount];
                var tangents = new Vector3[mesh.vertexCount];
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

        public static MeshIntegrationResult Integrate(MeshIntegrationGroup group, BlendShapeOperation op)
        {
            var integrator = new MeshUtility.MeshIntegrator();
            foreach (var x in group.Renderers)
            {
                if (x is SkinnedMeshRenderer smr)
                {
                    integrator.Push(smr);
                }
                else if (x is MeshRenderer mr)
                {
                    integrator.Push(mr);
                }
            }
            return integrator.Integrate(group.Name, op);
        }

        delegate bool TriangleFilter(int i0, int i1, int i2);

        Mesh CreateMesh(string name, List<DrawCount> dst, TriangleFilter filter)
        {
            var mesh = new Mesh();
            mesh.name = name;
            if (Positions.Count > ushort.MaxValue)
            {
                Debug.LogFormat("exceed 65535 vertices: {0}", Positions.Count);
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.vertices = Positions.ToArray();
            mesh.normals = Normals.ToArray();
            mesh.uv = UV.ToArray();
            mesh.tangents = Tangents.ToArray();
            mesh.boneWeights = BoneWeights.ToArray();
            mesh.subMeshCount = SubMeshes.Count;
            for (var submesh = 0; submesh < SubMeshes.Count; ++submesh)
            {
                if (filter != null)
                {
                    var indices = SubMeshes[submesh].Indices.ToArray();
                    var filtered = new List<int>();
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        var i0 = indices[i];
                        var i1 = indices[i + 1];
                        var i2 = indices[i + 2];
                        if (filter(i0, i1, i2))
                        {
                            filtered.Add(i0);
                            filtered.Add(i1);
                            filtered.Add(i2);
                        }
                    }
                    mesh.SetIndices(filtered.ToArray(), MeshTopology.Triangles, submesh);
                    dst.Add(new DrawCount
                    {
                        Count = filtered.Count,
                        Material = SubMeshes[submesh].Material,
                    });
                }
                else
                {
                    // use all triangle
                    var indices = SubMeshes[submesh].Indices;
                    mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, submesh);
                    dst.Add(new DrawCount
                    {
                        Count = indices.Count,
                        Material = SubMeshes[submesh].Material,
                    });
                }
            }

            return mesh;
        }

        public MeshIntegrationResult Integrate(string name, BlendShapeOperation op)
        {
            if (_Bones.Count != _BindPoses.Count)
            {
                throw new ArgumentException();
            }

            bool useSplit = false;
            var used = new bool[Positions.Count];
            if (op == BlendShapeOperation.Split)
            {
                foreach (var x in BlendShapes)
                {
                    for (int i = 0; i < x.Positions.Length; ++i)
                    {
                        if (x.Positions[i] != Vector3.zero)
                        {
                            used[i] = true;
                        }
                    }
                }
                var count = used.Count(x => x);
                useSplit = count > 0 && count < used.Length;
            }

            if (useSplit)
            {
                Result.Integrated = new MeshInfo();
                var mesh = CreateMesh(name, Result.Integrated.SubMeshes,
                    (i0, i1, i2) => used[i0] || used[i1] || used[i2]);
                Result.Integrated.Mesh = mesh;
                AddBlendShapesToMesh(mesh);
                // skinning
                mesh.bindposes = _BindPoses.ToArray();

                Result.IntegratedNoBlendShape = new MeshInfo();
                var meshWithoutBlendShape = CreateMesh(name + ".no_blendshape", Result.IntegratedNoBlendShape.SubMeshes,
                    (i0, i1, i2) => !used[i0] && !used[i1] && !used[i2]);
                Result.IntegratedNoBlendShape.Mesh = meshWithoutBlendShape;
                // skinning
                meshWithoutBlendShape.bindposes = _BindPoses.ToArray();
            }
            else
            {
                var useBlendShape = op == BlendShapeOperation.Use && BlendShapes.Count > 0;
                if (useBlendShape)
                {
                    Result.Integrated = new MeshInfo();
                    var mesh = CreateMesh(name, Result.Integrated.SubMeshes, null);
                    Result.Integrated.Mesh = mesh;
                    AddBlendShapesToMesh(mesh);
                    // skinning
                    mesh.bindposes = _BindPoses.ToArray();
                }
                else
                {
                    Result.IntegratedNoBlendShape = new MeshInfo();
                    var mesh = CreateMesh(name, Result.IntegratedNoBlendShape.SubMeshes, null);
                    Result.IntegratedNoBlendShape.Mesh = mesh;
                    // skinning
                    mesh.bindposes = _BindPoses.ToArray();
                }
            }
            Result.Bones = _Bones.ToArray();

            return Result;
        }
    }
}
