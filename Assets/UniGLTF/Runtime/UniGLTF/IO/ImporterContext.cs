using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UniGLTF.AltTask;
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

        public ImporterContext(GltfParser parser, IEnumerable<(string, UnityEngine.Object)> externalObjectMap = null)
        {
            m_parser = parser;
            m_textureFactory = new TextureFactory(GLTF, Storage, externalObjectMap);
            m_materialFactory = new MaterialFactory(GLTF, Storage, externalObjectMap);
        }

        #region Source
        GltfParser m_parser;
        public GltfParser Parser => m_parser;
        public String Json => m_parser.Json;
        public glTF GLTF => m_parser.GLTF;
        public IStorage Storage => m_parser.Storage;
        #endregion

        // configuration

        /// <summary>
        /// GLTF から Unity に変換するときに反転させる軸
        /// </summary>
        public Axises InvertAxis = Axises.Z;

        #region Load. Build unity objects
        public virtual async Awaitable LoadAsync(Func<string, IDisposable> MeasureTime = null)
        {
            if (MeasureTime == null)
            {
                MeasureTime = new ImporterContextSpeedLog().MeasureTime;
            }

            AxisInverter inverter = default;
            switch (InvertAxis)
            {
                case Axises.Z:
                    inverter = AxisInverter.ReverseZ;
                    break;

                case Axises.X:
                    inverter = AxisInverter.ReverseX;
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (Root == null)
            {
                Root = new GameObject("GLTF");
            }

            // UniGLTF does not support draco
            // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md#conformance
            if (GLTF.extensionsRequired.Contains("KHR_draco_mesh_compression"))
            {
                throw new UniGLTFNotSupportedException("draco is not supported");
            }

            // create textures
            for (int i = 0; i < GLTF.materials.Count; ++i)
            {
                foreach (var param in MaterialFactory.EnumerateGetTextureparam(i))
                {
                    await m_textureFactory.GetTextureAsync(GLTF, param);
                }
            }

            await m_materialFactory.LoadMaterialsAsync(m_textureFactory.GetTextureAsync);

            // meshes
            var meshImporter = new MeshImporter();
            for (int i = 0; i < GLTF.meshes.Count; ++i)
            {
                var index = i;
                using (MeasureTime("ReadMesh"))
                {
                    var x = meshImporter.ReadMesh(this, index, inverter);
                    var y = await BuildMeshAsync(MeasureTime, x, index);
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
            await LoopAwaitable.Create();

            using (MeasureTime("BuildHierarchy"))
            {
                var nodes = new List<NodeImporter.TransformWithSkin>();
                for (int i = 0; i < Nodes.Count; ++i)
                {
                    nodes.Add(NodeImporter.BuildHierarchy(this, i));
                }

                NodeImporter.FixCoordinate(this, nodes, inverter);

                // skinning
                for (int i = 0; i < nodes.Count; ++i)
                {
                    NodeImporter.SetupSkinning(this, nodes, i, inverter);
                }

                // connect root
                foreach (var x in GLTF.rootnodes)
                {
                    var t = nodes[x].Transform;
                    t.SetParent(Root.transform, false);
                }
            }
            await LoopAwaitable.Create();

            using (MeasureTime("AnimationImporter"))
            {
                AnimationImporter.Import(this);
            }

            await OnLoadModel(MeasureTime);
        }

        protected virtual async Awaitable OnLoadModel(Func<string, IDisposable> MeasureTime)
        {
            // do nothing
        }

        async Awaitable<MeshWithMaterials> BuildMeshAsync(Func<string, IDisposable> MeasureTime, MeshImporter.MeshContext x, int i)
        {
            using (MeasureTime("BuildMesh"))
            {
                var meshWithMaterials = await MeshImporter.BuildMeshAsync(MaterialFactory, x);
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
