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
            if (GLTF == null) return false;
            if (GLTF.asset == null) return false;
            return IsGeneratedUniGLTFAndOlderThan(GLTF.asset.generator, major, minor);
        }

        /// <summary>
        /// URI access
        /// </summary>
        public IStorage Storage;
        #endregion

        #region Parse
        public void Parse(string path)
        {
            Parse(path, File.ReadAllBytes(path));
        }

        /// <summary>
        /// Parse gltf json or Parse json chunk of glb
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public virtual void Parse(string path, Byte[] bytes)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".gltf":
                    ParseJson(Encoding.UTF8.GetString(bytes), new FileSystemStorage(Path.GetDirectoryName(path)));
                    break;

                case ".zip":
                    {
                        var zipArchive = Zip.ZipArchiveStorage.Parse(bytes);
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

                case ".glb":
                    ParseGlb(bytes);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bytes"></param>
        public void ParseGlb(Byte[] bytes)
        {
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

            var jsonBytes = chunks[0].Bytes;
            ParseJson(Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                new SimpleStorage(chunks[1].Bytes));
        }

        public bool UseUniJSONParser;
        public virtual void ParseJson(string json, IStorage storage)
        {
            Json = json;
            Storage = storage;

            if (UseUniJSONParser)
            {
                Json.ParseAsJson().Deserialize(ref GLTF);
            }
            else
            {
                GLTF = JsonUtility.FromJson<glTF>(Json);
            }

            if (GLTF.asset.version != "2.0")
            {
                throw new UniGLTFException("unknown gltf version {0}", GLTF.asset.version);
            }

            // Version Compatibility
            RestoreOlderVersionValues();

            // parepare byte buffer
            //GLTF.baseDir = System.IO.Path.GetDirectoryName(Path);
            foreach (var buffer in GLTF.buffers)
            {
                buffer.OpenStorage(storage);
            }
        }

        void RestoreOlderVersionValues()
        {
            var parsed = UniJSON.JsonParser.Parse(Json);
            for (int i = 0; i < GLTF.images.Count; ++i)
            {
                if (string.IsNullOrEmpty(GLTF.images[i].name))
                {
                    try
                    {
                        var extraName = parsed["images"][i]["extra"]["name"].Value.GetString();
                        if (!string.IsNullOrEmpty(extraName))
                        {
                            //Debug.LogFormat("restore texturename: {0}", extraName);
                            GLTF.images[i].name = extraName;
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }
            for (int i = 0; i < GLTF.meshes.Count; ++i)
            {
                var mesh = GLTF.meshes[i];
                try
                {
                    for (int j = 0; j < mesh.primitives.Count; ++j)
                    {
                        var primitive = mesh.primitives[j];
                        for (int k = 0; k < primitive.targets.Count; ++k)
                        {
                            var extraName = parsed["meshes"][i]["primitives"][j]["targets"][k]["extra"]["name"].Value.GetString();
                            //Debug.LogFormat("restore morphName: {0}", extraName);
                            primitive.extras.targetNames.Add(extraName);
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
#if false
            for (int i = 0; i < GLTF.nodes.Count; ++i)
            {
                var node = GLTF.nodes[i];
                try
                {
                    var extra = parsed["nodes"][i]["extra"]["skinRootBone"].AsInt;
                    //Debug.LogFormat("restore extra: {0}", extra);
                    //node.extras.skinRootBone = extra;
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
#endif
        }
        #endregion

        #region Load. Build unity objects
        /// <summary>
        /// ReadAllBytes, Parse, Create GameObject
        /// </summary>
        /// <param name="path">allbytes</param>
        public void Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Load(path, bytes);
        }

        /// <summary>
        /// Parse, Create GameObject
        /// </summary>
        /// <param name="path">gltf or glb path</param>
        /// <param name="bytes">allbytes</param>
        public void Load(string path, byte[] bytes)
        {
            Parse(path, bytes);
            Load();
            Root.name = Path.GetFileNameWithoutExtension(path);
        }

        public void CreateTextureItems(UnityPath imageBaseDir = default(UnityPath))
        {
            if (m_textures.Any())
            {
                return;
            }

            for (int i = 0; i < GLTF.textures.Count; ++i)
            {

                TextureItem item = null;
#if UNITY_EDITOR
                var image = GLTF.GetImageFromTextureIndex(i);
                if (imageBaseDir.IsUnderAssetsFolder
                    && !string.IsNullOrEmpty(image.uri)
                    && !image.uri.StartsWith("data:")
                    )
                {
                    ///
                    /// required SaveTexturesAsPng or SetTextureBaseDir
                    ///
                    var assetPath = imageBaseDir.Child(image.uri);
                    var textureName = !string.IsNullOrEmpty(image.name) ? image.name : Path.GetFileNameWithoutExtension(image.uri);
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

#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER && !UNITY_WEBGL)
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
                    if (GLTF.extensionsRequired.Contains("KHR_draco_mesh_compression"))
                    {
                        throw new UniGLTFNotSupportedException("draco is not supported");
                    }

                    // meshes
                    var meshImporter = new MeshImporter();
                    for (int i = 0; i < GLTF.meshes.Count; ++i)
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
                    x.ProcessOnAnyThread(GLTF, Storage);
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
                    yield return x.ProcessOnMainThreadCoroutine(GLTF);
                }
            }
        }

        IEnumerator LoadMaterials()
        {
            using (MeasureTime("LoadMaterials"))
            {
                if (GLTF.materials == null || !GLTF.materials.Any())
                {
                    AddMaterial(MaterialImporter.CreateMaterial(0, null));
                }
                else
                {
                    for (int i = 0; i < GLTF.materials.Count; ++i)
                    {
                        AddMaterial(MaterialImporter.CreateMaterial(i, GLTF.materials[i]));
                    }
                }
            }
            yield return null;
        }

        IEnumerator LoadMeshes()
        {
            var meshImporter = new MeshImporter();
            for (int i = 0; i < GLTF.meshes.Count; ++i)
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
                foreach (var x in GLTF.nodes)
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
                foreach (var x in GLTF.rootnodes)
                {
                    var t = nodes[x].Transform;
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

            //
            // https://answers.unity.com/questions/647615/how-to-update-import-settings-for-newly-created-as.html
            //
            int created = 0;
            //for (int i = 0; i < GLTF.textures.Count; ++i)
            for (int i = 0; i < GLTF.images.Count; ++i)
            {
                folder.EnsureFolder();

                //var x = GLTF.textures[i];
                var image = GLTF.images[i];
                var src = Storage.GetPath(image.uri);
                if (UnityPath.FromFullpath(src).IsUnderAssetsFolder)
                {
                    // asset is exists.
                }
                else
                {
                    string textureName;
                    var byteSegment = GLTF.GetImageBytes(Storage, i, out textureName);

                    // path
                    var dst = folder.Child(textureName + image.GetExt());
                    File.WriteAllBytes(dst.FullPath, byteSegment.ToArray());
                    dst.ImportAsset();

                    // make relative path from PrefabParentDir
                    image.uri = dst.Value.Substring(prefabParentDir.Value.Length + 1);
                    ++created;
                }
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
