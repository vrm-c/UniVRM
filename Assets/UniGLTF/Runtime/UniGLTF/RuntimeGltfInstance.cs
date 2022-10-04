using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// ImporterContext の Load 結果の GltfModel
    ///
    /// Runtime でモデルを Destory したときに関連リソース(Texture, Material...などの UnityEngine.Object)を自動的に Destroy する。
    /// </summary>
    public class RuntimeGltfInstance : MonoBehaviour, IResponsibilityForDestroyObjects
    {
        /// <summary>
        /// this is UniGLTF root gameObject
        /// </summary>
        public GameObject Root => (this != null) ? this.gameObject : null;

        /// <summary>
        /// Transforms with gltf node index.
        /// </summary>
        public IReadOnlyList<Transform> Nodes => _nodes;

        /// <summary>
        /// Transform states on load.
        /// </summary>
        public IReadOnlyDictionary<Transform, TransformState> InitialTransformStates => _initialTransformStates;

        /// <summary>
        /// Runtime resources.
        /// ex. Material, Texture, AnimationClip, Mesh.
        /// </summary>
        public IReadOnlyList<(SubAssetKey, UnityEngine.Object)> RuntimeResources => _resources;

        /// <summary>
        /// Materials.
        /// </summary>
        public IReadOnlyList<Material> Materials => _materials;

        /// <summary>
        /// Textures.
        /// </summary>
        public IReadOnlyList<Texture> Textures => _textures;

        /// <summary>
        /// Animation Clips.
        /// </summary>
        public IReadOnlyList<AnimationClip> AnimationClips => _animationClips;

        /// <summary>
        /// Meshes.
        /// </summary>
        public IReadOnlyList<Mesh> Meshes => _meshes;

        /// <summary>
        /// Renderers.
        /// ex. MeshRenderer, SkinnedMeshRenderer.
        /// </summary>
        public IReadOnlyList<Renderer> Renderers => _renderers;

        /// <summary>
        /// Mesh Renderers.
        /// </summary>
        public IReadOnlyList<MeshRenderer> MeshRenderers => _meshRenderers;

        /// <summary>
        /// Skinned Mesh Renderers.
        /// </summary>
        public IReadOnlyList<SkinnedMeshRenderer> SkinnedMeshRenderers => _skinnedMeshRenderers;

        /// <summary>
        /// ShowMeshes の対象になる Renderer。
        /// Destroy対象とは無関係なので、自由に操作して OK。
        /// </summary>
        /// <typeparam name="Renderer"></typeparam>
        /// <returns></returns>
        public IList<Renderer> VisibleRenderers => _visibleRenderers;

        private readonly List<Transform> _nodes = new List<Transform>();
        private readonly Dictionary<Transform, TransformState> _initialTransformStates = new Dictionary<Transform, TransformState>();
        private readonly List<(SubAssetKey, UnityEngine.Object)> _resources = new List<(SubAssetKey, UnityEngine.Object)>();
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly List<AnimationClip> _animationClips = new List<AnimationClip>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private readonly List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();
        private readonly List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

        private readonly List<Renderer> _visibleRenderers = new List<Renderer>();

        public static RuntimeGltfInstance AttachTo(GameObject go, ImporterContext context)
        {
            var loaded = go.AddComponent<RuntimeGltfInstance>();

            foreach (var node in context.Nodes)
            {
                // Maintain index order.
                loaded._nodes.Add(node);
                loaded._initialTransformStates.Add(node, new TransformState(node));
            }

            context.TransferOwnership((k, o) =>
            {
                if (o == null) return;

                loaded._resources.Add((k, o));


                switch (o)
                {
                    case Material material:
                        loaded._materials.Add(material);
                        break;
                    case Texture texture:
                        loaded._textures.Add(texture);
                        break;
                    case AnimationClip animationClip:
                        loaded._animationClips.Add(animationClip);
                        break;
                    case Mesh mesh:
                        loaded._meshes.Add(mesh);
                        break;
                }
            });

            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                loaded.AddRenderer(renderer);
            }

            return loaded;
        }

        public void AddRenderer(Renderer renderer)
        {
            _renderers.Add(renderer);

            VisibleRenderers.Add(renderer);

            switch (renderer)
            {
                case MeshRenderer meshRenderer:
                    _meshRenderers.Add(meshRenderer);
                    break;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    _skinnedMeshRenderers.Add(skinnedMeshRenderer);
                    break;
            }
        }

        public void ShowMeshes()
        {
            foreach (var r in VisibleRenderers)
            {
                r.enabled = true;
            }
        }

        public void EnableUpdateWhenOffscreen()
        {
            foreach (var skinnedMeshRenderer in SkinnedMeshRenderers)
            {
                skinnedMeshRenderer.updateWhenOffscreen = true;
            }
        }

        void OnDestroy()
        {
            Debug.Log("UnityResourceDestroyer.OnDestroy");
            foreach (var (_, obj) in _resources)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(obj);
            }
        }

        public void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            foreach (var (key, x) in _resources.ToArray())
            {
                take(key, x);
                _resources.Remove((key, x));
            }
        }

        public void Dispose()
        {
            if (this != null && this.gameObject != null)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(this.gameObject);
            }
        }
    }
}
