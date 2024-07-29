using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UniGLTF.MeshUtility;
using UnityEngine;

namespace UniGLTF
{
    public class gltfExporter : IDisposable
    {
        protected ExportingGltfData _data;

        protected glTF _gltf => _data.Gltf;

        public GameObject Copy
        {
            get;
            protected set;
        }

        public List<Mesh> Meshes { get; private set; } = new List<Mesh>();

        /// <summary>
        /// Mesh毎に、元のBlendShapeIndex => ExportされたBlendShapeIndex の対応を記録する
        ///
        /// BlendShape が空の場合にスキップするので
        /// </summary>
        /// <value></value>
        public Dictionary<Mesh, Dictionary<int, int>> MeshBlendShapeIndexMap
        {
            get;
            private set;
        }

        public List<Transform> Nodes
        {
            get;
            private set;
        }

        public List<Material> Materials
        {
            get;
            private set;
        }

        protected ITextureExporter TextureExporter => _textureExporter;
        private TextureExporter _textureExporter;

        GltfExportSettings m_settings;

        IProgress<ExportProgress> m_progress;

        void ReportProgress(string msg, float progress)
        {
            if (m_progress == null)
            {
                return;
            }
            m_progress.Report(new ExportProgress("gltfExporter", msg, progress));
        }

        private readonly IAnimationExporter m_animationExporter;
        private readonly IMaterialExporter m_materialExporter;
        private readonly ITextureSerializer m_textureSerializer;

        public gltfExporter(
            ExportingGltfData data,
            GltfExportSettings settings,
            IProgress<ExportProgress> progress = null,
            IAnimationExporter animationExporter = null,
            IMaterialExporter materialExporter = null,
            ITextureSerializer textureSerializer = null
        )
        {
            _data = data;

            _gltf.asset = new glTFAssets
            {
                generator = "UniGLTF-" + UniGLTFVersion.VERSION,
                version = "2.0",
            };

            m_settings = settings;
            if (m_settings == null)
            {
                // default
                m_settings = new GltfExportSettings();
            }

            m_animationExporter = animationExporter;
            m_materialExporter = materialExporter ?? MaterialExporterUtility.GetValidGltfMaterialExporter();
            m_textureSerializer = textureSerializer ?? new RuntimeTextureSerializer();
        }

        GameObject m_tmpParent = null;

        public virtual void Prepare(GameObject go)
        {
            // コピーを作って左手系を右手系に変換する
            Copy = GameObject.Instantiate(go);
            Copy.transform.ReverseRecursive(m_settings.InverseAxis.Create());

            // Export の root は gltf の scene になるので、
            // エクスポート対象が単一の GameObject の場合に、
            // ダミー親 "m_tmpParent" を一時的に作成する。
            //
            // https://github.com/vrm-c/UniVRM/pull/736
            if (Copy.transform.childCount == 0)
            {
                m_tmpParent = new GameObject("tmpParent");
                Copy.transform.SetParent(m_tmpParent.transform, true);
                Copy = m_tmpParent;
            }

            if (Copy.transform.TryGetComponent<Renderer>(out var r))
            {
                // should throw ?
                Debug.LogError("root mesh is not exported");
            }

            ReportProgress("prepared", 0.1f);
        }

        public void Dispose()
        {
            if (m_tmpParent != null)
            {
                var child = m_tmpParent.transform.GetChild(0);
                child.SetParent(null);
                Copy = child.gameObject;
                if (Application.isPlaying)
                {
                    GameObject.Destroy(m_tmpParent);
                }
                else
                {
                    GameObject.DestroyImmediate(m_tmpParent);
                }
            }

            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(Copy);
            }
            else
            {
                GameObject.Destroy(Copy);
            }

            _textureExporter.Dispose();
        }

