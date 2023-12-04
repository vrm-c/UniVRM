using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    /// <summary>
    /// - Freeze
    /// - Integration
    /// - Split
    /// 
    /// - Implement runtime logic => Process a hierarchy in scene. Do not process prefab.
    /// - Implement undo
    ///
    /// </summary>
    public class GltfMeshUtility
    {
        /// <summary>
        /// Same as VRM-0 normalization
        /// - Mesh
        /// - Node
        /// - InverseBindMatrices
        /// </summary>
        public bool FreezeBlendShape = false;

        /// <summary>
        /// Same as VRM-0 normalization
        /// - Mesh
        /// - Node
        /// - InverseBindMatrices
        /// </summary>
        public bool FreezeScaling = true;

        /// <summary>
        /// Same as VRM-0 normalization
        /// - Mesh
        /// - Node
        /// - InverseBindMatrices
        /// </summary>
        public bool FreezeRotation = false;

        public List<MeshIntegrationGroup> MeshIntegrationGroups = new List<MeshIntegrationGroup>();

        /// <summary>
        /// Create a headless model and solve VRM.FirstPersonFlag.Auto
        /// </summary>
        public bool GenerateMeshForFirstPersonAuto = false;

        /// <summary>
        /// Split into having and not having BlendShape
        /// </summary>
        public bool SplitByBlendShape = false;

        protected UniGLTF.MeshUtility.MeshIntegrationGroup _GetOrCreateGroup(string name)
        {
            foreach (var g in MeshIntegrationGroups)
            {
                if (g.Name == name)
                {
                    return g;
                }
            }
            MeshIntegrationGroups.Add(new UniGLTF.MeshUtility.MeshIntegrationGroup
            {
                Name = name,
            });
            return MeshIntegrationGroups.Last();
        }

        public virtual void UpdateMeshIntegrationGroups(GameObject root)
        {
            MeshIntegrationGroups.Clear();
            if (root == null)
            {
                return;
            }
            var group = _GetOrCreateGroup("all mesh");
            group.Renderers.AddRange(root.GetComponentsInChildren<Renderer>());
        }

        public void IntegrateAll(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            MeshIntegrationGroups.Add(new MeshIntegrationGroup
            {
                Name = "ALL",
                Renderers = root.GetComponentsInChildren<Renderer>().ToList(),
            });
        }

        static GameObject GetOrCreateEmpty(GameObject go, string name)
        {
            foreach (var child in go.transform.GetChildren())
            {
                if (child.name == name
                 && child.localPosition == Vector3.zero
                 && child.localScale == Vector3.one
                 && child.localRotation == Quaternion.identity)
                {
                    return child.gameObject;
                }
            }
            var empty = new GameObject(name);
            empty.transform.SetParent(go.transform, false);
            return empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go">MeshIntegrationGroup を作ったとき root</param>
        /// <param name="instance">go が prefab だった場合に instance されたもの</param>
        /// <returns></returns>
        public virtual IEnumerable<MeshIntegrationGroup> CopyInstantiate(GameObject go, GameObject instance)
        {
            if (instance == null)
            {
                foreach (var g in MeshIntegrationGroups)
                {
                    yield return g;
                }
            }
            else
            {
                foreach (var g in MeshIntegrationGroups)
                {
                    yield return g.CopyInstantiate(go, instance);
                }
            }
        }

        public virtual (List<MeshIntegrationResult>, List<GameObject>) Process(
            GameObject target, IEnumerable<MeshIntegrationGroup> groupCopy)
        {
            if (FreezeBlendShape || FreezeRotation || FreezeScaling)
            {
                // MeshをBakeする
                var newMesh = BoneNormalizer.NormalizeHierarchyFreezeMesh(target, FreezeRotation);

                // - ヒエラルキーから回転・拡縮を除去する
                // - BakeされたMeshで置き換える
                // - bindPoses を再計算する
                BoneNormalizer.Replace(target, newMesh, FreezeRotation, FreezeScaling);
            }

            var newList = new List<GameObject>();

            var empty = GetOrCreateEmpty(target, "mesh");

            var results = new List<MeshIntegrationResult>();
            foreach (var group in groupCopy)
            {
                if (TryIntegrate(empty, group, out var resultAndAdded))
                {
                    var (result, newGo) = resultAndAdded;
                    results.Add(result);
                    newList.AddRange(newGo);
                }
            }

            return (results, newList);
        }

        public void clear(List<MeshIntegrationResult> results)
        {
            // 用が済んだ 統合前 の renderer を削除する
            foreach (var result in results)
            {
                foreach (var r in result.SourceMeshRenderers)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(r.gameObject.GetComponent<MeshFilter>());
                        GameObject.Destroy(r);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(r.gameObject.GetComponent<MeshFilter>());
                        GameObject.DestroyImmediate(r);
                    }
                }
                foreach (var r in result.SourceSkinnedMeshRenderers)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(r);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(r, true);
                    }
                }
            }

            MeshIntegrationGroups.Clear();
        }

        protected virtual bool TryIntegrate(GameObject empty,
            MeshIntegrationGroup group, out (MeshIntegrationResult, GameObject[]) resultAndAdded)
        {
            if (MeshIntegrator.TryIntegrate(group, SplitByBlendShape
                ? MeshIntegrator.BlendShapeOperation.Split
                : MeshIntegrator.BlendShapeOperation.Use, out var result))
            {
                var newGo = result.AddIntegratedRendererTo(empty).ToArray();
                resultAndAdded = (result, newGo);
                return true;
            }

            resultAndAdded = default;
            return false;
        }
    }
}
