using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UniGLTF.Utils;
using UnityEngine;


namespace VRM
{
    public class VRMFirstPerson : MonoBehaviour
    {
        // If no layer names are set, use the default layer IDs.
        // Otherwise use the two Unity layers called "VRMFirstPersonOnly" and "VRMThirdPersonOnly".
        public static bool TriedSetupLayer = false;
        public static int FIRSTPERSON_ONLY_LAYER = 9;
        public static int THIRDPERSON_ONLY_LAYER = 10;

        [SerializeField]
        public Transform FirstPersonBone;

        [SerializeField]
        public Vector3 FirstPersonOffset;

        [Serializable]
        public struct RendererFirstPersonFlags
        {
            public Renderer Renderer;
            public FirstPersonFlag FirstPersonFlag;
            public Mesh SharedMesh
            {
                get
                {
                    var renderer = Renderer as SkinnedMeshRenderer;
                    if (renderer != null)
                    {
                        return renderer.sharedMesh;
                    }

                    var filter = Renderer.GetComponent<MeshFilter>();
                    if (filter != null)
                    {
                        return filter.sharedMesh;
                    }

                    return null;
                }
            }
        }

        [SerializeField]
        public List<RendererFirstPersonFlags> Renderers = new List<RendererFirstPersonFlags>();

        public void CopyTo(GameObject _dst, Dictionary<Transform, Transform> map)
        {
            var dst = _dst.AddComponent<VRMFirstPerson>();
            dst.FirstPersonBone = map[FirstPersonBone];
            dst.FirstPersonOffset = FirstPersonOffset;
            dst.Renderers = Renderers
            .Where(x =>
            {
                if (x.Renderer == null || x.Renderer.transform == null)
                {
                    Debug.LogWarning("[VRMFirstPerson] Renderer is null", this);
                    return false;
                }
                if (!map.ContainsKey(x.Renderer.transform))
                {
                    Debug.LogWarning("[VRMFirstPerson] Cannot copy. Not found ?", this);
                    return false;
                }
                return true;
            })
            .Select(x =>
            {
                var mapped = map[x.Renderer.transform];
                var renderer = mapped.GetComponent<Renderer>();
                return new VRMFirstPerson.RendererFirstPersonFlags
                {
                    Renderer = renderer,
                    FirstPersonFlag = x.FirstPersonFlag,
                };
            }).ToList();
        }

        public void SetDefault()
        {
            FirstPersonOffset = new Vector3(0, 0.06f, 0);
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                FirstPersonBone = animator.GetBoneTransform(HumanBodyBones.Head);
            }
        }

        public void Reset()
        {
            SetDefault();
            TraverseRenderers();
        }

