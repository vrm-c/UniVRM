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

            // using (MeasureTime("LoadTextures"))
            // {
            //     for (int i = 0; i < GLTF.materials.Count; ++i)
            //     {
            //         foreach (var param in MaterialFactory.EnumerateGetTextureparam(i))
            //         {
            //             await m_textureFactory.GetTextureAsync(GLTF, param);
            //         }
            //     }
            // }

            using (MeasureTime("LoadMaterials"))
            {
                await m_materialFactory.LoadMaterialsAsync(m_textureFactory.GetTextureAsync);
            }

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
            await NextFrameAwaitable.Create();

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
            await NextFrameAwaitable.Create();

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
            for (int i = 0; i < GLTF.textures.Count; ++i)
            {
                folder.EnsureFolder();

                var gltfTexture = GLTF.textures[i];
                var gltfImage = GLTF.images[gltfTexture.source];
                var src = Storage.GetPath(gltfImage.uri);
                if (UnityPath.FromFullpath(src).IsUnderAssetsFolder)
                {
                    // asset is exists.
                }
                else
                {
                    var byteSegment = GLTF.GetImageBytes(Storage, gltfTexture.source);
                    var textureName = gltfTexture.name;

                    // path
                    var dst = folder.Child(textureName + gltfImage.GetExt());
                    File.WriteAllBytes(dst.FullPath, byteSegment.ToArray());
                    dst.ImportAsset();

                    // make relative path from PrefabParentDir
                    gltfImage.uri = dst.Value.Substring(prefabParentDir.Value.Length + 1);
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

        /// <summary>
        /// Importに使った一時オブジェクトを破棄する
        /// 
        /// 変換のあるテクスチャで、変換前のもの
        /// normal, occlusion, metallicRoughness
        /// </summary>
        public void Dispose()
        {
            // m_textureFactory.Dispose();
        }

        /// <summary>
        /// Root ヒエラルキーで使っているリソース
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<UnityEngine.Object> ResourcesInRootHierarchy()
        {
            foreach (var mesh in Meshes)
            {
                yield return mesh.Mesh;
            }
            foreach (var material in m_materialFactory.Materials)
            {
                yield return material.Asset;
            }
            foreach (var texture in m_textureFactory.Textures)
            {
                yield return texture.Texture;
            }
            foreach (var animation in AnimationClips)
            {
                yield return animation;
            }
        }

        public UnityResourceDestroyer DisposeOnGameObjectDestroyed()
        {
            var destroyer = Root.AddComponent<UnityResourceDestroyer>();
            foreach (var x in ResourcesInRootHierarchy())
            {
                destroyer.Resources.Add(x);
            }
            return destroyer;
        }
    }
}
