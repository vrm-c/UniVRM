using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.MeshUtility;
using UnityEngine;

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
            .Distinct()
            .ToArray();
            return eraseBones;
        }

        public static async Task<Mesh> CreateErasedMeshAsync(SkinnedMeshRenderer smr,
            Transform firstPersonBone,
            IAwaitCaller awaitCaller)
        {
            var eraseBones = GetBonesThatHasAncestor(smr, firstPersonBone);
            if (eraseBones.Any())
            {
                return await BoneMeshEraser.CreateErasedMeshAsync(smr.sharedMesh, eraseBones, awaitCaller);
            }
            else
            {
                return null;
            }
        }

        // <summary>
        // 頭部を取り除いたモデルを複製する
        // </summary>
        // <parameter>renderer: 元になるSkinnedMeshRenderer</parameter>
        // <parameter>eraseBones: 削除対象になるボーンのindex</parameter>
        public async static Task<SkinnedMeshRenderer> CreateHeadlessMeshAsync(SkinnedMeshRenderer renderer,
            Transform firstPersonBone, IAwaitCaller awaitCaller)
        {
            var mesh = await CreateErasedMeshAsync(renderer, firstPersonBone, awaitCaller);
            if (mesh != null)
            {
                var go = new GameObject("_headless_" + renderer.name);
                var erased = go.AddComponent<SkinnedMeshRenderer>();
                erased.enabled = false; // hide
                erased.sharedMesh = mesh;
                erased.sharedMaterials = renderer.sharedMaterials;
                erased.bones = renderer.bones;
                erased.rootBone = renderer.rootBone;
                return erased;
            }
            else
            {
                // 削除対象が含まれないので何もしない
                return null;
            }
        }

        bool m_done;

        async Task SetupSelfRendererAsync(GameObject go, UniGLTF.RuntimeGltfInstance runtime,
            Transform firstPersonBone, RendererFirstPersonFlags x,
            (int FirstPersonOnly, int ThirdPersonOnly) layer, IAwaitCaller awaitCaller = null)
        {
            switch (x.FirstPersonFlag)
            {
                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto:
                    {
                        if (x.GetRenderer(go.transform) is SkinnedMeshRenderer smr)
                        {
                            // 頭を取り除いた複製モデルを作成し、１人称用にする
                            var headless = await CreateHeadlessMeshAsync(smr, firstPersonBone, awaitCaller);
                            if (headless != null)
                            {
                                // オリジナルのモデルを３人称用にする                                
                                smr.gameObject.layer = layer.ThirdPersonOnly;

                                headless.gameObject.layer = layer.FirstPersonOnly;
                                headless.transform.SetParent(smr.transform, false);
                                if (runtime != null)
                                {
                                    runtime.AddResource(headless.sharedMesh);
                                    runtime.AddRenderer(headless);
                                }
                            }
                            else
                            {
                                // ヘッドレスを作成しなかった場合は何もしない => both と同じ
                            }
                        }
                        else if (x.GetRenderer(go.transform) is MeshRenderer mr)
                        {
                            if (mr.transform.Ancestors().Any(y => y == firstPersonBone))
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
        }

        /// <summary>
        /// Each renderer is set according to the first person flag. 
        /// If the flag is `auto`, headless mesh creation will be performed.
        /// Creating a headless mesh(Renderer) is a heavy process and can be done in threads.
        /// </summary>
        /// <param name="go">The target model root</param>
        /// <param name="isSelf">The target model is the VR user himself</param>
        /// <param name="firstPersonOnlyLayer">layer VRMFirstPersonOnly or 9</param>
        /// <param name="thirdPersonOnlyLayer">layer VRMThirdPersonOnly ir 10</param>
        /// <param name="awaitCaller">Headless mesh creation task scheduler. By default, creation is immediate</param>
        /// <returns></returns>
        public async Task SetupAsync(GameObject go, IAwaitCaller awaitCaller, bool isSelf = true, int? firstPersonOnlyLayer = default, int? thirdPersonOnlyLayer = default)
        {
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            var layer = (
                Vrm10FirstPersonLayerSettings.GetFirstPersonOnlyLayer(firstPersonOnlyLayer),
                Vrm10FirstPersonLayerSettings.GetThirdPersonOnlyLayer(thirdPersonOnlyLayer));

            if (m_done)
            {
                return;
            }
            m_done = true;

            var runtime = go.GetComponentOrThrow<RuntimeGltfInstance>();
            var vrmInstance = go.GetComponentOrThrow<Vrm10Instance>();
            // NOTE: This bone must be referenced by renderers instead of the control rig bone.
            var firstPersonBone = vrmInstance.Humanoid.Head;

            var used = new HashSet<string>();
            foreach (var x in Renderers)
            {
                if (!used.Add(x.Renderer))
                {
                    // 同じ対象が複数回現れた
                    UniGLTFLogger.Warning($"VRM10ObjectFirstPerson.SetupAsync: duplicated {x.Renderer}");
                    continue;
                }

                if (isSelf)
                {
                    await SetupSelfRendererAsync(go, runtime, firstPersonBone, x, layer, awaitCaller);
                }
                else
                {
                    switch (x.FirstPersonFlag)
                    {
                        case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.firstPersonOnly:
                            if (x.GetRenderer(go.transform) is Renderer r)
                            {
                                // invisible
                                r.enabled = false;
                                runtime.VisibleRenderers.Remove(r);
                            }
                            break;

                        case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto:
                        // => Same as Both
                        case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.both:
                        case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.thirdPersonOnly:
                            // do nothing
                            break;
                    }
                }
            }
        }
    }
}
