using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    public class Vrm10MeshUtility : UniGLTF.MeshUtility.GltfMeshUtility
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
                            Name = UniGLTF.Extensions.VRMC_vrm.FirstPersonType.thirdPersonOnly.ToString(),
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

        protected override UniGLTF.MeshUtility.MeshIntegrationResult Integrate(List<GameObject> newGo,
            GameObject empty,
            List<UniGLTF.MeshUtility.MeshIntegrationResult> results,
            UniGLTF.MeshUtility.MeshIntegrationGroup group)
        {
            var result = base.Integrate(newGo, empty, results, group);

            if (_generateFirstPerson && group.Name == "auto")
            {
                Debug.Log("generateFirstPerson");
                if (result.Integrated.Mesh != null)
                {
                    _ProcessFirstPerson(_vrmInstance, result.Integrated);
                }
                if (result.IntegratedNoBlendShape.Mesh != null)
                {
                    _ProcessFirstPerson(_vrmInstance, result.IntegratedNoBlendShape);
                }
            }

            return result;
        }

        Vrm10Instance _vrmInstance = null;
        /// <summary>
        /// glTF に比べて Humanoid や FirstPerson の処理が追加される
        /// </summary>
        public override List<GameObject> Process(GameObject go)
        {
            _vrmInstance = go.GetComponent<Vrm10Instance>();
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
            var list = base.Process(go);

            if (FreezeBlendShape || FreezeRotation || FreezeScaling)
            {
                var animator = go.GetComponent<Animator>();
                var newAvatar = AvatarDescription.RecreateAvatar(animator);
                animator.avatar = newAvatar;
            }

            return list;
        }

        void _ProcessFirstPerson(Vrm10Instance vrmInstance, UniGLTF.MeshUtility.MeshInfo info)
        {
            var firstPersonBone = vrmInstance.Humanoid.Head;
            var task = VRM10ObjectFirstPerson.CreateErasedMeshAsync(
                info.IntegratedRenderer,
                firstPersonBone,
                new VRMShaders.ImmediateCaller());
            task.Wait();

            if (task.Result != null)
            {
                info.IntegratedRenderer.sharedMesh = task.Result;
                info.IntegratedRenderer.name = "auto.headless";
            }
            else
            {
                Debug.LogWarning("no result");
            }
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
                var g = _GetOrCreateGroup(a.FirstPersonFlag.ToString());
                g.Renderers.Add(a.GetRenderer(root.transform));
            }
        }

        UniGLTF.MeshUtility.MeshIntegrationGroup _GetOrCreateGroup(string name)
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
    }
}