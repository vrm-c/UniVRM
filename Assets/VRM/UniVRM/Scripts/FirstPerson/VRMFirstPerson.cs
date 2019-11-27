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
            Renderers = Traverse(transform)
                .Select(x => x.GetComponent<Renderer>())
                .Where(x => x != null)
                .Select(x => new RendererFirstPersonFlags
                {
                    Renderer = x,
                    FirstPersonFlag = context == null
                        ? FirstPersonFlag.Auto
                        : GetFirstPersonFlag(context, x)
                })
                .ToList()
                ;
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

            foreach(var x in context.GLTF.extensions.VRM.firstPerson.meshAnnotations)
            {
                if (x.mesh == index)
                {
                    return EnumUtil.TryParseOrDefault<FirstPersonFlag>(x.firstPersonFlag);
                }
            }

            return FirstPersonFlag.Auto;
        }

        void CreateHeadlessModel(Renderer _renderer, Transform EraseRoot)
        {
            {
                var renderer = _renderer as SkinnedMeshRenderer;
                if (renderer != null)
                {
                    CreateHeadlessModelForSkinnedMeshRenderer(renderer, EraseRoot);
                    return;
                }
            }


            {
                var renderer = _renderer as MeshRenderer;
                if (renderer != null)
                {
                    CreateHeadlessModelForMeshRenderer(renderer, EraseRoot);
                    return;
                }
            }

            // ここには来ない
        }
		
        public static void SetupLayers()
        {
            if (!TriedSetupLayer) {
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

        private static void CreateHeadlessModelForSkinnedMeshRenderer(SkinnedMeshRenderer renderer, Transform eraseRoot)
        {
            SetupLayers();
            renderer.gameObject.layer = THIRDPERSON_ONLY_LAYER;

            var go = new GameObject("_headless_" + renderer.name);
            go.layer = FIRSTPERSON_ONLY_LAYER;
            go.transform.SetParent(renderer.transform, false);

            var m_eraseBones = renderer.bones.Select(x =>
            {
                var eb = new BoneMeshEraser.EraseBone
                {
                    Bone = x,
                };

                if (eraseRoot != null)
                {
                    // 首の子孫を消去
                    if (eb.Bone.Ancestor().Any(y => y == eraseRoot))
                    {
                        //Debug.LogFormat("erase {0}", x);
                        eb.Erase = true;
                    }
                }

                return eb;
            })
            .ToArray();

            var bones = renderer.bones;
            var eraseBones = m_eraseBones
                .Where(x => x.Erase)
                .Select(x => bones.IndexOf(x.Bone))
                .ToArray();

            var mesh = BoneMeshEraser.CreateErasedMesh(renderer.sharedMesh, eraseBones);

            var erased = go.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = mesh;
            erased.sharedMaterials = renderer.sharedMaterials;
            erased.bones = renderer.bones;
            erased.rootBone = renderer.rootBone;
            erased.updateWhenOffscreen = true;
        }

        bool m_done;

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
                        CreateHeadlessModel(x.Renderer, FirstPersonBone);
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
    }
}
