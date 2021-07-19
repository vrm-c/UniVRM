using System.Collections.Generic;
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
        public GameObject Root => this.gameObject;

        /// <summary>
        /// Runtime resources.
        /// ex. Material, Texture, Mesh.
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

        private readonly List<(SubAssetKey, UnityEngine.Object)> _resources = new List<(SubAssetKey, UnityEngine.Object)>();
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private readonly List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();
        private readonly List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

        public static RuntimeGltfInstance AttachTo(GameObject go, ImporterContext context)
        {
            var loaded = go.AddComponent<RuntimeGltfInstance>();
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
                    case Mesh mesh:
                        loaded._meshes.Add(mesh);
                        break;
                }
            });

            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                loaded._renderers.Add(renderer);

                switch (renderer)
                {
                    case MeshRenderer meshRenderer:
                        loaded._meshRenderers.Add(meshRenderer);
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        loaded._skinnedMeshRenderers.Add(skinnedMeshRenderer);
                        break;
                }
            }

            return loaded;
        }

        public void ShowMeshes()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }

        public void EnableUpdateWhenOffscreen()
        {
            foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.updateWhenOffscreen = true;
            }
        }

        void OnDestroy()
        {
            Debug.Log("UnityResourceDestroyer.OnDestroy");
            foreach (var (_, obj) in _resources)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(obj);
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
                UnityObjectDestoyer.DestroyRuntimeOrEditor(this.gameObject);
            }
        }
    }
}
