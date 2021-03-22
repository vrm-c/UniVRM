using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    public class gltfExporter : IDisposable
    {

        protected glTF glTF;

        public GameObject Copy
        {
            get;
            protected set;
        }

        public List<Mesh> Meshes
        {
            get;
            private set;
        }

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

        public TextureExporter TextureManager;

        protected virtual IMaterialExporter CreateMaterialExporter()
        {
            return new MaterialExporter();
        }

        /// <summary>
        /// このエクスポーターがサポートするExtension
        /// </summary>
        protected virtual IEnumerable<string> ExtensionUsed
        {
            get
            {
                yield return glTF_KHR_materials_unlit.ExtensionName;
                yield return glTF_KHR_texture_transform.ExtensionName;
            }
        }

        IAxisInverter m_axisInverter;

        public gltfExporter(glTF gltf, Axises invertAxis = Axises.Z)
        {
            glTF = gltf;

            glTF.extensionsUsed.AddRange(ExtensionUsed);

            glTF.asset = new glTFAssets
            {
                generator = "UniGLTF-" + UniGLTFVersion.VERSION,
                version = "2.0",
            };

            m_axisInverter = invertAxis.Create();
        }

        GameObject m_tmpParent = null;

        public virtual void Prepare(GameObject go)
        {
            // コピーを作って左手系を右手系に変換する
            Copy = GameObject.Instantiate(go);
            Copy.transform.ReverseRecursive(m_axisInverter);

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

            if (Copy.transform.GetComponent<Renderer>() != null)
            {
                // should throw ?
                Debug.LogError("root mesh is not exported");
            }
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
        }

        #region Export
        static glTFNode ExportNode(Transform x, List<Transform> nodes, List<Renderer> renderers, List<SkinnedMeshRenderer> skins)
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
                var meshRenderer = x.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    node.mesh = renderers.IndexOf(meshRenderer);
                }

                var skinnedMeshRenderer = x.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                {
                    node.mesh = renderers.IndexOf(skinnedMeshRenderer);
                    node.skin = skins.IndexOf(skinnedMeshRenderer);
                }
            }

            return node;
        }

        public virtual void ExportExtensions()
        {

        }

        public virtual void Export(MeshExportSettings meshExportSettings)
        {
            var bytesBuffer = new ArrayByteBuffer(new byte[50 * 1024 * 1024]);
            var bufferIndex = glTF.AddBuffer(bytesBuffer);

            Nodes = Copy.transform.Traverse()
                .Skip(1) // exclude root object for the symmetry with the importer
                .ToList();

            #region Materials and Textures
            Materials = Nodes.SelectMany(x => x.GetSharedMaterials()).Where(x => x != null).Distinct().ToList();

            TextureManager = new TextureExporter();

            var materialExporter = CreateMaterialExporter();
            glTF.materials = Materials.Select(x => materialExporter.ExportMaterial(x, TextureManager)).ToList();
            #endregion

            #region Meshes
            var unityMeshes = MeshWithRenderer.FromNodes(Nodes).Where(x => x.Mesh.vertices.Any()).ToList();

            MeshBlendShapeIndexMap = new Dictionary<Mesh, Dictionary<int, int>>();
            foreach (var (mesh, gltfMesh, blendShapeIndexMap) in MeshExporter.ExportMeshes(
                    glTF, bufferIndex, unityMeshes, Materials, meshExportSettings, m_axisInverter))
            {
                glTF.meshes.Add(gltfMesh);
                if (!MeshBlendShapeIndexMap.ContainsKey(mesh))
                {
                    // 同じmeshが複数回現れた
                    MeshBlendShapeIndexMap.Add(mesh, blendShapeIndexMap);
                }
            }
            Meshes = unityMeshes.Select(x => x.Mesh).ToList();
            #endregion

            #region Nodes and Skins
            var unitySkins = unityMeshes
                .Where(x => x.UniqueBones != null)
                .ToList();
            glTF.nodes = Nodes.Select(x => ExportNode(x, Nodes, unityMeshes.Select(y => y.Renderer).ToList(), unitySkins.Select(y => y.Renderer as SkinnedMeshRenderer).ToList())).ToList();
            glTF.scenes = new List<gltfScene>
                {
                    new gltfScene
                    {
                        nodes = Copy.transform.GetChildren().Select(x => Nodes.IndexOf(x)).ToArray(),
                    }
                };

            foreach (var x in unitySkins)
            {
                var matrices = x.GetBindPoses().Select(m_axisInverter.InvertMat4).ToArray();
                var accessor = glTF.ExtendBufferAndGetAccessorIndex(bufferIndex, matrices, glBufferTarget.NONE);

                var renderer = x.Renderer as SkinnedMeshRenderer;
                var skin = new glTFSkin
                {
                    inverseBindMatrices = accessor,
                    joints = x.UniqueBones.Select(y => Nodes.IndexOf(y)).ToArray(),
                    skeleton = Nodes.IndexOf(renderer.rootBone),
                };
                var skinIndex = glTF.skins.Count;
                glTF.skins.Add(skin);

                foreach (var z in Nodes.Where(y => y.Has(x.Renderer)))
                {
                    var nodeIndex = Nodes.IndexOf(z);
                    var node = glTF.nodes[nodeIndex];
                    node.skin = skinIndex;
                }
            }
            #endregion

#if UNITY_EDITOR
            #region Animations

            var clips = new List<AnimationClip>();
            var animator = Copy.GetComponent<Animator>();
            var animation = Copy.GetComponent<Animation>();
            if (animator != null)
            {
                clips = AnimationExporter.GetAnimationClips(animator);
            }
            else if (animation != null)
            {
                clips = AnimationExporter.GetAnimationClips(animation);
            }

            if (clips.Any())
            {
                foreach (AnimationClip clip in clips)
                {
                    var animationWithCurve = AnimationExporter.Export(clip, Copy.transform, Nodes);

                    foreach (var kv in animationWithCurve.SamplerMap)
                    {
                        var sampler = animationWithCurve.Animation.samplers[kv.Key];

                        var inputAccessorIndex = glTF.ExtendBufferAndGetAccessorIndex(bufferIndex, kv.Value.Input);
                        sampler.input = inputAccessorIndex;

                        var outputAccessorIndex = glTF.ExtendBufferAndGetAccessorIndex(bufferIndex, kv.Value.Output);
                        sampler.output = outputAccessorIndex;

                        // modify accessors
                        var outputAccessor = glTF.accessors[outputAccessorIndex];
                        var channel = animationWithCurve.Animation.channels.First(x => x.sampler == kv.Key);
                        switch (glTFAnimationTarget.GetElementCount(channel.target.path))
                        {
                            case 1:
                                outputAccessor.type = "SCALAR";
                                //outputAccessor.count = ;
                                break;
                            case 3:
                                outputAccessor.type = "VEC3";
                                outputAccessor.count /= 3;
                                break;

                            case 4:
                                outputAccessor.type = "VEC4";
                                outputAccessor.count /= 4;
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                    animationWithCurve.Animation.name = clip.name;
                    glTF.animations.Add(animationWithCurve.Animation);
                }
            }
            #endregion
#endif

            ExportExtensions();

            // Extension で Texture が増える場合があるので最後に呼ぶ
            for (int i = 0; i < TextureManager.Exported.Count; ++i)
            {
                var unityTexture = TextureManager.Exported[i];
                glTF.PushGltfTexture(bufferIndex, unityTexture);
            }
        }
        #endregion
    }
}
