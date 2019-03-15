using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using DepthFirstScheduler;
using UniJSON;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER)
using System.Threading.Tasks;
#endif


namespace UniGLTF
{
    /// <summary>
    /// GLTF importer
    /// </summary>
    public class ImporterContext: IDisposable
    {
        #region MeasureTime
        bool m_showSpeedLog
#if VRM_DEVELOP
            = true
#endif
            ;
        public bool ShowSpeedLog
        {
            set { m_showSpeedLog = value; }
        }

        public struct KeyElapsed
        {
            public string Key;
            public TimeSpan Elapsed;
            public KeyElapsed(string key, TimeSpan elapsed)
            {
                Key = key;
                Elapsed = elapsed;
            }
        }

        public struct MeasureScope : IDisposable
        {
            Action m_onDispose;
            public MeasureScope(Action onDispose)
            {
                m_onDispose = onDispose;
            }
            public void Dispose()
            {
                m_onDispose();
            }
        }

        public List<KeyElapsed> m_speedReports = new List<KeyElapsed>();

        public IDisposable MeasureTime(string key)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            return new MeasureScope(() =>
            {
                m_speedReports.Add(new KeyElapsed(key, sw.Elapsed));
            });
        }

        public string GetSpeedLog()
        {
            var total = TimeSpan.Zero;

            var sb = new StringBuilder();
            sb.AppendLine("【SpeedLog】");
            foreach (var kv in m_speedReports)
            {
                sb.AppendLine(string.Format("{0}: {1}ms", kv.Key, (int)kv.Elapsed.TotalMilliseconds));
                total += kv.Elapsed;
            }
            sb.AppendLine(string.Format("total: {0}ms", (int)total.TotalMilliseconds));

            return sb.ToString();
        }
        #endregion

        IShaderStore m_shaderStore;
        public IShaderStore ShaderStore
        {
            get
            {
                if (m_shaderStore == null)
                {
                    m_shaderStore = new ShaderStore(this);
                }
                return m_shaderStore;
            }
        }

        IMaterialImporter m_materialImporter;
        protected void SetMaterialImporter(IMaterialImporter importer)
        {
            m_materialImporter = importer;
        }
        public IMaterialImporter MaterialImporter
        {
            get
            {
                if (m_materialImporter == null)
                {
                    m_materialImporter = new MaterialImporter(ShaderStore, this);
                }
                return m_materialImporter;
            }
        }

        public ImporterContext(IShaderStore shaderStore)
        {
            m_shaderStore = shaderStore;
        }

        public ImporterContext(IMaterialImporter materialImporter)
        {
            m_materialImporter = materialImporter;
        }

        public ImporterContext()
        {
        }

        #region Source

        /// <summary>
        /// JSON source
        /// </summary>
        public String Json;

        /// <summary>
        /// GLTF parsed from JSON
        /// </summary>
        public glTF GLTF; // parsed

        public VGltf.Types.Gltf GLTF2;
        public VGltf.ResourcesStore Store;

        public static bool IsGeneratedUniGLTFAndOlderThan(string generatorVersion, int major, int minor)
        {
            if (string.IsNullOrEmpty(generatorVersion)) return false;
            if (generatorVersion == "UniGLTF") return true;
            if (!generatorVersion.StartsWith("UniGLTF-")) return false;

            try
            {
                var index = generatorVersion.IndexOf('.');
                var generatorMajor = int.Parse(generatorVersion.Substring(8, index - 8));
                var generatorMinor = int.Parse(generatorVersion.Substring(index + 1));

                if (generatorMajor < major)
                {
                    return true;
                }
                else
                {
                    if (generatorMinor >= minor)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("{0}: {1}", generatorVersion, ex);
                return false;
            }
        }

        public bool IsGeneratedUniGLTFAndOlder(int major, int minor)
        {
            if (GLTF2 == null) return false;
            if (GLTF2.Asset == null) return false;
            return IsGeneratedUniGLTFAndOlderThan(GLTF2.Asset.Generator, major, minor);
        }

        /// <summary>
        /// URI access
        /// </summary>
        //public IStorage Storage;
        #endregion

        // 1. Parse
        // 2. Setup
        // 3. Load

        #region Parse

        public void Parse(string path)
        {
            Parse(path, File.ReadAllBytes(path));
        }

        public virtual void Parse(string path, byte[] bytes)
        {
            using(var s = new MemoryStream(bytes))
            {
                Parse(path, s);
            }
        }

        /// <summary>
        /// Parse gltf json or Parse json chunk of glb
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public virtual void Parse(string path, Stream s)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".gltf":
                    ParseAsGltf(path, s);
                    break;

#if false
                case ".zip":
                    {
                        var zipArchive = Zip.ZipArchiveStorage.
                        var gltf = zipArchive.Entries.FirstOrDefault(x => x.FileName.ToLower().EndsWith(".gltf"));
                        if (gltf == null)
                        {
                            throw new Exception("no gltf in archive");
                        }
                        var jsonBytes = zipArchive.Extract(gltf);
                        var json = Encoding.UTF8.GetString(jsonBytes);
                        ParseJson(json, zipArchive);
                    }
                    break;
#endif

                case ".glb":
                    ParseAsGlb(s);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Obsolete]
        public void ParseJson(string json, IStorage storage)
        {
            ParseJson(json); // Discard storage...
        }

