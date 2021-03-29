using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Text;
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

        MaterialFactory m_materialFactory;
        public MaterialFactory MaterialFactory => m_materialFactory;

        public readonly GltfMaterialImporter GltfMaterialImporter = new GltfMaterialImporter();

        TextureFactory m_textureFactory;
        public TextureFactory TextureFactory => m_textureFactory;

        IAwaitCaller m_awaitCaller;

        public ImporterContext(GltfParser parser,
            IEnumerable<(string, UnityEngine.Object)> externalObjectMap = null)
        {
            m_parser = parser;
            m_textureFactory = new TextureFactory(externalObjectMap);
            m_materialFactory = new MaterialFactory(externalObjectMap);
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
            m_awaitCaller = awaitCaller;

            if (MeasureTime == null)
            {
                MeasureTime = new ImporterContextSpeedLog().MeasureTime;
            }

            var inverter = InvertAxis.Create();

            if (Root == null)
            {
                Root = new GameObject("GLTF");
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

            using (MeasureTime("LoadMaterials"))
            {
                await LoadMaterialsAsync();
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
            await m_awaitCaller.NextFrame();

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
            await m_awaitCaller.NextFrame();

            using (MeasureTime("AnimationImporter"))
            {
                AnimationImporter.Import(this);
            }

            await OnLoadModel(m_awaitCaller, MeasureTime);
        }

        public async Task LoadMaterialsAsync()
        {
            if (m_parser.GLTF.materials == null || m_parser.GLTF.materials.Count == 0)
            {
                // no material. work around.
                var param = GltfMaterialImporter.CreateParam(m_parser, 0);
                var material = await MaterialFactory.LoadAsync(param, TextureFactory.GetTextureAsync);
            }
            else
            {
                for (int i = 0; i < m_parser.GLTF.materials.Count; ++i)
                {
                    var param = GltfMaterialImporter.CreateParam(m_parser, i);
                    var material = await MaterialFactory.LoadAsync(param, TextureFactory.GetTextureAsync);
                }
            }
        }

        protected virtual Task OnLoadModel(IAwaitCaller awaitCaller, Func<string, IDisposable> MeasureTime)
        {
            // do nothing
            return Task.FromResult<object>(null);
        }

        async Task<MeshWithMaterials> BuildMeshAsync(Func<string, IDisposable> MeasureTime, MeshImporter.MeshContext x, int i)
        {
            using (MeasureTime("BuildMesh"))
            {
                var meshWithMaterials = await MeshImporter.BuildMeshAsync(m_awaitCaller, MaterialFactory, x);
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

            m_materialFactory.Dispose();
            m_textureFactory.Dispose();

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
