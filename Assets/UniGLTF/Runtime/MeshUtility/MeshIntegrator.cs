using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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

        public class BlendShape
        {
            public string Name;
            public List<Vector3> Positions = new List<Vector3>();
            public List<Vector3> Normals = new List<Vector3>();
            public List<Vector3> Tangents = new List<Vector3>();

            public BlendShape(string name)
            {
                Name = name;
            }

            public void Fill(int count)
            {
                var size = count - Positions.Count;
                if (size < 0)
                {
                    throw new Exception();
                }
                Positions.AddRange(Enumerable.Repeat(Vector3.zero, size));
                Normals.AddRange(Enumerable.Repeat(Vector3.zero, size));
                Tangents.AddRange(Enumerable.Repeat(Vector3.zero, size));
            }
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
            foreach (var x in BlendShapes)
            {
                mesh.AddBlendShapeFrame(x.Name, 100.0f,
                    x.Positions.ToArray(),
                    x.Normals.ToArray(),
                    x.Tangents.ToArray());
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

            var bone = renderer.transform;
            if (bone == null)
            {
                Debug.LogWarningFormat("{0} is root gameobject.", bone.name);
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

            var vertexOffset = Positions.Count;
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
                var indices = mesh.GetIndices(i).Select(x => x + vertexOffset);
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

                var blendShape = GetOrCreateBlendShape(mesh.GetBlendShapeName(i), vertexOffset);
                blendShape.Positions.AddRange(positions);
                blendShape.Normals.AddRange(normals);
                blendShape.Tangents.AddRange(tangents);
            }
            foreach (var blendShape in BlendShapes)
            {
                blendShape.Fill(Positions.Count);
            }
        }

        BlendShape GetOrCreateBlendShape(string name, int vertexOffset)
        {
            BlendShape found = null;
            foreach (var blendshape in BlendShapes)
            {
                if (blendshape.Name == name)
                {
                    found = blendshape;
                    break;
                }
            }
            if (found == null)
            {
                found = new BlendShape(name);
                BlendShapes.Add(found);
            }

            found.Fill(vertexOffset);

            return found;
        }

        public static bool TryIntegrate(MeshIntegrationGroup group, BlendShapeOperation op,
        out MeshIntegrationResult result)
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
            result = integrator.Integrate(group.Name, op);
            if (result.Integrated != null || result.IntegratedNoBlendShape != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        delegate bool TriangleFilter(int i0, int i1, int i2);

        static int[] GetFilteredIndices(List<int> indices, TriangleFilter filter)
        {
            if (filter == null)
            {
                return indices.ToArray();
            }

            var filtered = new List<int>();
            for (int i = 0; i < indices.Count; i += 3)
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
            return filtered.ToArray();
        }

        Mesh CreateMesh(string name, List<DrawCount> dst, TriangleFilter filter)
        {
            var mesh = new Mesh();
            mesh.name = name;
            if (Positions.Count > ushort.MaxValue)
            {
                UniGLTFLogger.Log($"exceed 65535 vertices: {Positions.Count}");
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.vertices = Positions.ToArray();
            mesh.normals = Normals.ToArray();
            mesh.uv = UV.ToArray();
            if (Tangents != null && Tangents.Count == Positions.Count)
            {
                mesh.tangents = Tangents.ToArray();
            }
            if (BoneWeights != null && BoneWeights.Count == Positions.Count)
            {
                mesh.boneWeights = BoneWeights.ToArray();
            }

            int subMeshCount = 0;
            foreach (var submesh in SubMeshes)
            {
                var indices = GetFilteredIndices(submesh.Indices, filter);
                if (indices.Length > 0)
                {
                    mesh.subMeshCount = (subMeshCount + 1);
                    mesh.SetIndices(indices, MeshTopology.Triangles, subMeshCount++);
                    dst.Add(new DrawCount
                    {
                        Count = indices.Length,
                        Material = submesh.Material,
                    });
                }
            }

            return mesh;
        }

        MeshIntegrationResult Integrate(string name, BlendShapeOperation op)
        {
            if (_Bones.Count != _BindPoses.Count)
            {
                throw new ArgumentException();
            }

            var splitter = new TriangleSeparator(Positions.Count);
            if (op == BlendShapeOperation.Split)
            {
                foreach (var blendShape in BlendShapes)
                {
                    splitter.CheckPositions(blendShape.Positions);
                }
            }

            if (splitter.ShouldSplit)
            {
                //
                // has BlendShape
                //
                Result.Integrated = new MeshInfo();
                var mesh = CreateMesh(name, Result.Integrated.SubMeshes,
                    splitter.TriangleHasBlendShape);
                Result.Integrated.Mesh = mesh;
                AddBlendShapesToMesh(mesh);
                // skinning
                mesh.bindposes = _BindPoses.ToArray();

                //
                // no BlendShape
                //
                Result.IntegratedNoBlendShape = new MeshInfo();
                var meshWithoutBlendShape = CreateMesh(name + ".no_blendshape", Result.IntegratedNoBlendShape.SubMeshes,
                    splitter.TriangleHasNotBlendShape);
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