        [Obsolete]
        public void ParseJson(string json, VGltf.Glb.StoredBuffer buffer = null)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            ParseAsGltf(bytes, new VGltf.ResourceLoaderFromEmbedOnly(), buffer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bytes"></param>
        [Obsolete]
        public void ParseGlb(Byte[] bytes)
        {
            ParseAsGlb(bytes);
        }

        public void ParseAsGltf(string path, Stream s, VGltf.Glb.StoredBuffer buffer = null)
        {
            var loader = new VGltf.ResourceLoaderFromFileStorage(Path.GetDirectoryName(path));
            ParseAsGltf(s, loader, buffer);
        }

        public void ParseAsGltf(byte[] bytes, VGltf.IResourceLoader loader, VGltf.Glb.StoredBuffer buffer = null)
        {
            using(var s = new MemoryStream(bytes))
            {
                ParseAsGltf(s, loader, buffer);
            }
        }

        public void ParseAsGltf(Stream ss, VGltf.IResourceLoader loader, VGltf.Glb.StoredBuffer buffer = null)
        {
            // TODO: Remove Json string loader
            var bytes = new byte[ss.Length];
            ss.Read(bytes, 0, bytes.Length);

            Json = Encoding.UTF8.GetString(bytes);

            using(var s = new MemoryStream(bytes))
            {
                var c = VGltf.GltfContainer.FromGltf(s, buffer);
                SetupGltf(c, loader);
            }
        }

        public void ParseAsGlb(Byte[] bytes)
        {
            using(var s = new MemoryStream(bytes))
            {
                ParseAsGlb(s);
            }
        }

        public void ParseAsGlb(Stream ss)
        {
            // TODO: Remove Json string loader
            var bytes = new byte[ss.Length];
            ss.Read(bytes, 0, bytes.Length);

            var chunks = glbImporter.ParseGlbChanks(bytes);

            if (chunks.Count != 2)
            {
                throw new Exception("unknown chunk count: " + chunks.Count);
            }

            if (chunks[0].ChunkType != GlbChunkType.JSON)
            {
                throw new Exception("chunk 0 is not JSON");
            }

            if (chunks[1].ChunkType != GlbChunkType.BIN)
            {
                throw new Exception("chunk 1 is not BIN");
            }

            var jsonBytes = chunks[0].Bytes.ToArray();
            Json = Encoding.UTF8.GetString(jsonBytes);

            using(var s = new MemoryStream(bytes))
            {
                var c = VGltf.GltfContainer.FromGlb(s);
                SetupGltf(c, new VGltf.ResourceLoaderFromEmbedOnly());
            }
        }

        public bool UseUniJSONParser;

        public virtual void SetupGltf(VGltf.GltfContainer c, VGltf.IResourceLoader loader)
        {
            var schema = VJson.Schema.JsonSchemaAttribute.CreateFromClass<VGltf.Types.Gltf>();
            var ex = VJson.Schema.JsonSchemaExtensions.Validate(schema, c.Gltf);
            if (ex != null)
            {
                Debug.LogWarning("Maybe this model data contains an invalid glTF2.0 format: " + ex);
            }

//            Storage = storage;

            // Use new Json deserializer (TODO: fix performance)
            if (c.Gltf.Asset.Version != "2.0")
            {
                throw new UniGLTFException("unknown gltf version {0}", GLTF2.Asset.Version);
            }

            Store = new VGltf.ResourcesStore(c.Gltf, c.Buffer, loader);
            GLTF2 = c.Gltf;

            // Compatibility
            if (UseUniJSONParser)
            {
                Json.ParseAsJson().Deserialize(ref GLTF);
            }
            else
            {
                GLTF = JsonUtility.FromJson<glTF>(Json);
            }

            AfterSetupHook();
        }

        public virtual void AfterSetupHook()
        {
        }

        // TODO: Remove

        #endregion

        #region Load. Build unity objects
        /// <summary>
        /// ReadAllBytes, Parse, Create GameObject
        /// </summary>
        /// <param name="path">allbytes</param>
        public void Load(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                Load(path, fs);
            }
        }

