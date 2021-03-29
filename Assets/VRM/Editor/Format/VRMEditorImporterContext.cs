using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public class VRMEditorImporterContext
    {
        VRMImporterContext m_context;
        UnityPath m_prefabPath;
        List<UnityPath> m_paths = new List<UnityPath>();


        public VRMEditorImporterContext(VRMImporterContext context, UnityPath prefabPath)
        {
            m_context = context;
            m_prefabPath = prefabPath;
        }

        public bool AvoidOverwriteAndLoad(UnityPath assetPath, UnityEngine.Object o)
        {
            if (o is BlendShapeAvatar)
            {
                var loaded = assetPath.LoadAsset<BlendShapeAvatar>();
                var proxy = m_context.Root.GetComponent<VRMBlendShapeProxy>();
                proxy.BlendShapeAvatar = loaded;

                return true;
            }

            if (o is BlendShapeClip)
            {
                return true;
            }

            if (o is Material)
            {
                var loaded = assetPath.LoadAsset<Material>();
                if (loaded == null)
                {
                    throw new Exception();
                }

                // replace component reference
                foreach (var mesh in m_context.Meshes)
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
            // texture is already extracted
            // else if (o is Texture2D)
            // {
            //     var textureDir = prefabPath.GetAssetFolder(".Textures");
            //     var texturePath = textureDir.Child(o.name.EscapeFilePath() + ".asset");
            //     return texturePath;
            // }
            else if (o is Mesh)
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

        /// <summary>
        /// Extract images from glb or gltf out of Assets folder.
        /// </summary>
        public void ConvertAndExtractImages(Action<IEnumerable<UnityPath>> onTextureReloaded)
        {
            // 
            // convert images(metallic roughness, occlusion map)
            //
            var task = m_context.LoadMaterialsAsync();
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

            //
            // extract converted textures
            //
            var subAssets = m_context.TextureFactory.Textures
                    .Where(x => x.IsUsed)
                    .Select(x => x.Texture)
                    .ToArray();
            var vrmTextures = new VRMTextureEnumerator(m_context.VRM);
            var dirName = $"{m_prefabPath.FileNameWithoutExtension}.Textures";
            TextureExtractor.ExtractTextures(m_context.Parser, m_prefabPath.Parent.Child(dirName), vrmTextures.Enumerate, subAssets, _ => { }, onTextureReloaded);
        }

        bool SaveAsAsset(UnityEngine.Object o)
        {
            if (o is GameObject)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)))
            {
                // already exists. not dispose
#if VRM_DEVELOP                    
                Debug.Log($"Loaded. skip: {o}");
#endif
                return true;
            }

            var assetPath = GetAssetPath(m_prefabPath, o);
            if (assetPath.IsNull)
            {
                // not dispose
                return true;
            }

            //             if (assetPath.IsFileExists)
            //             {
            //                 if (AvoidOverwriteAndLoad(assetPath, o))
            //                 {
            // #if VRM_DEVELOP                    
            //                     Debug.Log($"AvoidOverwriteAndLoad: {assetPath}");
            // #endif
            //                     // 上書きせずに既存のアセットからロードして置き換えた
            //                     return true;
            //                 }
            //             }

            // アセットとして書き込む
            assetPath.Parent.EnsureFolder();
            assetPath.CreateAsset(o);
            m_paths.Add(assetPath);

            // 所有権が移動
            return true;
        }

        public void SaveAsAsset()
        {
            m_context.ShowMeshes();

            //
            // save sub assets
            //
            m_paths.Clear();
            m_paths.Add(m_prefabPath);
            m_context.TransferOwnership(SaveAsAsset);

            // Create or update Main Asset
            if (m_prefabPath.IsFileExists)
            {
                Debug.LogFormat("replace prefab: {0}", m_prefabPath);
                var prefab = m_prefabPath.LoadAsset<GameObject>();
                PrefabUtility.SaveAsPrefabAssetAndConnect(m_context.Root, m_prefabPath.Value, InteractionMode.AutomatedAction);

            }
            else
            {
                Debug.LogFormat("create prefab: {0}", m_prefabPath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(m_context.Root, m_prefabPath.Value, InteractionMode.AutomatedAction);
            }

            foreach (var x in m_paths)
            {
                x.ImportAsset();
            }
        }
    }
}
