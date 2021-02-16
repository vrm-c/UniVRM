using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using UniJSON;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniGLTF
{
    /// <summary>
    /// GLTF importer
    /// </summary>
    public class ImporterContext : IDisposable
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

        MaterialFactory m_materialFactory;
        public MaterialFactory MaterialFactory => m_materialFactory;

        TextureFactory m_textureFactory;
        public TextureFactory TextureFactory => m_textureFactory;

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
            if (!generatorVersion.FastStartsWith("UniGLTF-")) return false;

            try
            {
                var splitted = generatorVersion.Substring(8).Split('.');
                var generatorMajor = int.Parse(splitted[0]);
                var generatorMinor = int.Parse(splitted[1]);

                if (generatorMajor < major)
                {
                    return true;
                }
                else if (generatorMajor > major)
                {
                    return false;
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
            var chunks = glbImporter.ParseGlbChunks(bytes);

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

            try
            {
                var jsonBytes = chunks[0].Bytes;
                ParseJson(Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                    new SimpleStorage(chunks[1].Bytes));
            }
            catch (StackOverflowException ex)
            {
                throw new Exception("[UniVRM Import Error] json parsing failed, nesting is too deep.\n" + ex);
            }
            catch
            {
                throw;
            }
        }

        public virtual void ParseJson(string json, IStorage storage)
        {
            Json = json;
            Storage = storage;
            GLTF = GltfDeserializer.Deserialize(json.ParseAsJson());
            if (GLTF.asset.version != "2.0")
            {
                throw new UniGLTFException("unknown gltf version {0}", GLTF.asset.version);
            }

            m_textureFactory = new TextureFactory(GLTF, Storage);
            m_materialFactory = new MaterialFactory(GLTF, Storage);

            // Version Compatibility
            RestoreOlderVersionValues();

            FixUnique();
            FixNodeName();

            // parepare byte buffer
            //GLTF.baseDir = System.IO.Path.GetDirectoryName(Path);
            foreach (var buffer in GLTF.buffers)
            {
                buffer.OpenStorage(storage);
            }
        }

        void FixUnique()
        {
            var used = new HashSet<string>();
            foreach (var mesh in GLTF.meshes)
            {
                if (string.IsNullOrEmpty(mesh.name))
                {
                    mesh.name = "mesh_" + Guid.NewGuid().ToString("N");
                    used.Add(mesh.name);
                }
                else
                {
                    var lname = mesh.name.ToLower();
                    if (used.Contains(lname))
                    {
                        // rename
                        var uname = lname + "_" + Guid.NewGuid().ToString("N");
                        Debug.LogWarning($"same name: {lname} => {uname}");
                        mesh.name = uname;
                        lname = uname;
                    }

                    used.Add(lname);
                }
            }
        }

        /// <summary>
        /// rename empty name to $"{index}"
        /// </summary>
        void FixNodeName()
        {
            for (var i = 0; i < GLTF.nodes.Count; ++i)
            {
                var node = GLTF.nodes[i];
                if (string.IsNullOrWhiteSpace(node.name))
                {
                    node.name = $"{i}";
                }
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
        }
        #endregion

        #region Load. Build unity objects

        public bool EnableLoadBalancing;

        public virtual async Task LoadAsync(Func<Task> nextFrame = null)
        {
            if (nextFrame == null)
            {
                nextFrame = () => Task.FromResult<object>(null);
            }

            if (Root == null)
            {
                Root = new GameObject("_root_");
            }

            // UniGLTF does not support draco
            // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md#conformance
            if (GLTF.extensionsRequired.Contains("KHR_draco_mesh_compression"))
            {
                throw new UniGLTFNotSupportedException("draco is not supported");
            }

            await m_materialFactory.LoadMaterialsAsync(m_textureFactory.GetTextureAsync);

            // meshes
            var meshImporter = new MeshImporter();
            for (int i = 0; i < GLTF.meshes.Count; ++i)
            {
                var index = i;
                using (MeasureTime("ReadMesh"))
                {
                    var x = await Task.Run(() => meshImporter.ReadMesh(this, index));
                    var y = await BuildMeshAsync(nextFrame, x, index);
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
            await nextFrame();

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
                foreach (var x in GLTF.rootnodes)
                {
                    var t = nodes[x].Transform;
                    t.SetParent(Root.transform, false);
                }
            }
            await nextFrame();

            using (MeasureTime("AnimationImporter"))
            {
                AnimationImporter.Import(this);
            }

            await OnLoadModel(nextFrame);

            if (m_showSpeedLog)
            {
                Debug.Log(GetSpeedLog());
            }
        }

        protected virtual async Task OnLoadModel(Func<Task> nextFrame)
        {
            Root.name = "GLTF";
            await nextFrame();
        }

        async Task<MeshWithMaterials> BuildMeshAsync(Func<Task> nextFrame, MeshImporter.MeshContext x, int i)
        {
            using (MeasureTime("BuildMesh"))
            {
                MeshWithMaterials meshWithMaterials;
                if (EnableLoadBalancing)
                {
                    meshWithMaterials = await MeshImporter.BuildMeshAsync(nextFrame, MaterialFactory, x);
                }
                else
                {
                    meshWithMaterials = MeshImporter.BuildMesh(MaterialFactory, x);
                }

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
            foreach (var x in TextureFactory.ObjectsForSubAsset())
            {
                yield return x;
            }
            foreach (var x in MaterialFactory.ObjectsForSubAsset())
            {
                yield return x;
            }
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
                foreach (var mesh in Meshes)
                {
                    foreach (var r in mesh.Renderers)
                    {
                        for (int i = 0; i < r.sharedMaterials.Length; ++i)
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

            // Create or update Main Asset
            if (prefabPath.IsFileExists)
            {
                Debug.LogFormat("replace prefab: {0}", prefabPath);
                var prefab = prefabPath.LoadAsset<GameObject>();
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(Root, prefabPath.Value, InteractionMode.AutomatedAction);
#else
                PrefabUtility.ReplacePrefab(Root, prefab, ReplacePrefabOptions.ReplaceNameBased);
#endif

            }
            else
            {
                Debug.LogFormat("create prefab: {0}", prefabPath);
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(Root, prefabPath.Value, InteractionMode.AutomatedAction);
#else
                PrefabUtility.CreatePrefab(prefabPath.Value, Root);
#endif
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
        public void ExtractImages(UnityPath prefabPath)
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

            // texture will load from assets
            m_textureFactory.ImageBaseDir = prefabParentDir;
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
            foreach (var x in Meshes)
            {
                UnityEngine.Object.DestroyImmediate(x.Mesh, true);
            }
            foreach (var x in AnimationClips)
            {
                UnityEngine.Object.DestroyImmediate(x, true);
            }
            MaterialFactory.Dispose();
            TextureFactory.Dispose();
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
        /// Destroy assets that created ImporterContext. This function is clean up for importer error.
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
