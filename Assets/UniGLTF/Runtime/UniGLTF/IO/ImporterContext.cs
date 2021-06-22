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
    public class ImporterContext : IResponsibilityForDestroyObjects
    {
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
                .ToDictionary(x => x.Key, x => (Texture)x.Value),
                Parser.MigrationFlags.IsRoughnessTextureValueSquared);
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
        public virtual async Task<RuntimeGltfInstance> LoadAsync(IAwaitCaller awaitCaller = null, Func<string, IDisposable> MeasureTime = null)
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
                await LoadAnimationAsync(awaitCaller);
                await SetupAnimationsAsync(awaitCaller);
            }

            await OnLoadHierarchy(awaitCaller, MeasureTime);

            return RuntimeGltfInstance.AttachTo(Root, this);
        }

        /// <summary>
        /// ImporterContext.AnimationClips に AnimationClip を読み込むところまでが責務
        /// </summary>
        /// <param name="awaitCaller"></param>
        /// <returns></returns>
        protected virtual async Task LoadAnimationAsync(IAwaitCaller awaitCaller)
        {
            if (GLTF.animations != null && GLTF.animations.Any())
            {
                foreach (var (key, gltfAnimation) in Enumerable.Zip(AnimationImporterUtil.EnumerateSubAssetKeys(GLTF), GLTF.animations, (x, y) => (x, y)))
                {
                    AnimationInfo animation = default;
                    if (_externalObjectMap.TryGetValue(key, out UnityEngine.Object value))
                    {
                        animation = new AnimationInfo(key, value as AnimationClip, true);
                    }
                    else
                    {
                        animation = new AnimationInfo(key, AnimationImporterUtil.ConvertAnimationClip(GLTF, gltfAnimation, InvertAxis.Create()), false);
                    }
                    AnimationClips.Add(animation);
                }

                await awaitCaller.NextFrame();
            }
        }

        /// <summary>
        /// AnimationClips を AnimationComponent に載せる
        /// </summary>
        protected virtual async Task SetupAnimationsAsync(IAwaitCaller awaitCaller)
        {
            if (AnimationClips.Count == 0)
            {
                return;
            }
            var animation = Root.AddComponent<Animation>();
            for (int i = 0; i < AnimationClips.Count; ++i)
            {
                var clip = AnimationClips[i].Clip;
                animation.AddClip(clip, clip.name);
                if (i == 0)
                {
                    animation.clip = clip;
                }
            }
            await awaitCaller.NextFrame();
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
        protected GameObject Root;
        public List<Transform> Nodes = new List<Transform>();

        public List<MeshWithMaterials> Meshes = new List<MeshWithMaterials>();

        public struct AnimationInfo
        {
            public readonly SubAssetKey Key;
            public readonly AnimationClip Clip;
            public readonly bool IsExternal;

            public AnimationInfo(SubAssetKey key, AnimationClip clip, bool isExternal)
            {
                Key = key;
                Clip = clip;
                IsExternal = isExternal;
            }
        }

        public List<AnimationInfo> AnimationClips = new List<AnimationInfo>();
        #endregion

        /// <summary>
        /// ImporterContextが所有する UnityEngine.Object を破棄する
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var info in AnimationClips)
            {
                if (info.IsExternal)
                {
                    // external は削除不要
                    continue;
                }
                UnityObjectDestoyer.DestroyRuntimeOrEditor(info.Clip);
            }
            AnimationClips.Clear();

            foreach (var x in Meshes)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(x.Mesh);
            }
            Meshes.Clear();

            MaterialFactory?.Dispose();
            TextureFactory?.Dispose();
        }

        /// <summary>
        /// Root ヒエラルキーで使っているリソース
        /// </summary>
        /// <returns></returns>
        public virtual void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            foreach (var mesh in Meshes.ToArray())
            {
                take(SubAssetKey.Create(mesh.Mesh), mesh.Mesh);
                Meshes.Remove(mesh);
            }

            TextureFactory.TransferOwnership(take);
            MaterialFactory.TransferOwnership(take);

            foreach (var info in AnimationClips)
            {
                if (info.IsExternal)
                {
                    // external は削除しないので不要
                    continue;
                }
                take(info.Key, info.Clip);
            }

            AnimationClips.Clear();
        }
    }
}
