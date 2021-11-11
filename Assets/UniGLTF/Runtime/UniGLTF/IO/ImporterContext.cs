using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Profiling;
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
        public AnimationClipFactory AnimationClipFactory { get; }
        public IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> ExternalObjectMap;

        /// <summary>
        /// UnityObject の 生成(LoadAsync) と 破棄(Dispose) を行う。
        /// LoadAsync が成功した場合、返り値(RuntimeGltfInstance) に破棄する責務を移動させる。
        /// </summary>
        /// <param name="data">Jsonからデシリアライズされた GLTF 情報など</param>
        /// <param name="externalObjectMap">外部オブジェクトのリスト(主にScriptedImporterのRemapで使う)</param>
        /// <param name="textureDeserializer">Textureロードをカスタマイズする</param>
        /// <param name="materialGenerator">Materialロードをカスタマイズする(URP向け)</param>
        public ImporterContext(
            GltfData data,
            IReadOnlyDictionary<SubAssetKey, UnityEngine.Object> externalObjectMap = null,
            ITextureDeserializer textureDeserializer = null,
            IMaterialDescriptorGenerator materialGenerator = null)
        {
            Data = data;
            TextureDescriptorGenerator = new GltfTextureDescriptorGenerator(Data);
            MaterialDescriptorGenerator = materialGenerator ?? new GltfMaterialDescriptorGenerator();

            ExternalObjectMap = externalObjectMap ?? new Dictionary<SubAssetKey, UnityEngine.Object>();
            textureDeserializer = textureDeserializer ?? new UnityTextureDeserializer();

            TextureFactory = new TextureFactory(textureDeserializer, ExternalObjectMap
                .Where(x => x.Value is Texture)
                .ToDictionary(x => x.Key, x => (Texture)x.Value),
                Data.MigrationFlags.IsRoughnessTextureValueSquared);
            MaterialFactory = new MaterialFactory(ExternalObjectMap
                .Where(x => x.Value is Material)
                .ToDictionary(x => x.Key, x => (Material)x.Value));
            AnimationClipFactory = new AnimationClipFactory(ExternalObjectMap
                .Where(x => x.Value is AnimationClip)
                .ToDictionary(x => x.Key, x => (AnimationClip)x.Value));
        }

        #region Source
        public GltfData Data { get; }
        public String Json => Data.Json;
        public glTF GLTF => Data.GLTF;
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
                awaitCaller = new ImmediateCaller();
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
                await LoadTexturesAsync(awaitCaller);
            }

            using (MeasureTime("LoadMaterials"))
            {
                await LoadMaterialsAsync(awaitCaller);
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

        public virtual async Task LoadAnimationAsync(IAwaitCaller awaitCaller)
        {
            if (GLTF.animations != null && GLTF.animations.Any())
            {
                foreach (var (key, gltfAnimation) in Enumerable.Zip(AnimationImporterUtil.EnumerateSubAssetKeys(GLTF), GLTF.animations, (x, y) => (x, y)))
                {
                    await AnimationClipFactory.LoadAnimationClipAsync(key, () =>
                    {
                        var clip = AnimationImporterUtil.ConvertAnimationClip(Data, gltfAnimation, InvertAxis.Create());
                        return Task.FromResult(clip);
                    });
                }

                await awaitCaller.NextFrame();
            }
        }

        /// <summary>
        /// AnimationClips を AnimationComponent に載せる
        /// </summary>
        protected virtual async Task SetupAnimationsAsync(IAwaitCaller awaitCaller)
        {
            if (AnimationClipFactory.LoadedClipKeys.Count == 0) return;

            var animation = Root.AddComponent<Animation>();
            for (var clipIdx = 0; clipIdx < AnimationClipFactory.LoadedClipKeys.Count; ++clipIdx)
            {
                var key = AnimationClipFactory.LoadedClipKeys[clipIdx];
                var clip = AnimationClipFactory.GetAnimationClip(key);
                animation.AddClip(clip, key.Name);

                if (clipIdx == 0)
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
            if (GLTF.meshes.Count > 0)
            {
                for (var i = 0; i < GLTF.meshes.Count; ++i)
                {
                    var index = i;
                    using (MeasureTime("ReadMesh"))
                    {
                        var meshContext = await awaitCaller.Run(() => meshImporter.ReadMesh(Data, index, inverter));
                        var meshWithMaterials = await BuildMeshAsync(awaitCaller, MeasureTime, meshContext, index);
                        Meshes.Add(meshWithMaterials);
                    }
                }

                await awaitCaller.NextFrame();
            }

            if (GLTF.nodes.Count > 0)
            {
                using (MeasureTime("LoadNodes"))
                {
                    Profiler.BeginSample("ImporterContext.LoadNodes");
                    for (var i = 0; i < GLTF.nodes.Count; i++)
                    {
                        Nodes.Add(NodeImporter.ImportNode(GLTF.nodes[i], i).transform);
                    }
                    Profiler.EndSample();
                }

                await awaitCaller.NextFrame();
            }

            using (MeasureTime("BuildHierarchy"))
            {
                var nodes = new List<NodeImporter.TransformWithSkin>();
                if (Nodes.Count > 0)
                {
                    Profiler.BeginSample("NodeImporter.BuildHierarchy");
                    for (var i = 0; i < Nodes.Count; ++i)
                    {
                        nodes.Add(NodeImporter.BuildHierarchy(GLTF, i, Nodes, Meshes));
                    }
                    Profiler.EndSample();

                    await awaitCaller.NextFrame();
                }

                NodeImporter.FixCoordinate(GLTF, nodes, inverter);

                // skinning
                if (nodes.Count > 0)
                {
                    Profiler.BeginSample("NodeImporter.SetupSkinning");
                    for (var i = 0; i < nodes.Count; ++i)
                    {
                        NodeImporter.SetupSkinning(Data, nodes, i, inverter);
                    }
                    Profiler.EndSample();

                    await awaitCaller.NextFrame();
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

        public async Task LoadTexturesAsync(IAwaitCaller awaitCaller = null)
        {
            awaitCaller = awaitCaller ?? new ImmediateCaller();

            var textures = TextureDescriptorGenerator.Get().GetEnumerable();
            foreach (var param in textures)
            {
                var tex = await TextureFactory.GetTextureAsync(param, awaitCaller);
            }
        }

        public async Task LoadMaterialsAsync(IAwaitCaller awaitCaller = null)
        {
            awaitCaller = awaitCaller ?? new ImmediateCaller();

            if (Data.GLTF.materials == null || Data.GLTF.materials.Count == 0)
            {
                // no material. work around.
                var param = MaterialDescriptorGenerator.Get(Data, 0);
                var material = await MaterialFactory.LoadAsync(param, TextureFactory.GetTextureAsync, awaitCaller);
            }
            else
            {
                for (int i = 0; i < Data.GLTF.materials.Count; ++i)
                {
                    var param = MaterialDescriptorGenerator.Get(Data, i);
                    var material = await MaterialFactory.LoadAsync(param, TextureFactory.GetTextureAsync, awaitCaller);
                }
            }
        }

        protected virtual Task OnLoadHierarchy(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            // do nothing
            return Task.FromResult<object>(null);
        }

        async Task<MeshWithMaterials> BuildMeshAsync(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime, MeshContext x, int i)
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
        #endregion

        /// <summary>
        /// ImporterContextが所有する UnityEngine.Object を破棄する
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var x in Meshes)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(x.Mesh);
            }
            Meshes.Clear();

            AnimationClipFactory?.Dispose();
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

            AnimationClipFactory.TransferOwnership(take);
            TextureFactory.TransferOwnership(take);
            MaterialFactory.TransferOwnership(take);
        }
    }
}
