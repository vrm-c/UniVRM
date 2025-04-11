using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniHumanoid;
using UnityEngine;


namespace UniVRM10
{
    public class Vrm10MeshUtility : UniGLTF.MeshUtility.GltfMeshUtility
    {
        public Vrm10MeshUtility()
        {
            FreezeMesh = true;
            FreezeMeshKeepRotation = true;
            FreezeMeshCurrentBlendShapeWeight = true;
        }

        bool _generateFirstPerson = false;
        public override IEnumerable<UniGLTF.MeshUtility.MeshIntegrationGroup> CopyInstantiate(GameObject go, GameObject instance)
        {
            var copy = base.CopyInstantiate(go, instance);
            _generateFirstPerson = false;
            if (GenerateMeshForFirstPersonAuto)
            {
                foreach (var g in copy)
                {
                    if (g.Name == "auto")
                    {
                        _generateFirstPerson = true;
                        // 元のメッシュを三人称に変更
                        yield return new UniGLTF.MeshUtility.MeshIntegrationGroup
                        {
                            Name = UniGLTF.Extensions.VRMC_vrm.FirstPersonType.thirdPersonOnly.ToString(),
                            IntegrationType = UniGLTF.MeshUtility.MeshIntegrationGroup.MeshIntegrationTypes.ThirdPersonOnly,
                            Renderers = g.Renderers.ToList(),
                        };
                    }
                    yield return g;
                }
            }
            else
            {
                foreach (var g in copy)
                {
                    yield return g;
                }
            }
        }

        protected override
         bool TryIntegrate(
            GameObject empty,
            UniGLTF.MeshUtility.MeshIntegrationGroup group,
            out (UniGLTF.MeshUtility.MeshIntegrationResult, GameObject[]) resultAndAdded)
        {
            if (!base.TryIntegrate(empty, group, out resultAndAdded))
            {
                return false;
            }
            var (result, newList) = resultAndAdded;

            if (_generateFirstPerson && group.Name == nameof(UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto))
            {
                // Mesh 統合の後処理
                // FirstPerson == "auto" の場合に                
                // 頭部の無いモデルを追加で作成する
                UniGLTFLogger.Log("generateFirstPerson");
                if (result.Integrated.Mesh != null)
                {
                    // BlendShape 有り
                    _ProcessFirstPerson(_vrmInstance.Humanoid.Head, result.Integrated.IntegratedRenderer);
                }
                if (result.IntegratedNoBlendShape.Mesh != null)
                {
                    // BlendShape 無しの方
                    _ProcessFirstPerson(_vrmInstance.Humanoid.Head, result.IntegratedNoBlendShape.IntegratedRenderer);
                }
            }
            return true;
        }

        private void _ProcessFirstPerson(Transform firstPersonBone, SkinnedMeshRenderer smr)
        {
            var task = VRM10ObjectFirstPerson.CreateErasedMeshAsync(
                smr,
                firstPersonBone,
                new ImmediateCaller());
            task.Wait();
            var mesh = task.Result;
            if (mesh != null)
            {
                smr.sharedMesh = mesh;
                smr.name = "auto.headless";
            }
            else
            {
                UniGLTFLogger.Warning("no result");
            }
        }

        Vrm10Instance _vrmInstance = null;
        /// <summary>
        /// glTF に比べて Humanoid や FirstPerson の処理が追加される
        /// </summary>
        public override (List<UniGLTF.MeshUtility.MeshIntegrationResult>, List<GameObject>) Process(
            GameObject target, IEnumerable<UniGLTF.MeshUtility.MeshIntegrationGroup> groupCopy)
        {
            _vrmInstance = target.GetComponentOrThrow<Vrm10Instance>();

            // TODO: update: spring
            // TODO: update: constraint
            // TODO: update: firstPerson offset
            var (list, newList) = base.Process(target, groupCopy);

            if (FreezeMesh)
            {
                if (target.TryGetComponent<Animator>(out var animator))
                {
                    HumanoidLoader.RebuildHumanAvatar(animator);
                }
            }

            return (list, newList);
        }

        public override void UpdateMeshIntegrationGroups(GameObject root)
        {
            MeshIntegrationGroups.Clear();
            if (root == null)
            {
                return;
            }
            var vrm1 = root.GetComponentOrNull<Vrm10Instance>();
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
                var g = _GetOrCreateGroup(a.FirstPersonFlag.ToString());
                g.Renderers.Add(a.GetRenderer(root.transform));
            }

            var orphan = root.GetComponentsInChildren<Renderer>().Where(x => !_HasRenderer(x)).ToArray();
            if (orphan.Length > 0)
            {
                var g = _GetOrCreateGroup("both");
                g.Renderers.AddRange(orphan);
            }
        }
    }
}