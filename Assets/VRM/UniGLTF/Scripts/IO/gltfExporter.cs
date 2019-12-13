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
        const string CONVERT_HUMANOID_KEY = UniGLTFVersion.MENU + "/Export";

#if UNITY_EDITOR
        [MenuItem(CONVERT_HUMANOID_KEY, true, 1)]
        private static bool ExportValidate()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;
            var path = EditorUtility.SaveFilePanel(
                    "Save glb",
                    "",
                    go.name + ".glb",
                    "glb");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export();
            }
            var bytes = gltf.ToGlbBytes();
            File.WriteAllBytes(path, bytes);

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
                AssetDatabase.Refresh();
            }
        }
#endif

        glTF glTF;

        public bool UseSparseAccessorForBlendShape
        {
            get;
            set;
        }

        public bool ExportOnlyBlendShapePosition
        {
            get;
            set;
        }

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

        /// <summary>
        /// このエクスポーターがサポートするExtension
        /// </summary>
        protected virtual IEnumerable<string> ExtensionUsed
        {
            get
            {
                yield return glTF_KHR_materials_unlit.ExtensionName;
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

        public static glTF Export(GameObject go)
        {
            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export();
            }
            return gltf;
        }

        public virtual void Prepare(GameObject go)
        {
            // コピーを作って、Z軸を反転することで左手系を右手系に変換する
            Copy = GameObject.Instantiate(go);
            Copy.transform.ReverseZRecursive();
        }

        public void Export()
        {
            FromGameObject(glTF, Copy, UseSparseAccessorForBlendShape);
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

        void FromGameObject(glTF gltf, GameObject go, bool useSparseAccessorForMorphTarget = false)
        {
            var bytesBuffer = new ArrayByteBuffer(new byte[50 * 1024 * 1024]);
            var bufferIndex = gltf.AddBuffer(bytesBuffer);

            GameObject tmpParent = null;
            if (go.transform.childCount == 0)
            {
                tmpParent = new GameObject("tmpParent");
                go.transform.SetParent(tmpParent.transform, true);
                go = tmpParent;
            }

            try
            {

                Nodes = go.transform.Traverse()
                    .Skip(1) // exclude root object for the symmetry with the importer
                    .ToList();

                #region Materials and Textures
                Materials = Nodes.SelectMany(x => x.GetSharedMaterials()).Where(x => x != null).Distinct().ToList();
                var unityTextures = Materials.SelectMany(x => TextureIO.GetTextures(x)).Where(x => x.Texture != null).Distinct().ToList();

                TextureManager = new TextureExportManager(unityTextures.Select(x => x.Texture));

                var materialExporter = CreateMaterialExporter();
                gltf.materials = Materials.Select(x => materialExporter.ExportMaterial(x, TextureManager)).ToList();

                for (int i = 0; i < unityTextures.Count; ++i)
                {
                    var unityTexture = unityTextures[i];
                    TextureIO.ExportTexture(gltf, bufferIndex, TextureManager.GetExportTexture(i), unityTexture.TextureType);
                }
                #endregion


                #region Meshes
                var unityMeshes = Nodes
                    .Select(x => new MeshWithRenderer
                    {
                        Mesh = x.GetSharedMesh(),
                        Renderer = x.GetComponent<Renderer>(),
                    })
                    .Where(x =>
                    {
                        if (x.Mesh == null)
                        {
                            return false;
                        }
                        if (x.Renderer.sharedMaterials == null
                        || x.Renderer.sharedMaterials.Length == 0)
                        {
                            return false;
                        }

                        return true;
                    })
                    .ToList();
                MeshExporter.ExportMeshes(gltf, bufferIndex, unityMeshes, Materials, useSparseAccessorForMorphTarget, ExportOnlyBlendShapePosition);
                Meshes = unityMeshes.Select(x => x.Mesh).ToList();
                #endregion

                #region Nodes and Skins
                var unitySkins = Nodes
                    .Select(x => x.GetComponent<SkinnedMeshRenderer>()).Where(x =>
                        x != null
                        && x.bones != null
                        && x.bones.Length > 0)
                    .ToList();
                gltf.nodes = Nodes.Select(x => ExportNode(x, Nodes, unityMeshes.Select(y => y.Renderer).ToList(), unitySkins)).ToList();
                gltf.scenes = new List<gltfScene>
                {
                    new gltfScene
                    {
                        nodes = go.transform.GetChildren().Select(x => Nodes.IndexOf(x)).ToArray(),
                    }
                };

                foreach (var x in unitySkins)
                {
                    var matrices = x.sharedMesh.bindposes.Select(y => y.ReverseZ()).ToArray();
                    var accessor = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, matrices, glBufferTarget.NONE);

                    var skin = new glTFSkin
                    {
                        inverseBindMatrices = accessor,
                        joints = x.bones.Select(y => Nodes.IndexOf(y)).ToArray(),
                        skeleton = Nodes.IndexOf(x.rootBone),
                    };
                    var skinIndex = gltf.skins.Count;
                    gltf.skins.Add(skin);

                    foreach (var z in Nodes.Where(y => y.Has(x)))
                    {
                        var nodeIndex = Nodes.IndexOf(z);
                        var node = gltf.nodes[nodeIndex];
                        node.skin = skinIndex;
                    }
                }
                #endregion

#if UNITY_EDITOR
                #region Animations

                var clips = new List<AnimationClip>();
                var animator = go.GetComponent<Animator>();
                var animation = go.GetComponent<Animation>();
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
                        var animationWithCurve = AnimationExporter.Export(clip, go.transform, Nodes);

                        foreach (var kv in animationWithCurve.SamplerMap)
                        {
                            var sampler = animationWithCurve.Animation.samplers[kv.Key];

                            var inputAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, kv.Value.Input);
                            sampler.input = inputAccessorIndex;

                            var outputAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, kv.Value.Output);
                            sampler.output = outputAccessorIndex;

                            // modify accessors
                            var outputAccessor = gltf.accessors[outputAccessorIndex];
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
                        gltf.animations.Add(animationWithCurve.Animation);
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
