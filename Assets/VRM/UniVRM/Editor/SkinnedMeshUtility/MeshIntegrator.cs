using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    /// <summary>
    /// 複数のメッシュをまとめる
    /// </summary>
    [DisallowMultipleComponent]
    public static class MeshIntegrator
    {
        const string MENU_KEY = SkinnedMeshUtility.MENU_KEY + "MeshIntegrator";
        const string ASSET_SUFFIX = ".mesh.asset";
        const string ASSET_WITH_BLENDSHAPE_SUFFIX = ".blendshape.asset";

        [MenuItem(MENU_KEY, true, SkinnedMeshUtility.MENU_PRIORITY)]
        private static bool ExportValidate()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        [MenuItem(MENU_KEY, false, SkinnedMeshUtility.MENU_PRIORITY)]
        private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;

            Integrate(go);
        }

        public static SkinnedMeshRenderer Integrate(GameObject go)
        {
            var without_blendshape = _Integrate(go, false);
            if (without_blendshape == null)
            {
                return null;

            }

            // save mesh to Assets
            var assetPath = string.Format("{0}{1}", go.name, ASSET_SUFFIX);
#if UNITY_2018_2_OR_NEWER
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
            var prefab = PrefabUtility.GetPrefabParent(go);
#endif
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}_{2}{3}",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    go.name,
                    ASSET_SUFFIX
                    );
            }
            else
            {
                assetPath = string.Format("Assets/{0}{1}", go.name, ASSET_SUFFIX);
            }

            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(without_blendshape.sharedMesh, assetPath);
            return without_blendshape;
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

        class Integrator
        {
//            public List<SkinnedMeshRenderer> Renderers { get; private set; }
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
                        bs.Positions = Positions.ToArray();
                        bs.Normals = Normals.ToArray();
                        bs.Tangents = Tangents.Select(y => (Vector3)y).ToArray();
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

            public Integrator()
            {
//                Renderers = new List<SkinnedMeshRenderer>();

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

//                Renderers.Add(renderer);

                var indexOffset = Positions.Count;
                var boneIndexOffset = Bones.Count;

                Positions.AddRange(mesh.vertices);
                Normals.AddRange(mesh.normals);
                UV.AddRange(mesh.uv);
                Tangents.AddRange(mesh.tangents);

                if (mesh.vertexCount == mesh.boneWeights.Length)
                {
                    BoneWeights.AddRange(mesh.boneWeights.Select(x => AddBoneIndexOffset(x, boneIndexOffset)).ToArray());
                }
                else
                {
                    BoneWeights.AddRange(Enumerable.Range(0, mesh.vertexCount).Select(x => new BoneWeight()).ToArray());
                }

                BindPoses.AddRange(mesh.bindposes);
                Bones.AddRange(renderer.bones);

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

        static IEnumerable<Transform> Traverse(Transform parent)
        {
            if (parent.gameObject.activeSelf)
            {
                yield return parent;

                foreach (Transform child in parent)
                {
                    foreach (var x in Traverse(child))
                    {
                        yield return x;
                    }
                }
            }
        }

        static public IEnumerable<SkinnedMeshRenderer> EnumerateRenderer(Transform root, bool hasBlendShape)
        {
            foreach (var x in Traverse(root))
            {
                var renderer = x.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    if (renderer.sharedMesh != null)
                    {
                        if (renderer.gameObject.activeSelf)
                        {
                            if (renderer.sharedMesh.blendShapeCount > 0 == hasBlendShape)
                            {
                                yield return renderer;
                            }
                        }
                    }
                }
            }
        }

        static IEnumerable<MeshRenderer> EnumerateMeshRenderer(Transform root)
        {
            foreach (var x in Traverse(root))
            {
                var renderer = x.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var filter = x.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null && renderer.gameObject.activeSelf)
                    {
                        yield return renderer;
                    }
                }
            }
        }

        static IEnumerable<Transform> Ancestors(Transform self)
        {
            yield return self;

            if (self.parent != null)
            {
                foreach (var x in Ancestors(self.parent))
                {
                    yield return x;
                }
            }
        }

        static SkinnedMeshRenderer _Integrate(GameObject go, bool hasBlendShape)
        {
            var meshNode = new GameObject();
            if (hasBlendShape)
            {
                meshNode.name = "MeshIntegrator(BlendShape)";
            }
            else
            {
                meshNode.name = "MeshIntegrator";
            }
            meshNode.transform.SetParent(go.transform, false);

            var renderers = EnumerateRenderer(go.transform, hasBlendShape).ToArray();

            // Root objectを選出する
            var root = renderers.Select(x => x.rootBone != null ? x.rootBone : x.transform)
                .Select(x => Ancestors(x).Reverse().ToArray())
                .Aggregate((a, b) =>
                {
                    int i = 0;
                    for(; i<a.Length && i<b.Length; ++i)
                    {
                        if (a[i] != b[i])
                        {
                            break;
                        }
                    }
                    return a.Take(i).ToArray();
                })
                .Last()
                ;
            Debug.LogFormat("root bone: {0}", root.name);

            // レンダラから情報を集める
            var integrator = new Integrator();
            foreach(var x in renderers)
            {
                integrator.Push(x);
            }

            foreach (var meshRenderer in EnumerateMeshRenderer(go.transform))
            {
                integrator.Push(meshRenderer);
            }

            var mesh = new Mesh();
            mesh.name = "integrated";

            if (integrator.Positions.Count > ushort.MaxValue)
            {
#if UNITY_2017_3_OR_NEWER
                Debug.LogFormat("exceed 65535 vertices: {0}", integrator.Positions.Count);
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                throw new NotImplementedException(String.Format("exceed 65535 vertices: {0}", integrator.Positions.Count.ToString()));
#endif
            }

            mesh.vertices = integrator.Positions.ToArray();
            mesh.normals = integrator.Normals.ToArray();
            mesh.uv = integrator.UV.ToArray();
            mesh.tangents = integrator.Tangents.ToArray();
            mesh.boneWeights = integrator.BoneWeights.ToArray();
            mesh.subMeshCount = integrator.SubMeshes.Count;
            for (var i = 0; i < integrator.SubMeshes.Count; ++i)
            {
                mesh.SetIndices(integrator.SubMeshes[i].Indices.ToArray(), MeshTopology.Triangles, i);
            }
            mesh.bindposes = integrator.BindPoses.ToArray();

            if (hasBlendShape)
            {
                integrator.AddBlendShapesToMesh(mesh);
            }

            var integrated = meshNode.AddComponent<SkinnedMeshRenderer>();
            integrated.sharedMesh = mesh;
            integrated.sharedMaterials = integrator.SubMeshes.Select(x => x.Material).ToArray();
            integrated.bones = integrator.Bones.ToArray();

            return integrated;
        }
    }
}
