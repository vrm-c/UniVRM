using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
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

        static IEnumerable<Transform> Traverse(Transform parent)
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

        public void CopyTo(GameObject _dst, Dictionary<Transform, Transform> map)
        {
            var dst = _dst.AddComponent<VRMFirstPerson>();
            dst.FirstPersonBone = map[FirstPersonBone];
            dst.FirstPersonOffset = FirstPersonOffset;
            dst.Renderers = Renderers.Select(x =>
            {
                var renderer = map[x.Renderer.transform].GetComponent<Renderer>();
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

        private void Reset()
        {
            SetDefault();
            TraverseRenderers();
        }

        public void TraverseRenderers(VRMImporterContext context = null)
        {
            var rendererComponents = transform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in rendererComponents)
            {
                Renderers.Add(new RendererFirstPersonFlags
                {
                    Renderer = renderer,
                    FirstPersonFlag = context == null
                        ? FirstPersonFlag.Auto
                        : GetFirstPersonFlag(context, renderer)
                });
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

            foreach (var x in context.GLTF.extensions.VRM.firstPerson.meshAnnotations)
            {
                if (x.mesh == index)
                {
                    return CacheEnum.TryParseOrDefault<FirstPersonFlag>(x.firstPersonFlag, true);
                }
            }

            return FirstPersonFlag.Auto;
        }

        /// <summary>
        /// ヘッドレスモデルを作成した場合に返す
        /// </summary>
        Mesh CreateHeadlessModel(Renderer _renderer, Transform EraseRoot)
        {
            {
                var renderer = _renderer as SkinnedMeshRenderer;
                if (renderer != null)
                {
                    return CreateHeadlessModelForSkinnedMeshRenderer(renderer, EraseRoot);
                }
            }


            {
                var renderer = _renderer as MeshRenderer;
                if (renderer != null)
                {
                    CreateHeadlessModelForMeshRenderer(renderer, EraseRoot);
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

        private static void CreateHeadlessModelForMeshRenderer(MeshRenderer renderer, Transform eraseRoot)
        {
            if (renderer.transform.Ancestors().Any(x => x == eraseRoot))
            {
                // 祖先に削除ボーンが居る
                SetupLayers();
                renderer.gameObject.layer = THIRDPERSON_ONLY_LAYER;
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
        private static Mesh CreateHeadlessModelForSkinnedMeshRenderer(SkinnedMeshRenderer renderer, Transform eraseRoot)
        {
            SetupLayers();
            var bones = renderer.bones;

            var eraseBones = bones.Select((x, i) =>
            {
                // 祖先に削除対象が存在するか
                bool erase = x.Ancestor().Any(y => y == eraseRoot);
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
            renderer.gameObject.layer = THIRDPERSON_ONLY_LAYER;

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
        /// 配下のモデルのレイヤー設定など
        /// </summary>
        public void Setup()
        {
            SetupLayers();
            if (m_done) return;
            m_done = true;
            foreach (var x in Renderers)
            {
                switch (x.FirstPersonFlag)
                {
                    case FirstPersonFlag.Auto:
                        {
                            var headlessMesh = CreateHeadlessModel(x.Renderer, FirstPersonBone);
                            if (headlessMesh != null)
                            {
                                m_headlessMeshes.Add(headlessMesh);
                            }
                        }
                        break;

                    case FirstPersonFlag.FirstPersonOnly:
                        x.Renderer.gameObject.layer = FIRSTPERSON_ONLY_LAYER;
                        break;

                    case FirstPersonFlag.ThirdPersonOnly:
                        x.Renderer.gameObject.layer = THIRDPERSON_ONLY_LAYER;
                        break;

                    case FirstPersonFlag.Both:
                        //x.Renderer.gameObject.layer = 0;
                        break;
                }
            }
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
