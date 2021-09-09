using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF.MeshUtility;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    [Serializable]
    public class VRM10ObjectFirstPerson
    {
        [SerializeField]
        public List<RendererFirstPersonFlags> Renderers = new List<RendererFirstPersonFlags>();
        public void SetDefault(Transform root)
        {
            Renderers.Clear();

            var renderers = root.GetComponentsInChildren<Renderer>();
            var paths = renderers.Select(x => x.transform.RelativePathFrom(root)).ToArray();
            foreach (var path in paths)
            {
                Renderers.Add(new RendererFirstPersonFlags
                {
                    FirstPersonFlag = UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto,
                    Renderer = path,
                });
            }
        }

        static int[] GetBonesThatHasAncestor(SkinnedMeshRenderer smr, Transform ancestor)
        {
            var eraseBones = smr.bones
            .Where(x => x.Ancestor().Any(y => y == ancestor))
            .Select(x => Array.IndexOf(smr.bones, x))
            .ToArray();
            return eraseBones;
        }

        // <summary>
        // 頭部を取り除いたモデルを複製する
        // </summary>
        // <parameter>renderer: 元になるSkinnedMeshRenderer</parameter>
        // <parameter>eraseBones: 削除対象になるボーンのindex</parameter>
        private async static Task<SkinnedMeshRenderer> CreateHeadlessMeshAsync(SkinnedMeshRenderer renderer, int[] eraseBones, IAwaitCaller awaitCaller)
        {
            var mesh = await BoneMeshEraser.CreateErasedMeshAsync(renderer.sharedMesh, eraseBones, awaitCaller);

            var go = new GameObject("_headless_" + renderer.name);
            var erased = go.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = mesh;
            erased.sharedMaterials = renderer.sharedMaterials;
            erased.bones = renderer.bones;
            erased.rootBone = renderer.rootBone;
            erased.updateWhenOffscreen = true;

            return erased;
        }

        bool m_done;

        async Task<SkinnedMeshRenderer> SetupRendererAsync(GameObject go, Transform FirstPersonBone, RendererFirstPersonFlags x,
            (int FirstPersonOnly, int ThirdPersonOnly) layer, IAwaitCaller awaitCaller = null)
        {
            switch (x.FirstPersonFlag)
            {
                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto:
                    {
                        if (x.GetRenderer(go.transform) is SkinnedMeshRenderer smr)
                        {
                            var eraseBones = GetBonesThatHasAncestor(smr, FirstPersonBone);
                            if (eraseBones.Any())
                            {
                                // オリジナルのモデルを３人称用にする                                
                                smr.gameObject.layer = layer.ThirdPersonOnly;

                                // 頭を取り除いた複製モデルを作成し、１人称用にする
                                var headless = await CreateHeadlessMeshAsync(smr, eraseBones, awaitCaller);
                                headless.enabled = false;
                                headless.gameObject.layer = layer.FirstPersonOnly;
                                headless.transform.SetParent(smr.transform, false);
                                return headless;
                            }
                            else
                            {
                                // 削除対象が含まれないので何もしない
                            }
                        }
                        else if (x.GetRenderer(go.transform) is MeshRenderer mr)
                        {
                            if (mr.transform.Ancestors().Any(y => y == FirstPersonBone))
                            {
                                // 頭の子孫なので１人称では非表示に
                                mr.gameObject.layer = layer.ThirdPersonOnly;
                            }
                            else
                            {
                                // 特に変更しない => 両方表示
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    break;

                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.firstPersonOnly:
                    // １人称のカメラでだけ描画されるようにする
                    x.GetRenderer(go.transform).gameObject.layer = layer.FirstPersonOnly;
                    break;

                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.thirdPersonOnly:
                    // ３人称のカメラでだけ描画されるようにする
                    x.GetRenderer(go.transform).gameObject.layer = layer.ThirdPersonOnly;
                    break;

                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.both:
                    // 特に何もしない。すべてのカメラで描画される
                    break;
            }

            return null;
        }

        /// <summary>
        /// Setup first person
        /// 
        /// * SetupLayers
        /// * Each renderer is set according to the first person flag. If the flag is `auto`, headless mesh creation will be performed. It's a heavy process.
        /// * If visible is false, the created headless mesh will be hidden.
        /// 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="firstPersonOnlyLayer"></param>
        /// <param name="thirdPersonOnlyLayer"></param>
        /// <param name="awaitCaller"></param>
        /// <returns></returns>
        public async Task<List<SkinnedMeshRenderer>> SetupAsync(GameObject go, int? firstPersonOnlyLayer = default, int? thirdPersonOnlyLayer = default, IAwaitCaller awaitCaller = default)
        {
            if (awaitCaller == null)
            {
                awaitCaller = new ImmediateCaller();
            }

            var layer = (
                Vrm10FirstPersonLayerSettings.GetFirstPersonOnlyLayer(firstPersonOnlyLayer),
                Vrm10FirstPersonLayerSettings.GetThirdPersonOnlyLayer(thirdPersonOnlyLayer));

            var created = new List<SkinnedMeshRenderer>();
            if (m_done)
            {
                return created;
            }
            m_done = true;

            var FirstPersonBone = go.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            foreach (var x in Renderers)
            {
                var renderer = await SetupRendererAsync(go, FirstPersonBone, x, layer, awaitCaller);
                if (renderer)
                {
                    created.Add(renderer);
                }
            }
            return created;
        }
    }
}