        #region Export
        static glTFNode ExportNode(Transform x, List<Transform> nodes, IReadOnlyList<MeshExportInfo> meshWithRenderers, List<SkinnedMeshRenderer> skins)
        {
            var node = new glTFNode
            {
                name = x.name,
                children = x.transform.GetChildren().Select(y => nodes.IndexOf(y)).ToArray(),
                rotation = x.transform.localRotation.ToArray(),
                translation = x.transform.localPosition.ToArray(),
                scale = x.transform.localScale.ToArray(),
            };

            if (x.gameObject.activeInHierarchy)
            {
                if (x.TryGetComponent<MeshRenderer>(out var meshRenderer) && meshRenderer.enabled)
                {
                    if (x.TryGetComponent<MeshFilter>(out var meshFilter))
                    {
                        var mesh = meshFilter.sharedMesh;
                        var materials = meshRenderer.sharedMaterials;
                        if (MeshExportInfo.TryGetSameMeshIndex(meshWithRenderers, mesh, materials, out int meshIndex))
                        {
                            node.mesh = meshIndex;
                        }
                        else if (mesh == null)
                        {
                            // mesh が無い
                            node.mesh = -1;
                        }
                        else if (mesh.vertexCount == 0)
                        {
                            // 頂点データが無い場合
                            node.mesh = -1;
                        }
                        else
                        {
                            // MeshとMaterialが一致するものが見つからなかった
                            throw new Exception("Mesh not found.");
                        }
                    }
                }

                if (x.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer) && skinnedMeshRenderer.enabled)
                {
                    var mesh = skinnedMeshRenderer.sharedMesh;
                    var materials = skinnedMeshRenderer.sharedMaterials;
                    if (MeshExportInfo.TryGetSameMeshIndex(meshWithRenderers, mesh, materials, out int meshIndex))
                    {
                        node.mesh = meshIndex;
                        node.skin = skins.IndexOf(skinnedMeshRenderer);
                    }
                    else if (mesh == null)
                    {
                        // mesh が無い
                        node.mesh = -1;
                    }
                    else if (mesh.vertexCount == 0)
                    {
                        // 頂点データが無い場合
                        node.mesh = -1;
                    }
                    else
                    {
                        // MeshとMaterialが一致するものが見つからなかった
                        throw new Exception("Mesh not found.");
                    }
                }
            }

