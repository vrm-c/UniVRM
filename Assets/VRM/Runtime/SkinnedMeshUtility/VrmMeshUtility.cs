using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;


namespace VRM
{
    public class VrmMeshUtility : UniGLTF.MeshUtility.GltfMeshUtility
    {
        bool _generateFirstPerson = false;
        protected override List<UniGLTF.MeshUtility.MeshIntegrationGroup> CopyMeshIntegrationGroups()
        {
            var copy = new List<UniGLTF.MeshUtility.MeshIntegrationGroup>();
            _generateFirstPerson = false;
            if (GenerateMeshForFirstPersonAuto)
            {
                foreach (var g in MeshIntegrationGroups)
                {
                    if (g.Name == "auto")
                    {
                        _generateFirstPerson = true;
                        // 元のメッシュを三人称に変更
                        copy.Add(new UniGLTF.MeshUtility.MeshIntegrationGroup
                        {
                            Name = FirstPersonFlag.ThirdPersonOnly.ToString(),
                            Renderers = g.Renderers.ToList(),
                        });
                    }
                    copy.Add(g);
                }
            }
            else
            {
                copy.AddRange(MeshIntegrationGroups);
            }
            return copy;
        }

        protected override
        (UniGLTF.MeshUtility.MeshIntegrationResult, GameObject[]) Integrate(
            GameObject empty,
            UniGLTF.MeshUtility.MeshIntegrationGroup group)
        {
            var (result, newList) = base.Integrate(empty, group);

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

            return (result, newList);
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
        public override (List<UniGLTF.MeshUtility.MeshIntegrationResult>, List<GameObject>) Process(GameObject go)
        {
            _vrmInstance = go.GetComponent<VRMFirstPerson>();
            if (_vrmInstance == null)
            {
                throw new ArgumentException();
            }

            if (ForceUniqueName)
            {
                throw new NotImplementedException();

                // 必用？
                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            // TODO: update: spring
            // TODO: update: constraint
            // TODO: update: firstPerson offset
            var (list, newList) = base.Process(go);

            if (FreezeBlendShape || FreezeRotation || FreezeScaling)
            {
                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            return (list, newList);
        }

        public override void UpdateMeshIntegrationGroups(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            var vrm1 = root.GetComponent<VRMFirstPerson>();
            if (vrm1 == null)
            {
                return;
            }
            foreach (var a in vrm1.Renderers)
            {
                var g = _GetOrCreateGroup(a.FirstPersonFlag.ToString());
                g.Renderers.Add(a.Renderer);
            }
        }
    }
}