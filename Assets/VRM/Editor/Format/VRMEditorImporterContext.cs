using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public class VRMEditorImporterContext
    {
        VRMImporterContext m_context;
        UnityPath m_prefabPath;
        List<UnityPath> m_paths = new List<UnityPath>();

        public ITextureDescriptorGenerator TextureDescriptorGenerator => m_context.TextureDescriptorGenerator;

        public VRMEditorImporterContext(VRMImporterContext context, UnityPath prefabPath)
        {
            m_context = context;
            m_prefabPath = prefabPath;
        }

        public UnityPath GetAssetPath(UnityPath prefabPath, UnityEngine.Object o)
        {
            if (o is BlendShapeAvatar
                || o is BlendShapeClip)
            {
                var dir = prefabPath.GetAssetFolder(".BlendShapes");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is Avatar)
            {
                var dir = prefabPath.GetAssetFolder(".Avatar");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is VRMMetaObject)
            {
                var dir = prefabPath.GetAssetFolder(".MetaObject");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is UniHumanoid.AvatarDescription)
            {
                var dir = prefabPath.GetAssetFolder(".AvatarDescription");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is Material)
            {
                var materialDir = prefabPath.GetAssetFolder(".Materials");
                var materialPath = materialDir.Child(o.name.EscapeFilePath() + ".asset");
                return materialPath;
            }
            else if (o is Mesh)
            {
                var meshDir = prefabPath.GetAssetFolder(".Meshes");
                var meshPath = meshDir.Child(o.name.EscapeFilePath() + ".asset");
                return meshPath;
            }
            else if (o is AnimationClip)
            {
                var meshDir = prefabPath.GetAssetFolder(".Animations");
                var meshPath = meshDir.Child(o.name.EscapeFilePath() + ".anim");
                return meshPath;
            }
            else
            {
                return default(UnityPath);
            }
        }

        /// <summary>
        /// Extract images from glb or gltf out of Assets folder.
        /// </summary>
        public void ConvertAndExtractImages(Action<IEnumerable<UnityPath>> onTextureReloaded)
        {
            //
            // convert images(metallic roughness, occlusion map)
            //
            var task = m_context.LoadMaterialsAsync(new ImmediateCaller());
            if (!task.IsCompleted)
            {
                throw new Exception();
            }
            if (task.IsFaulted)
            {
                if (task.Exception is AggregateException ae && ae.InnerExceptions.Count == 1)
                {
                    throw ae.InnerException;
                }
                else
                {
                    throw task.Exception;
                }
            }

            // Convert thumbnail image
            var task2 = m_context.ReadMetaAsync(new ImmediateCaller());
            if (!task2.IsCompleted || task2.IsCanceled || task2.IsFaulted)
            {
                throw new Exception();
            }

            //
            // extract converted textures
            //
            var subAssets = m_context.TextureFactory.ConvertedTextures;
            var vrmTextures = new BuiltInVrmMaterialDescriptorGenerator(m_context.VRM);
            var dirName = $"{m_prefabPath.FileNameWithoutExtension}.Textures";
            TextureExtractor.ExtractTextures(m_context.Data, m_prefabPath.Parent.Child(dirName), m_context.TextureDescriptorGenerator, subAssets, (_x, _y) => { }, onTextureReloaded);
        }

        void SaveAsAsset(SubAssetKey _, UnityEngine.Object o)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)))
            {
                if (Symbols.VRM_DEVELOP)
                {
                    // 来ない？
                    Debug.LogWarning($"{o} already exists. skip write");
                }
                return;
            }

            var assetPath = GetAssetPath(m_prefabPath, o);
            if (!assetPath.IsNull)
            {
                // アセットとして書き込む
                assetPath.Parent.EnsureFolder();
                assetPath.CreateAsset(o);
                m_paths.Add(assetPath);
            }
        }

        public void SaveAsAsset(UniGLTF.RuntimeGltfInstance loaded)
        {
            loaded.ShowMeshes();

            //
            // save sub assets
            //
            m_paths.Clear();
            m_paths.Add(m_prefabPath);
            loaded.TransferOwnership(SaveAsAsset);
            var root = loaded.Root;

            // Remove RuntimeGltfInstance component before saving as a prefab.
            UnityObjectDestroyer.DestroyRuntimeOrEditor(loaded);

            // Create or update Main Asset
            if (m_prefabPath.IsFileExists)
            {
                UniGLTFLogger.Log($"replace prefab: {m_prefabPath}");
                var prefab = m_prefabPath.LoadAsset<GameObject>();
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, m_prefabPath.Value, InteractionMode.AutomatedAction);
            }
            else
            {
                UniGLTFLogger.Log($"create prefab: {m_prefabPath}");
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, m_prefabPath.Value, InteractionMode.AutomatedAction);
            }

            // destroy GameObject on scene
            UnityObjectDestroyer.DestroyRuntimeOrEditor(root);

            foreach (var x in m_paths)
            {
                x.ImportAsset();
            }
        }
    }
}