        /// <summary>
        /// Parse, Create GameObject
        /// </summary>
        /// <param name="path">gltf or glb path</param>
        /// <param name="bytes">allbytes</param>
        public void Load(string path, byte[] bytes)
        {
            using(var s = new MemoryStream(bytes))
            {
                Load(path, s);
            }
        }

        public void Load(string path, Stream s)
        {
            Parse(path, s);
            Load();
            Root.name = Path.GetFileNameWithoutExtension(path);
        }

        public void CreateTextureItems(UnityPath imageBaseDir = default(UnityPath))
        {
            if (m_textures.Any())
            {
                return;
            }

            if (GLTF2.Textures == null)
            {
                return;
            }

            for (int i = 0; i < GLTF2.Textures.Count; ++i)
            {
                TextureItem item = null;
#if UNITY_EDITOR
                int? imageIndex;
                var image = VGltf.Types.GltfExtensions.GetImageByTextureIndex(GLTF2, i, out imageIndex);
                if (image == null) {
                    continue;
                }

                if (imageBaseDir.IsUnderAssetsFolder
                    && !string.IsNullOrEmpty(image.Uri)
                    && !image.Uri.StartsWith("data:")
                    )
                {
                    ///
                    /// required SaveTexturesAsPng or SetTextureBaseDir
                    ///
                    var assetPath = imageBaseDir.Child(image.Uri);
                    var textureName = !string.IsNullOrEmpty(image.Name) ? image.Name : Path.GetFileNameWithoutExtension(image.Uri);
                    item = new TextureItem(i, assetPath, textureName);
                }
                else
#endif
                {
                    item = new TextureItem(i);
                }

                AddTexture(item);
            }
        }

        /// <summary>
        /// Build unity objects from parsed gltf
        /// </summary>
        public void Load()
        {
            var schedulable = LoadAsync();
            schedulable.ExecuteAll();
        }

        [Obsolete("Action<Unit> to Action")]
        public IEnumerator LoadCoroutine(Action<Unit> onLoaded, Action<Exception> onError = null)
        {
            return LoadCoroutine(() => onLoaded(Unit.Default), onError);
        }

        public IEnumerator LoadCoroutine(Action<Exception> onError = null)
        {
            return LoadCoroutine(() => { }, onError);
        }

