using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public class gltfExporter : IDisposable
    {
        const string MENU_EXPORT_GLB_KEY = UniGLTFVersion.MENU + "/Export(glb)";
        const string MENU_EXPORT_GLTF_KEY = UniGLTFVersion.MENU + "/Export(gltf)";

#if UNITY_EDITOR
        [MenuItem(MENU_EXPORT_GLTF_KEY, true, 1)]
        [MenuItem(MENU_EXPORT_GLB_KEY, true, 1)]
        private static bool ExportValidate()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        [MenuItem(MENU_EXPORT_GLTF_KEY, priority = 0)]
        private static void ExportGltfFromMenu()
        {
            ExportFromMenu(false, new MeshExportSettings
            {
                ExportOnlyBlendShapePosition = false,
                UseSparseAccessorForMorphTarget = true,
            });
        }

        [MenuItem(MENU_EXPORT_GLB_KEY,  priority = 10)]
        private static void ExportGlbFromMenu()
        {
            ExportFromMenu(true, MeshExportSettings.Default);
        }

        private static void ExportFromMenu(bool isGlb, MeshExportSettings settings)
        {
            var go = Selection.activeObject as GameObject;

            var ext = isGlb ? "glb" : "gltf";

            if (go.transform.position == Vector3.zero &&
                go.transform.rotation == Quaternion.identity &&
                go.transform.localScale == Vector3.one)
            {
                var path = EditorUtility.SaveFilePanel(
                    $"Save {ext}", "", go.name + $".{ext}", $"{ext}");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var gltf = new glTF();
                using (var exporter = new gltfExporter(gltf))
                {
                    exporter.Prepare(go);
                    exporter.Export(settings);
                }

                if (isGlb)
                {
                    var bytes = gltf.ToGlbBytes();
                    File.WriteAllBytes(path, bytes);
                }
                else
                {
                    var (json, buffers) = gltf.ToGltf(path);
                    // without BOM
                    var encoding = new System.Text.UTF8Encoding(false);
                    File.WriteAllText(path, json, encoding);
                    // write to local folder
                    var dir = Path.GetDirectoryName(path);
                    foreach (var b in buffers)
                    {
                        var bufferPath = Path.Combine(dir, b.uri);
                        File.WriteAllBytes(bufferPath, b.GetBytes().ToArray());
                    }
                }

                if (path.StartsWithUnityAssetPath())
                {
                    AssetDatabase.ImportAsset(path.ToUnityRelativePath());
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "The Root transform should have Default translation, rotation and scale.", "ok");
            }
        }
#endif

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

        public TextureExportManager TextureManager;

        protected virtual IMaterialExporter CreateMaterialExporter()
        {
            return new MaterialExporter();
        }

        private ITextureExporter _textureExporter;
        public ITextureExporter TextureExporter
        {
            get
            {
                if (_textureExporter != null)
                {
                    return _textureExporter;
                }
                else
                {
                    _textureExporter = new TextureIO();
                    return _textureExporter;
                }
            }
            set
            {
                _textureExporter = value;
            }
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

        public gltfExporter(glTF gltf)
        {
            glTF = gltf;

            glTF.extensionsUsed.AddRange(ExtensionUsed);

            glTF.asset = new glTFAssets
            {
                generator = "UniGLTF-" + UniGLTFVersion.VERSION,
                version = "2.0",
            };
        }

        public virtual void Prepare(GameObject go)
        {
            // コピーを作って、Z軸を反転することで左手系を右手系に変換する
            Copy = GameObject.Instantiate(go);
            Copy.transform.ReverseZRecursive();
        }

        public void Dispose()
        {
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

        public virtual void Export(MeshExportSettings meshExportSettings)
        {
            var bytesBuffer = new ArrayByteBuffer(new byte[50 * 1024 * 1024]);
            var bufferIndex = glTF.AddBuffer(bytesBuffer);

            GameObject tmpParent = null;
            if (Copy.transform.childCount == 0)
            {
                tmpParent = new GameObject("tmpParent");
                Copy.transform.SetParent(tmpParent.transform, true);
                Copy = tmpParent;
            }

            try
            {
                Nodes = Copy.transform.Traverse()
                    .Skip(1) // exclude root object for the symmetry with the importer
                    .ToList();

                #region Materials and Textures
                Materials = Nodes.SelectMany(x => x.GetSharedMaterials()).Where(x => x != null).Distinct().ToList();
                var unityTextures = Materials.SelectMany(x => TextureExporter.GetTextures(x)).Where(x => x.texture != null).Distinct().ToList();

                TextureManager = new TextureExportManager(unityTextures.Select(x => x.texture));

                var materialExporter = CreateMaterialExporter();
                glTF.materials = Materials.Select(x => materialExporter.ExportMaterial(x, TextureManager)).ToList();

                for (int i = 0; i < unityTextures.Count; ++i)
                {
                    var unityTexture = unityTextures[i];
                    TextureExporter.ExportTexture(glTF, bufferIndex, TextureManager.GetExportTexture(i), unityTexture.textureType);
                }
                #endregion

                #region Meshes
                var unityMeshes = MeshWithRenderer.FromNodes(Nodes).ToList();

                MeshBlendShapeIndexMap = new Dictionary<Mesh, Dictionary<int, int>>();
                foreach (var (mesh, gltfMesh, blendShapeIndexMap) in MeshExporter.ExportMeshes(
                        glTF, bufferIndex, unityMeshes, Materials, meshExportSettings))
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
                    var matrices = x.GetBindPoses().Select(y => y.ReverseZ()).ToArray();
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
            }
            finally
            {
                if (tmpParent != null)
                {
                    tmpParent.transform.GetChild(0).SetParent(null);
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(tmpParent);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(tmpParent);
                    }
                }
            }
        }
        #endregion
    }
}
