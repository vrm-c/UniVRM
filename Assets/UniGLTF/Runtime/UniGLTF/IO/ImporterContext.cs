using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// GLTF importer
    /// </summary>
    public class ImporterContext : IDisposable
    {
        #region Animation
        protected IAnimationImporter m_animationImporter;
        public void SetAnimationImporter(IAnimationImporter animationImporter)
        {
            m_animationImporter = animationImporter;
        }
        public IAnimationImporter AnimationImporter
        {
            get
            {
                if (m_animationImporter == null)
                {
                    m_animationImporter = new RootAnimationImporter();
                }
                return m_animationImporter;
            }
        }

        #endregion

        public ITextureDescriptorGenerator TextureDescriptorGenerator { get; protected set; }
        public IMaterialDescriptorGenerator MaterialDescriptorGenerator { get; protected set; }
        public TextureFactory TextureFactory { get; }
        public MaterialFactory MaterialFactory { get; }
        IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> _externalObjectMap;

        public ImporterContext(
            GltfParser parser,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null)
        {
            Parser = parser;
            TextureDescriptorGenerator = new GltfTextureDescriptorGenerator(Parser);
            MaterialDescriptorGenerator = new GltfMaterialDescriptorGenerator();

            _externalObjectMap = externalObjectMap ?? new Dictionary<SubAssetKey, UnityEngine.Object>();
            textureDeserializer = textureDeserializer ?? new UnityTextureDeserializer();

            TextureFactory = new TextureFactory(textureDeserializer, _externalObjectMap
                .Where(x => x.Value is Texture)
                .ToDictionary(x => x.Key, x => (Texture)x.Value));
            MaterialFactory = new MaterialFactory(_externalObjectMap
                .Where(x => x.Value is Material)
                .ToDictionary(x => x.Key, x => (Material)x.Value));
        }

        #region Source
        public GltfParser Parser { get; }
        public String Json => Parser.Json;
        public glTF GLTF => Parser.GLTF;
        public IStorage Storage => Parser.Storage;
        #endregion

        // configuration

        /// <summary>
        /// GLTF から Unity に変換するときに反転させる軸
        /// </summary>
        public Axes InvertAxis = Axes.Z;

        public static List<string> UnsupportedExtensions = new List<string>
        {
            // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_draco_mesh_compression
            "KHR_draco_mesh_compression",
            // https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_mesh_quantization
            "KHR_mesh_quantization",
        };

        #region Load. Build unity objects
        public virtual async Task LoadAsync(IAwaitCaller awaitCaller = null, Func<string, IDisposable> MeasureTime = null)
        {
            if (awaitCaller == null)
            {
                awaitCaller = new TaskCaller();
            }

            if (MeasureTime == null)
            {
                MeasureTime = new ImporterContextSpeedLog().MeasureTime;
            }

            if (GLTF.extensionsRequired != null)
            {
                var sb = new List<string>();
                foreach (var required in GLTF.extensionsRequired)
                {
                    if (UnsupportedExtensions.Contains(required))
                    {
                        sb.Add(required);
                    }
                }
                if (sb.Any())
                {
                    throw new UniGLTFNotSupportedException(string.Join(", ", sb) + " is not supported");
                }
            }

            using (MeasureTime("LoadTextures"))
            {
                await LoadTexturesAsync();
            }

            using (MeasureTime("LoadMaterials"))
            {
                await LoadMaterialsAsync();
            }

            await LoadGeometryAsync(awaitCaller, MeasureTime);

            using (MeasureTime("AnimationImporter"))
            {
                if (GLTF.animations != null && GLTF.animations.Any())
                {
                    var animation = Root.AddComponent<Animation>();
                    for (int i = 0; i < GLTF.animations.Count; ++i)
                    {
                        var gltfAnimation = GLTF.animations[i];
                        AnimationClip clip = default;
                        if (_externalObjectMap.TryGetValue(new SubAssetKey(typeof(AnimationClip), gltfAnimation.name), out UnityEngine.Object value))
                        {
                            clip = value as AnimationClip;
                        }
                        else
                        {
                            clip = AnimationImporter.Import(GLTF, i, InvertAxis);
                            AnimationClips.Add(clip);
                        }

                        animation.AddClip(clip, clip.name);
                        if (i == 0)
                        {
                            animation.clip = clip;
                        }
                    }
                }
            }

            await OnLoadHierarchy(awaitCaller, MeasureTime);
        }

        protected virtual async Task LoadGeometryAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            var inverter = InvertAxis.Create();

            var meshImporter = new MeshImporter();
            for (int i = 0; i < GLTF.meshes.Count; ++i)
            {
                var index = i;
                using (MeasureTime("ReadMesh"))
                {
                    var x = meshImporter.ReadMesh(GLTF, index, inverter);
                    var y = await BuildMeshAsync(awaitCaller, MeasureTime, x, index);
                    Meshes.Add(y);
                }
            }

            using (MeasureTime("LoadNodes"))
            {
                for (int i = 0; i < GLTF.nodes.Count; i++)
                {
                    Nodes.Add(NodeImporter.ImportNode(GLTF.nodes[i], i).transform);
                }
            }
            await awaitCaller.NextFrame();

            using (MeasureTime("BuildHierarchy"))
            {
                var nodes = new List<NodeImporter.TransformWithSkin>();
                for (int i = 0; i < Nodes.Count; ++i)
                {
                    nodes.Add(NodeImporter.BuildHierarchy(GLTF, i, Nodes, Meshes));
                }

                NodeImporter.FixCoordinate(GLTF, nodes, inverter);

                // skinning
                for (int i = 0; i < nodes.Count; ++i)
                {
                    NodeImporter.SetupSkinning(GLTF, nodes, i, inverter);
                }

                if (Root == null)
                {
                    Root = new GameObject("GLTF");
                }
                if (GLTF.rootnodes != null)
                {
                    // connect root
                    foreach (var x in GLTF.rootnodes)
                    {
                        var t = nodes[x].Transform;
                        t.SetParent(Root.transform, false);
                    }
                }
            }
            await awaitCaller.NextFrame();
        }

        public async Task LoadTexturesAsync()
        {
            var textures = TextureDescriptorGenerator.Get().GetEnumerable();
            foreach (var param in textures)
            {
                var tex = await TextureFactory.GetTextureAsync(param);
            }
        }

        public async Task LoadMaterialsAsync()
        {
            if (Parser.GLTF.materials == null || Parser.GLTF.materials.Count == 0)
            {
                // no material. work around.
                var param = MaterialDescriptorGenerator.Get(Parser, 0);
                var material = await MaterialFactory.LoadAsync(param, TextureFactory.GetTextureAsync);
            }
            else
            {
                for (int i = 0; i < Parser.GLTF.materials.Count; ++i)
                {
                    var param = MaterialDescriptorGenerator.Get(Parser, i);
                    var material = await MaterialFactory.LoadAsync(param, TextureFactory.GetTextureAsync);
                }
            }
        }

        protected virtual Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            // do nothing
            return Task.FromResult<object>(null);
        }

        async Task<MeshWithMaterials> BuildMeshAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime, MeshImporter.MeshContext x, int i)
        {
            using (MeasureTime("BuildMesh"))
            {
                var meshWithMaterials = await MeshImporter.BuildMeshAsync(awaitCaller, MaterialFactory.GetMaterial, x);
                var mesh = meshWithMaterials.Mesh;

                // mesh name
                if (string.IsNullOrEmpty(mesh.name))
                {
                    mesh.name = string.Format("UniGLTF import#{0}", i);
                }
                var originalName = mesh.name;
                for (int j = 1; Meshes.Any(y => y.Mesh.name == mesh.name); ++j)
                {
                    mesh.name = string.Format("{0}({1})", originalName, j);
                }

                return meshWithMaterials;
            }
        }
        #endregion

        #region Imported
        public GameObject Root;
        bool m_ownRoot = true;
        public List<Transform> Nodes = new List<Transform>();

        public List<MeshWithMaterials> Meshes = new List<MeshWithMaterials>();
        public void ShowMeshes()
        {
            foreach (var x in Meshes)
            {
                foreach (var y in x.Renderers)
                {
                    y.enabled = true;
                }
            }
        }
        void RemoveMesh(Mesh mesh)
        {
            var index = Meshes.FindIndex(x => x.Mesh == mesh);
            if (index >= 0)
            {
                Meshes.RemoveAt(index);
            }
        }

        public void EnableUpdateWhenOffscreen()
        {
            foreach (var x in Meshes)
            {
                foreach (var r in x.Renderers)
                {
                    var skinnedMeshRenderer = r as SkinnedMeshRenderer;
                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.updateWhenOffscreen = true;
                    }
                }
            }
        }

        public List<AnimationClip> AnimationClips = new List<AnimationClip>();
        #endregion

        /// <summary>
        /// ImporterContextが所有する UnityEngine.Object を破棄する
        /// </summary>
        public virtual void Dispose()
        {
            Action<UnityEngine.Object> destroy = UnityResourceDestroyer.DestroyResource();

            foreach (var x in AnimationClips)
            {
#if VRM_DEVELOP
                // Debug.Log($"Destroy {x}");
#endif
                destroy(x);
            }
            AnimationClips.Clear();

            foreach (var x in Meshes)
            {
#if VRM_DEVELOP
                // Debug.Log($"Destroy {x.Mesh}");
#endif
                destroy(x.Mesh);
            }
            Meshes.Clear();

            MaterialFactory?.Dispose();
            TextureFactory?.Dispose();

            if (m_ownRoot && Root != null)
            {
#if VRM_DEVELOP
                // Debug.Log($"Destroy {Root}");
#endif
                destroy(Root);
            }
        }

        /// <summary>
        /// Root ヒエラルキーで使っているリソース
        /// </summary>
        /// <returns></returns>
        public virtual void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            var list = new List<UnityEngine.Object>();
            foreach (var mesh in Meshes)
            {
                if (take(mesh.Mesh))
                {
                    list.Add(mesh.Mesh);
                }
            }
            foreach (var x in list)
            {
                RemoveMesh(x as Mesh);
            }

            TextureFactory.TransferOwnership(take);
            MaterialFactory.TransferOwnership(take);

            list.Clear();
            foreach (var animation in AnimationClips)
            {
                if (take(animation))
                {
                    list.Add(animation);
                }
            }
            foreach (var x in list)
            {
                AnimationClips.Remove(x as AnimationClip);
            }

            if (m_ownRoot && Root != null)
            {
                if (take(Root))
                {
                    // 所有権(Dispose権)
                    m_ownRoot = false;
                }
            }
        }

        /// <summary>
        /// RootにUnityResourceDestroyerをアタッチして、
        /// RootをUnityEngine.Object.Destroyしたときに、
        /// 関連するUnityEngine.Objectを破棄するようにする。
        /// Mesh, Material, Texture, AnimationClip, GameObject の所有者が
        /// ImporterContext から UnityResourceDestroyer に移動する。
        /// ImporterContext.Dispose の対象から外れる。
        /// </summary>
        /// <returns></returns>
        public UnityResourceDestroyer DisposeOnGameObjectDestroyed()
        {
            var destroyer = Root.AddComponent<UnityResourceDestroyer>();
            TransferOwnership(o =>
            {
                destroyer.Resources.Add(o);
                return true;
            });
            return destroyer;
        }
    }
}
