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

        public VRMEditorImporterContext(VRMImporterContext context)
        {
            m_context = context;
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

        public UnityPath GetAssetPath(UnityPath prefabPath, UnityEngine.Object o, bool meshAsSubAsset)
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
            else if (o is Texture2D)
            {
                var textureDir = prefabPath.GetAssetFolder(".Textures");
                var texturePath = textureDir.Child(o.name.EscapeFilePath() + ".asset");
                return texturePath;
            }
            else if (o is Mesh && !meshAsSubAsset)
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
        /// <param name="assetPath"></param>
        public void ConvertAndExtractImages(UnityPath assetPath, Action onTextureReloaded)
        {
            // 
            // convert images(metallic roughness, occlusion map)
            //
            var task = m_context.MaterialFactory.LoadMaterialsAsync(default(ImmediateCaller), m_context.TextureFactory.GetTextureAsync);
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
            var prefabParentDir = assetPath.Parent;
            var folder = assetPath.GetAssetFolder(".Textures");
            TextureExtractor.ExtractTextures(assetPath.Value, subAssets, _ => { }, onTextureReloaded);
        }

        public void SaveAsAsset(UnityPath prefabPath, bool meshAsSubAsset = false)
        {
            m_context.ShowMeshes();

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

            // backup
            var root = m_context.Root;

            m_context.TransferOwnership(o =>
            {

                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)))
                {
                    // already exists
                    return;
                }

                var assetPath = GetAssetPath(prefabPath, o, meshAsSubAsset);
                if (!assetPath.IsNull)
                {
                    if (assetPath.IsFileExists)
                    {
                        if (AvoidOverwriteAndLoad(assetPath, o))
                        {
                            // 上書きせずに既存のアセットからロードして置き換えた
                            return;
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
                    if (o is GameObject)
                    {

                    }
                    else
                    {
                        prefabPath.AddObjectToAsset(o);
                    }
                }

            });

            // Create or update Main Asset
            if (prefabPath.IsFileExists)
            {
                Debug.LogFormat("replace prefab: {0}", prefabPath);
                var prefab = prefabPath.LoadAsset<GameObject>();
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath.Value, InteractionMode.AutomatedAction);

            }
            else
            {
                Debug.LogFormat("create prefab: {0}", prefabPath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath.Value, InteractionMode.AutomatedAction);
            }
            foreach (var x in paths)
            {
                x.ImportAsset();
            }
        }
    }
}