            return node;
        }

        public virtual void ExportExtensions(ITextureSerializer textureSerializer)
        {
            // do nothing
        }

        public virtual void Export()
        {
            if (m_settings.FreezeMesh)
            {
                // Transform の回転とスケールを Mesh に適用します。
                // - BlendShape は現状がbakeされます
                // - 回転とスケールが反映された新しい Mesh が作成されます
                // - Transform の回転とスケールはクリアされます。world position を維持します
                var newMeshMap = BoneNormalizer.NormalizeHierarchyFreezeMesh(Copy);

                // SkinnedMeshRenderer.sharedMesh と MeshFilter.sharedMesh を新しいMeshで置き換える
                BoneNormalizer.Replace(Copy, newMeshMap, true, true);
            }

            Nodes = Copy.transform.Traverse()
                .Skip(1) // exclude root object for the symmetry with the importer
                .ToList();

            var uniqueUnityMeshes = new MeshExportList();
            uniqueUnityMeshes.GetInfo(Nodes, m_settings);

            #region Materials and Textures
            ReportProgress("Materials and Textures", 0.2f);
            Materials = uniqueUnityMeshes.GetUniqueMaterials().ToList();

            _textureExporter = new TextureExporter(m_textureSerializer);

            _gltf.materials = Materials.Select(x => m_materialExporter.ExportMaterial(x, TextureExporter, m_settings)).ToList();
            #endregion

            #region Meshes
            ReportProgress("Meshes", 0.4f);
            MeshBlendShapeIndexMap = new Dictionary<Mesh, Dictionary<int, int>>();
            foreach (var unityMesh in uniqueUnityMeshes)
            {
                if (!unityMesh.CanExport)
                {
                    continue;
                }

                var (gltfMesh, blendShapeIndexMap) = m_settings.DivideVertexBuffer
                    ? MeshExporter_DividedVertexBuffer.Export(_data, unityMesh, Materials, m_settings.InverseAxis.Create(), m_settings)
                    : MeshExporter_SharedVertexBuffer.Export(_data, unityMesh, Materials, m_settings.InverseAxis.Create(), m_settings)
                    ;
                _gltf.meshes.Add(gltfMesh);
                Meshes.Add(unityMesh.Mesh);
                if (!MeshBlendShapeIndexMap.ContainsKey(unityMesh.Mesh))
                {
                    // 重複防止
                    MeshBlendShapeIndexMap.Add(unityMesh.Mesh, blendShapeIndexMap);
                }
            }
            #endregion

            #region Nodes and Skins
            ReportProgress("Nodes and Skins", 0.8f);
            var skins = uniqueUnityMeshes
                .SelectMany(x => x.Renderers)
                .Where(x => x.Item1 is SkinnedMeshRenderer && x.UniqueBones != null)
                .Select(x => x.Item1 as SkinnedMeshRenderer)
                .ToList()
                ;
            foreach (var node in Nodes)
            {
                var gltfNode = ExportNode(node, Nodes, uniqueUnityMeshes, skins);
                _gltf.nodes.Add(gltfNode);
            }
            _gltf.scenes = new List<gltfScene>
                {
                    new gltfScene
                    {
                        nodes = Copy.transform.GetChildren().Select(x => Nodes.IndexOf(x)).ToArray(),
                    }
                };

            foreach (var x in uniqueUnityMeshes)
            {
                foreach (var (renderer, uniqueBones) in x.Renderers)
                {
                    if (uniqueBones != null && renderer is SkinnedMeshRenderer smr)
                    {
                        var matrices = x.GetBindPoses().Select(m_settings.InverseAxis.Create().InvertMat4).ToArray();
                        var accessor = _data.ExtendBufferAndGetAccessorIndex(matrices, glBufferTarget.NONE);
                        var skin = new glTFSkin
                        {
                            inverseBindMatrices = accessor,
                            joints = uniqueBones.Select(y =>
                            {
                                var index = Nodes.IndexOf(y);
                                if (index < 0)
                                {
                                    // bones の先頭を使う
                                    return 0;
                                }
                                else
                                {
                                    return index;
                                }
                            }).ToArray(),
                            skeleton = Nodes.IndexOf(smr.rootBone),
                        };
                        var skinIndex = _gltf.skins.Count;
                        _gltf.skins.Add(skin);

                        foreach (var z in Nodes.Where(y => y.Has(renderer)))
                        {
                            var nodeIndex = Nodes.IndexOf(z);
                            var node = _gltf.nodes[nodeIndex];
                            node.skin = skinIndex;
                        }
                    }
                }
            }
            #endregion

            if (m_animationExporter != null)
            {
                ReportProgress("Animations", 0.9f);
                m_animationExporter.Export(_data, Copy, Nodes);
            }

            ExportExtensions(m_textureSerializer);

            // Extension で Texture が増える場合があるので最後に呼ぶ
            var exported = _textureExporter.Export();
            for (var exportedTextureIdx = 0; exportedTextureIdx < exported.Count; ++exportedTextureIdx)
            {
                var (unityTexture, colorSpace) = exported[exportedTextureIdx];
                GltfTextureExporter.PushGltfTexture(_data, unityTexture, colorSpace, m_textureSerializer);
            }

            FixName(_gltf);
        }

        /// <summary>
        /// GlbLowLevelParser.FixNameUnique で付与した Suffix を remove
        /// </summary>
        public static void FixName(glTF gltf)
        {
            var regex = new Regex($@"{GlbLowLevelParser.UniqueFixResourceSuffix}\d+$");
            foreach (var gltfImages in gltf.images)
            {
                if (regex.IsMatch(gltfImages.name))
                {
                    gltfImages.name = regex.Replace(gltfImages.name, string.Empty);
                }
            }
            foreach (var gltfMaterial in gltf.materials)
            {
                if (regex.IsMatch(gltfMaterial.name))
                {
                    gltfMaterial.name = regex.Replace(gltfMaterial.name, string.Empty);
                }
            }
            foreach (var gltfAnimation in gltf.animations)
            {
                if (regex.IsMatch(gltfAnimation.name))
                {
                    gltfAnimation.name = regex.Replace(gltfAnimation.name, string.Empty);
                }
            }
        }
        #endregion
    }
}