        public IEnumerator LoadCoroutine(Action onLoaded, Action<Exception> onError = null)
        {
            if (onLoaded == null)
            {
                onLoaded = () => { };
            }

            if (onError == null)
            {
                onError = Debug.LogError;
            }

            var schedulable = LoadAsync();
            foreach (var x in schedulable.GetRoot().Traverse())
            {
                while (true)
                {
                    var status = x.Execute();
                    if (status != ExecutionStatus.Continue)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            onLoaded();
        }

        [Obsolete("Action<Unit> to Action")]
        public void LoadAsync(Action<Unit> onLoaded, Action<Exception> onError = null)
        {
            LoadAsync(() => onLoaded(Unit.Default), onError);
        }

        public void LoadAsync(Action onLoaded, Action<Exception> onError = null)
        {
            if (onError == null)
            {
                onError = Debug.LogError;
            }

            LoadAsync()
                .Subscribe(Scheduler.MainThread,
                _ => onLoaded(),
                onError
                );
        }

#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER)
        public async Task<GameObject> LoadAsyncTask()
        {
            await LoadAsync().ToTask();
            return Root;
        }
#endif

        protected virtual Schedulable<Unit> LoadAsync()
        {
            return
            Schedulable.Create()
                .AddTask(Scheduler.ThreadPool, () =>
                {
                    if (m_textures.Count == 0)
                    {
                        //
                        // runtime
                        //
                        CreateTextureItems();
                    }
                    else
                    {
                        //
                        // already CreateTextures(by assetPostProcessor or editor menu)
                        //
                    }
                })
                .ContinueWithCoroutine(Scheduler.ThreadPool, TexturesProcessOnAnyThread)
                .ContinueWithCoroutine(Scheduler.MainThread, TexturesProcessOnMainThread)
                .ContinueWithCoroutine(Scheduler.MainThread, LoadMaterials)
                .OnExecute(Scheduler.ThreadPool, parent =>
                {
                    // UniGLTF does not support draco
                    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md#conformance
                    if (GLTF2.ExtensionsRequired!=null && GLTF2.ExtensionsRequired.Contains("KHR_draco_mesh_compression"))
                    {
                        throw new UniGLTFNotSupportedException("draco is not supported");
                    }

                    // meshes
                    var meshImporter = new MeshImporter();
                    for (int i = 0; i < (GLTF2.Meshes != null ? GLTF2.Meshes.Count : 0); ++i)
                    {
                        var index = i;
                        parent.AddTask(Scheduler.ThreadPool,
                                () =>
                                {
                                    using (MeasureTime("ReadMesh"))
                                    {
                                        return meshImporter.ReadMesh(this, index);
                                    }
                                })
                        .ContinueWith(Scheduler.MainThread, x =>
                        {
                            using (MeasureTime("BuildMesh"))
                            {
                                var meshWithMaterials = MeshImporter.BuildMesh(this, x);

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
                        })
                        .ContinueWith(Scheduler.ThreadPool, x => Meshes.Add(x))
                        ;
                    }
                })
                .ContinueWithCoroutine(Scheduler.MainThread, LoadNodes)
                .ContinueWithCoroutine(Scheduler.MainThread, BuildHierarchy)
                .ContinueWith(Scheduler.MainThread, _ =>
                {
                    using (MeasureTime("AnimationImporter"))
                    {
                        AnimationImporter.ImportAnimation(this);
                    }
                })
                .ContinueWith(Scheduler.CurrentThread,
                    _ =>
                    {
                        OnLoadModel();
                        if (m_showSpeedLog)
                        {
                            Debug.Log(GetSpeedLog());
                        }
                        return Unit.Default;
                    });
        }

        protected virtual void OnLoadModel()
        {
            Root.name = "GLTF";
        }

        IEnumerator TexturesProcessOnAnyThread()
        {
            using (MeasureTime("TexturesProcessOnAnyThread"))
            {
                foreach (var x in GetTextures())
                {
                    x.ProcessOnAnyThread(Store);
                    yield return null;
                }
            }
        }

        IEnumerator TexturesProcessOnMainThread()
        {
            using (MeasureTime("TexturesProcessOnMainThread"))
            {
                foreach (var x in GetTextures())
                {
                    yield return x.ProcessOnMainThreadCoroutine(Store);
                }
            }
        }

        IEnumerator LoadMaterials()
        {
            using (MeasureTime("LoadMaterials"))
            {
                if (GLTF2.Materials == null || !GLTF2.Materials.Any())
                {
                    AddMaterial(MaterialImporter.CreateMaterial(0, null));
                }
                else
                {
                    for (int i = 0; i < GLTF2.Materials.Count; ++i)
                    {
                        AddMaterial(MaterialImporter.CreateMaterial(i, GLTF2.Materials[i])); // TODO:
                    }
                }
            }
            yield return null;
        }

        IEnumerator LoadMeshes()
        {
            var meshImporter = new MeshImporter();
            for (int i = 0; i < GLTF2.Meshes.Count; ++i)
            {
                var meshContext = meshImporter.ReadMesh(this, i);
                var meshWithMaterials = MeshImporter.BuildMesh(this, meshContext);
                var mesh = meshWithMaterials.Mesh;
                if (string.IsNullOrEmpty(mesh.name))
                {
                    mesh.name = string.Format("UniGLTF import#{0}", i);
                }
                Meshes.Add(meshWithMaterials);

                yield return null;
            }
        }

        IEnumerator LoadNodes()
        {
            using (MeasureTime("LoadNodes"))
            {
                foreach (var x in GLTF2.Nodes)
                {
                    Nodes.Add(NodeImporter.ImportNode(x).transform);
                }
            }

            yield return null;
        }

        IEnumerator BuildHierarchy()
        {
            using (MeasureTime("BuildHierarchy"))
            {
                var nodes = new List<NodeImporter.TransformWithSkin>();
                for (int i = 0; i < Nodes.Count; ++i)
                {
                    nodes.Add(NodeImporter.BuildHierarchy(this, i));
                }

                NodeImporter.FixCoordinate(this, nodes);

                // skinning
                for (int i = 0; i < nodes.Count; ++i)
                {
                    NodeImporter.SetupSkinning(this, nodes, i);
                }

                // connect root
                if (Root == null)
                {
                    Root = new GameObject("_root_");
                }

                foreach (var i in GLTF2.RootNodesIndices)
                {
                    var t = nodes[i].Transform;
                    t.SetParent(Root.transform, false);
                }
            }

            yield return null;
        }
#endregion

#region Imported
        public GameObject Root;
        public List<Transform> Nodes = new List<Transform>();

        List<TextureItem> m_textures = new List<TextureItem>();
        public IList<TextureItem> GetTextures()
        {
            return m_textures;
        }
        public TextureItem GetTexture(int i)
        {
            if (i < 0 || i >= m_textures.Count)
            {
                return null;
            }
            return m_textures[i];
        }
        public void AddTexture(TextureItem item)
        {
            m_textures.Add(item);
        }

        List<Material> m_materials = new List<Material>();
        public void AddMaterial(Material material)
        {
            var originalName = material.name;
            int j = 2;
            while (m_materials.Any(x => x.name == material.name))
            {
                material.name = string.Format("{0}({1})", originalName, j++);
            }
            m_materials.Add(material);
        }
        public IList<Material> GetMaterials()
        {
            return m_materials;
        }
        public Material GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index];
        }

        public List<MeshWithMaterials> Meshes = new List<MeshWithMaterials>();
        public void ShowMeshes()
        {
            foreach (var x in Meshes)
            {
                foreach(var y in x.Renderers)
                {
                    y.enabled = true;
                }
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

        protected virtual IEnumerable<UnityEngine.Object> ObjectsForSubAsset()
        {
            HashSet<Texture2D> textures = new HashSet<Texture2D>();
            foreach (var x in m_textures.SelectMany(y => y.GetTexturesForSaveAssets()))
            {
                if (!textures.Contains(x))
                {
                    textures.Add(x);
                }
            }
            foreach (var x in textures) { yield return x; }
            foreach (var x in m_materials) { yield return x; }
            foreach (var x in Meshes) { yield return x.Mesh; }
            foreach (var x in AnimationClips) { yield return x; }
        }

#if UNITY_EDITOR
        #region Assets
        public bool MeshAsSubAsset = false;

        protected virtual UnityPath GetAssetPath(UnityPath prefabPath, UnityEngine.Object o)
        {
            if (o is Material)
            {
                var materialDir = prefabPath.GetAssetFolder(".Materials");
                var materialPath = materialDir.Child(o.name.EscapeFilePath() + ".asset");
                return materialPath;
            }
            else if (o is Texture2D)
            {
                var textureDir = prefabPath.GetAssetFolder(".Textures");
                var texturePath = textureDir.Child(o.name.EscapeFilePath() + ".asset");
                return texturePath;
            }
            else if (o is Mesh && !MeshAsSubAsset)
            {
                var meshDir = prefabPath.GetAssetFolder(".Meshes");
                var meshPath = meshDir.Child(o.name.EscapeFilePath() + ".asset");
                return meshPath;
            }
            else
            {
                return default(UnityPath);
            }
        }

        public virtual bool AvoidOverwriteAndLoad(UnityPath assetPath, UnityEngine.Object o)
        {
            if (o is Material)
            {
                var loaded = assetPath.LoadAsset<Material>();

                // replace component reference
                foreach(var mesh in Meshes)
                {
                    foreach(var r in mesh.Renderers)
                    {
                        for(int i=0; i<r.sharedMaterials.Length; ++i)
                        {
                            if (r.sharedMaterials.Contains(o))
                            {
                                r.sharedMaterials = r.sharedMaterials.Select(x => x == o ? loaded : x).ToArray();
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public void SaveAsAsset(UnityPath prefabPath)
        {
            ShowMeshes();

            //var prefabPath = PrefabPath;
            if (prefabPath.IsFileExists)
            {
                // clear SubAssets
                foreach (var x in prefabPath.GetSubAssets().Where(x => !(x is GameObject) && !(x is Component)))
                {
                    GameObject.DestroyImmediate(x, true);
                }
            }

            //
            // save sub assets
            //
            var paths = new List<UnityPath>(){
                prefabPath
            };
            foreach (var o in ObjectsForSubAsset())
            {
                if (o == null) continue;

                var assetPath = GetAssetPath(prefabPath, o);
                if (!assetPath.IsNull)
                {
                    if (assetPath.IsFileExists)
                    {
                        if (AvoidOverwriteAndLoad(assetPath, o))
                        {
                            // 上書きせずに既存のアセットからロードして置き換えた
                            continue;
                        }
                    }

                    // アセットとして書き込む
                    assetPath.Parent.EnsureFolder();
                    assetPath.CreateAsset(o);
                    paths.Add(assetPath);
                }
                else
                {
                    // save as subasset
                    prefabPath.AddObjectToAsset(o);
                }
            }

            // Create or upate Main Asset
            if (prefabPath.IsFileExists)
            {
                Debug.LogFormat("replace prefab: {0}", prefabPath);
                var prefab = prefabPath.LoadAsset<GameObject>();
                PrefabUtility.ReplacePrefab(Root, prefab, ReplacePrefabOptions.ReplaceNameBased);
            }
            else
            {
                Debug.LogFormat("create prefab: {0}", prefabPath);
                PrefabUtility.CreatePrefab(prefabPath.Value, Root);
            }
            foreach (var x in paths)
            {
                x.ImportAsset();
            }
        }

        /// <summary>
        /// Extract images from glb or gltf out of Assets folder.
        /// </summary>
        /// <param name="prefabPath"></param>
        public void ExtranctImages(UnityPath prefabPath)
        {
            var prefabParentDir = prefabPath.Parent;

            // glb buffer
            var folder = prefabPath.GetAssetFolder(".Textures");
            folder.EnsureFolder();

            //
            // https://answers.unity.com/questions/647615/how-to-update-import-settings-for-newly-created-as.html
            //
            int created = 0;
            //for (int i = 0; i < GLTF.textures.Count; ++i)
            var imageCount = GLTF2.Images != null ? GLTF2.Images.Count : 0;
            for (int i = 0; i < imageCount; ++i)
            {
                //var x = GLTF.textures[i];
                var image = GLTF2.Images[i];

                if (!string.IsNullOrEmpty(image.Uri))
                {
                    var src = Store.Loader.FullPathOf(image.Uri);
                    if (UnityPath.FromFullpath(src).IsUnderAssetsFolder)
                    {
                        // asset is exists.
                        continue;
                    }
                }

                var textureName = TmpUtil.PrepareTextureName(image, i);

                var r = Store.GetOrLoadImageResourceAt(i);
                var imageBytes = r.Data.ToArray();

                // path
                var dst = folder.Child(textureName + VGltf.Types.ImageExtensions.GetExtension(image));
                File.WriteAllBytes(dst.FullPath, imageBytes);
                dst.ImportAsset();

                // make relative path from PrefabParentDir
                // TODO: side effects
                image.Uri = dst.Value.Substring(prefabParentDir.Value.Length + 1);
                ++created;
            }

            if (created > 0)
            {
                AssetDatabase.Refresh();
            }

            CreateTextureItems(prefabParentDir);
        }
        #endregion
#endif

        /// <summary>
        /// This function is used for clean up after create assets.
        /// </summary>
        /// <param name="destroySubAssets">Ambiguous arguments</param>
        [Obsolete("Use Dispose for runtime loader resource management")]
        public void Destroy(bool destroySubAssets)
        {
            if (Root != null) GameObject.DestroyImmediate(Root);
            if (destroySubAssets)
            {
#if UNITY_EDITOR
                foreach (var o in ObjectsForSubAsset())
                {
                    UnityEngine.Object.DestroyImmediate(o, true);
                }
#endif
            }
        }

        public void Dispose()
        {
            DestroyRootAndResources();
        }

        /// <summary>
        /// Destroy resources that created ImporterContext for runtime load.
        /// </summary>
        public void DestroyRootAndResources()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarningFormat("Dispose called in editor mode. This function is for runtime");
            }

            // Remove hierarchy
            if (Root != null) GameObject.Destroy(Root);

            // Remove resources. materials, textures meshes etc...
            foreach (var o in ObjectsForSubAsset())
            {
                UnityEngine.Object.DestroyImmediate(o, true);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Destroy the GameObject that became the basis of Prefab
        /// </summary>
        public void EditorDestroyRoot()
        {
            if (Root != null) GameObject.DestroyImmediate(Root);
        }

        /// <summary>
        /// Destroy assets that created ImporterContext. This function is clean up for imoprter error.
        /// </summary>
        public void EditorDestroyRootAndAssets()
        {
            // Remove hierarchy
            if (Root != null) GameObject.DestroyImmediate(Root);

            // Remove resources. materials, textures meshes etc...
            foreach (var o in ObjectsForSubAsset())
            {
                UnityEngine.Object.DestroyImmediate(o, true);
            }
        }
#endif
    }
}
