using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
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
    public class Vrm10MeshUtility
    {
        /// <summary>
        /// GameObject 名が重複している場合にリネームする。
        /// 最初に実行(Avatar生成時のエラーを回避？)
        /// </summary>
        public bool ForceUniqueName = false;

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

        MeshIntegrationGroup GetOrCreateGroup(string name)
        {
            foreach (var g in MeshIntegrationGroups)
            {
                if (g.Name == name)
                {
                    return g;
                }
            }
            MeshIntegrationGroups.Add(new MeshIntegrationGroup
            {
                Name = name,
            });
            return MeshIntegrationGroups.Last();
        }

        public void IntegrateFirstPerson(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            var vrm1 = root.GetComponent<Vrm10Instance>();
            if (vrm1 == null)
            {
                return;
            }
            var vrmObject = vrm1.Vrm;
            if (vrmObject == null)
            {
                return;
            }
            var fp = vrmObject.FirstPerson;
            if (fp == null)
            {
                return;
            }
            foreach (var a in fp.Renderers)
            {
                var g = GetOrCreateGroup(a.FirstPersonFlag.ToString());
                g.Renderers.Add(a.GetRenderer(root.transform));
            }
        }

        void RemoveComponent<T>(T c) where T : Component
        {
            var t = c.transform;
            if (Application.isPlaying)
            {
                GameObject.Destroy(c);
            }
            else
            {
                GameObject.DestroyImmediate(c);
            }

            if (t.childCount == 0)
            {
                var list = t.GetComponents<Component>();
                // Debug.Log($"{list[0].GetType().Name}");
                if (list.Length == 1 && list[0] == t)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(t.gameObject);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(t.gameObject);
                    }
                }
            }
        }

        public void Process(GameObject go)
        {
            // TODO unpack prefab

            if (ForceUniqueName)
            {
                // TODO: UNDO            
                throw new NotImplementedException();

                // 必用？
                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            // 正規化されたヒエラルキーを作る
            if (FreezeBlendShape || FreezeRotation || FreezeScaling)
            {
                // TODO: UNDO            
                var (normalized, boneMap) = BoneNormalizer.NormalizeHierarchyFreezeMesh(go,
                    removeScaling: FreezeScaling,
                    removeRotation: FreezeRotation,
                    freezeBlendShape: FreezeBlendShape);

                // TODO: update: spring
                // TODO: update: constraint
                // TODO: update: firstPoint offset
                // write back normalized transform to boneMap
                BoneNormalizer.WriteBackResult(go, normalized, boneMap);
                if (Application.isPlaying)
                {
                    GameObject.Destroy(normalized);
                }
                else
                {
                    GameObject.DestroyImmediate(normalized);
                }

                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            {
                // TODO: UNDO            
                foreach (var group in MeshIntegrationGroups)
                {
                    var result = MeshIntegrator.Integrate(group, SplitByBlendShape
                        ? MeshIntegrator.BlendShapeOperation.Split
                        : MeshIntegrator.BlendShapeOperation.None);

                    foreach (var r in result.SourceMeshRenderers)
                    {
                        RemoveComponent(r);
                    }
                    foreach (var r in result.SourceSkinnedMeshRenderers)
                    {
                        RemoveComponent(r);
                    }

                    result.AddIntegratedRendererTo(go);
                }
            }
        }
    }
}
