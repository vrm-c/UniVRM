using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.XR;


namespace VRM
{
    public class VrmMeshUtility : UniGLTF.MeshUtility.GltfMeshUtility
    {
        bool _generateFirstPerson = false;
        public override IEnumerable<UniGLTF.MeshUtility.MeshIntegrationGroup> CopyInstantiate(GameObject go, GameObject instance)
        {
            _generateFirstPerson = false;

            var copy = base.CopyInstantiate(go, instance);
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
                            Name = FirstPersonFlag.ThirdPersonOnly.ToString(),
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

        protected override bool
         TryIntegrate(
            GameObject empty,
            UniGLTF.MeshUtility.MeshIntegrationGroup group,
            out (UniGLTF.MeshUtility.MeshIntegrationResult, GameObject[]) resultAndAdded)
        {
            if (!base.TryIntegrate(empty, group, out resultAndAdded))
            {
                resultAndAdded = default;
                return false;
            }

            var (result, newGo) = resultAndAdded;
            if (_generateFirstPerson && group.Name == nameof(FirstPersonFlag.Auto))
            {
                // Mesh 統合の後処理
                // FirstPerson == "auto" の場合に                
                // 頭部の無いモデルを追加で作成する
                Debug.Log("generateFirstPerson");
                if (result.Integrated.Mesh != null)
                {
                    // BlendShape 有り
                    _ProcessFirstPerson(_vrmInstance.FirstPersonBone, result.Integrated.IntegratedRenderer);
                }
                if (result.IntegratedNoBlendShape.Mesh != null)
                {
                    // BlendShape 無しの方
                    _ProcessFirstPerson(_vrmInstance.FirstPersonBone, result.IntegratedNoBlendShape.IntegratedRenderer);
                }
            }
            return true;
        }

        private void _ProcessFirstPerson(Transform firstPersonBone, SkinnedMeshRenderer smr)
        {
            var mesh = _vrmInstance.ProcessFirstPerson(firstPersonBone, smr);
            if (mesh != null)
            {
                smr.sharedMesh = mesh;
                smr.name = "auto.headless";
            }
            else
            {
                Debug.LogWarning("no result");
            }
        }

        VRMFirstPerson _vrmInstance = null;
        /// <summary>
        /// glTF に比べて Humanoid や FirstPerson の処理が追加される
        /// </summary>
        public override (List<UniGLTF.MeshUtility.MeshIntegrationResult>, List<GameObject>) Process(
            GameObject target, IEnumerable<UniGLTF.MeshUtility.MeshIntegrationGroup> copyGroup)
        {
            _vrmInstance = target.GetComponentOrThrow<VRMFirstPerson>();

            // TODO: update: spring
            // TODO: update: constraint
            // TODO: update: firstPerson offset
            var (list, newList) = base.Process(target, copyGroup);

            if (FreezeMesh)
            {
                Avatar newAvatar = null;
                if (target.TryGetComponent<Animator>(out var animator))
                {
                    newAvatar = AvatarDescription.RecreateAvatar(animator);
                    // ??? clear old avatar ???
                    var t = animator.gameObject;
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(animator);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(animator);
                    }
                }

                target.AddComponent<Animator>().avatar = newAvatar;
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

            if (root.TryGetComponent<VRMFirstPerson>(out var vrm0))
            {
                foreach (var a in vrm0.Renderers)
                {
                    var g = _GetOrCreateGroup(a.FirstPersonFlag.ToString());
                    g.Renderers.Add(a.Renderer);
                }

                var orphan = root.GetComponentsInChildren<Renderer>().Where(x => !_HasRenderer(x)).ToArray();
                if (orphan.Length > 0)
                {
                    var g = _GetOrCreateGroup("both");
                    g.Renderers.AddRange(orphan);
                }
            }
            else
            {
                return;
            }
        }
    }
}