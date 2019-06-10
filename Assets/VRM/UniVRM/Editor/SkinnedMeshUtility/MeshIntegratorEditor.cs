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
    public static class MeshIntegratorEditor
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

            // レンダラから情報を集める
            var integrator = new MeshIntegrator();
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