        public void TraverseRenderers(VRMImporterContext context = null)
        {
            Renderers.Clear();

            var rendererComponents = transform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in rendererComponents)
            {
                // renderer が !enabled/!activeSelf なのがロード中なのか否か区別がつかないような気がするので
                // チェックしない。
                // if(!renderer.enabled)
                // {
                //     continue;
                // }
                var flags = new RendererFirstPersonFlags
                {
                    Renderer = renderer,
                    FirstPersonFlag = context == null
                        ? FirstPersonFlag.Auto
                        : GetFirstPersonFlag(context, renderer)
                };
                Renderers.Add(flags);
            }
        }

        static FirstPersonFlag GetFirstPersonFlag(VRMImporterContext context, Renderer r)
        {
            var mesh = r.transform.GetSharedMesh();
            if (mesh == null)
            {
                return FirstPersonFlag.Auto;
            }

            var index = context.Meshes.FindIndex(x => x.Mesh == mesh);
            if (index == -1)
            {
                return FirstPersonFlag.Auto;
            }

            foreach (var x in context.VRM.firstPerson.meshAnnotations)
            {
                if (x.mesh == index)
                {
                    return CachedEnum.ParseOrDefault<FirstPersonFlag>(x.firstPersonFlag, true);
                }
            }

            return FirstPersonFlag.Auto;
        }

        /// <summary>
        /// ヘッドレスモデルを作成した場合に返す
        /// </summary>
        Mesh CreateHeadlessModel(Renderer _renderer, Transform EraseRoot, SetVisibilityFunc setVisibility)
        {
            {
                var renderer = _renderer as SkinnedMeshRenderer;
                if (renderer != null)
                {
                    return CreateHeadlessModelForSkinnedMeshRenderer(renderer, EraseRoot, setVisibility);
                }
            }


            {
                var renderer = _renderer as MeshRenderer;
                if (renderer != null)
                {
                    CreateHeadlessModelForMeshRenderer(renderer, EraseRoot, setVisibility);
                    return null;
                }
            }

            // ここには来ない
            return null;
        }

        public static void SetupLayers()
        {
            if (!TriedSetupLayer)
            {
                TriedSetupLayer = true;
                int layer = LayerMask.NameToLayer("VRMFirstPersonOnly");
                FIRSTPERSON_ONLY_LAYER = (layer == -1) ? FIRSTPERSON_ONLY_LAYER : layer;
                layer = LayerMask.NameToLayer("VRMThirdPersonOnly");
                THIRDPERSON_ONLY_LAYER = (layer == -1) ? THIRDPERSON_ONLY_LAYER : layer;
            }
        }

        private static void CreateHeadlessModelForMeshRenderer(MeshRenderer renderer, Transform eraseRoot, SetVisibilityFunc setVisibility)
        {
            if (renderer.transform.Ancestors().Any(x => x == eraseRoot))
            {
                // 祖先に削除ボーンが居る
                setVisibility(renderer, false, true);
            }
            else
            {
                // 特に変更しない => 両方表示
            }
        }

        /// <summary>
        /// ヘッドレスモデルを作成する。
        ///
        /// 以下の場合は作成しない。
        ///
        /// * 削除対象が無い場合
        /// * 全部削除対象の場合
        ///
        /// </summary>
        private static Mesh CreateHeadlessModelForSkinnedMeshRenderer(SkinnedMeshRenderer renderer, Transform eraseRoot, SetVisibilityFunc setVisibility)
        {
            var bones = renderer.bones;

            var eraseBones = bones.Select((x, i) =>
            {
                // 祖先に削除対象が存在するか
                bool erase = x.Ancestors().Any(y => y == eraseRoot);
                return new
                {
                    i,
                    erase,
                };
            })
            .Where(x => x.erase)
            .Select(x => x.i)
            .ToArray()
            ;
            if (eraseBones.Length == 0)
            {
                // 削除対象が存在しない
                return null;
            }

            // 元のメッシュを三人称に変更(自分からは見えない)
            setVisibility(renderer, false, true);

            // 削除対象のボーンに対するウェイトを保持する三角形を除外して、一人称用のモデルを複製する
            var headlessMesh = BoneMeshEraser.CreateErasedMesh(renderer.sharedMesh, eraseBones);
            if (headlessMesh.triangles.Length == 0)
            {
                // 一人称用のmeshには描画すべき部分が無い(全部削除された)
                UnityEngine.Object.Destroy(headlessMesh);
                return null;
            }

            // 一人称用のモデルのセットアップ
            var go = new GameObject("_headless_" + renderer.name);
            go.layer = FIRSTPERSON_ONLY_LAYER;
            go.transform.SetParent(renderer.transform, false);
            var erased = go.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = headlessMesh;
            erased.sharedMaterials = renderer.sharedMaterials;
            erased.bones = bones;
            erased.rootBone = renderer.rootBone;
            erased.updateWhenOffscreen = true;
            return headlessMesh;
        }

        bool m_done;

        List<Mesh> m_headlessMeshes = new List<Mesh>();

        /// <summary>
        /// Set target renderer visibility
        /// 
        /// https://github.com/vrm-c/UniVRM/issues/633#issuecomment-758454045
        /// 
        /// </summary>
        /// <param name="renderer">Target renderer. Player avatar or other</param>
        /// <param name="firstPerson">visibility in HMD camera</param>
        /// <param name="thirdPerson">other camera visibility</param>
        public delegate void SetVisibilityFunc(Renderer renderer, bool firstPerson, bool thirdPerson);

        [Obsolete("Use SetVisibilityFunc")]
        public delegate void SetVisiblityFunc(Renderer renderer, bool firstPerson, bool thirdPerson);

        /// <summary>
        /// Default implementation.
        /// Threre are 4 cases.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="firstPerson"></param>
        /// <param name="thirdPerson"></param>
        public static void SetVisibility(Renderer renderer, bool firstPerson, bool thirdPerson)
        {
            SetupLayers();

            if (firstPerson && thirdPerson)
            {
                // both             
                // do nothing
            }
            else if (firstPerson)
            {
                // only first person
                renderer.gameObject.layer = FIRSTPERSON_ONLY_LAYER;
            }
            else if (thirdPerson)
            {
                // only third person
                renderer.gameObject.layer = THIRDPERSON_ONLY_LAYER;
            }
            else
            {
                // invisible
                renderer.enabled = false;
            }
        }

        [Obsolete("Use SetVisibility")]
        public static void SetVisiblity(Renderer renderer, bool firstPerson, bool thirdPerson) =>
            SetVisibility(renderer, firstPerson, thirdPerson);

        public void Setup()
        {
            // same as v0.63.2
            Setup(true, SetVisibility);
        }

        /// <summary>
        /// from v0.64.0
        /// </summary>
        /// <param name="isSelf"></param>
        public void Setup(bool isSelf, SetVisibilityFunc setVisibility)
        {
            if (m_done) return;
            m_done = true;

            if (isSelf)
            {
                // self avatar
                foreach (var x in Renderers)
                {
                    switch (x.FirstPersonFlag)
                    {
                        case FirstPersonFlag.Auto:
                            {
                                var headlessMesh = CreateHeadlessModel(x.Renderer, FirstPersonBone, setVisibility);
                                if (headlessMesh != null)
                                {
                                    m_headlessMeshes.Add(headlessMesh);
                                }
                            }
                            break;

                        case FirstPersonFlag.FirstPersonOnly:
                            setVisibility(x.Renderer, true, false);
                            break;

                        case FirstPersonFlag.ThirdPersonOnly:
                            setVisibility(x.Renderer, false, true);
                            break;

                        case FirstPersonFlag.Both:
                            setVisibility(x.Renderer, true, true);
                            break;
                    }
                }
            }
            else
            {
                // other avatar
                foreach (var x in Renderers)
                {
                    switch (x.FirstPersonFlag)
                    {
                        case FirstPersonFlag.FirstPersonOnly:
                            setVisibility(x.Renderer, false, false);
                            break;

                        case FirstPersonFlag.Auto:
                        // => Same as Both
                        case FirstPersonFlag.Both:
                        case FirstPersonFlag.ThirdPersonOnly:
                            setVisibility(x.Renderer, true, true);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// for MeshUtility interface
        /// </summary>
        public Mesh ProcessFirstPerson(Transform firstPersonBone, SkinnedMeshRenderer smr)
        {
            SetVisibilityFunc dummy = (Renderer renderer, bool firstPerson, bool thirdPerson) =>
            {
            };
            return CreateHeadlessModel(smr, FirstPersonBone, dummy);
        }

        void OnDestroy()
        {
            foreach (var mesh in m_headlessMeshes)
            {
                if (mesh != null)
                {
                    // Debug.LogFormat("[VRMFirstPerson] OnDestroy: {0}", mesh);
                    UnityEngine.Object.Destroy(mesh);
                }
            }
            m_headlessMeshes.Clear();
        }
    }
}
