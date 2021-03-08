using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UniGLTF
{
    /// <summary>
    /// Editor で Asset 化する場合専用
    /// </summary>
    public class EditorImporterContext
    {
        ImporterContext m_context;

        public EditorImporterContext(ImporterContext context)
        {
            m_context = context;
        }

        // public virtual IEnumerable<UnityEngine.Object> ObjectsForSubAsset()
        // {
        //     foreach (var x in m_context.TextureFactory.ObjectsForSubAsset())
        //     {
        //         yield return x;
        //     }
        //     foreach (var x in m_context.MaterialFactory.ObjectsForSubAsset())
        //     {
        //         yield return x;
        //     }
        //     foreach (var x in m_context.Meshes) { yield return x.Mesh; }
        //     foreach (var x in m_context.AnimationClips) { yield return x; }
        // }

        // /// <summary>
        // /// Destroy assets that created ImporterContext. This function is clean up for importer error.
        // /// </summary>
        // public virtual void EditorDestroyRootAndAssets()
        // {
        //     // Remove hierarchy
        //     if (m_context.Root != null) GameObject.DestroyImmediate(m_context.Root);

        //     // Remove resources. materials, textures meshes etc...
        //     foreach (var o in ObjectsForSubAsset())
        //     {
        //         UnityEngine.Object.DestroyImmediate(o, true);
        //     }
        // }

        public virtual UnityPath GetAssetPath(UnityPath prefabPath, UnityEngine.Object o, bool meshAsSubAsset)
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

        public virtual bool AvoidOverwriteAndLoad(UnityPath assetPath, UnityEngine.Object o)
        {
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
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(m_context.Root, prefabPath.Value, InteractionMode.AutomatedAction);
#else
                PrefabUtility.ReplacePrefab(Root, prefab, ReplacePrefabOptions.ReplaceNameBased);
#endif

            }
            else
            {
                Debug.LogFormat("create prefab: {0}", prefabPath);
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(m_context.Root, prefabPath.Value, InteractionMode.AutomatedAction);
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
            for (int i = 0; i < m_context.GLTF.textures.Count; ++i)
            {
                folder.EnsureFolder();

                var gltfTexture = m_context.GLTF.textures[i];
                var gltfImage = m_context.GLTF.images[gltfTexture.source];
                var src = m_context.Storage.GetPath(gltfImage.uri);
                if (UnityPath.FromFullpath(src).IsUnderAssetsFolder)
                {
                    // asset is exists.
                }
                else
                {
                    var byteSegment = m_context.GLTF.GetImageBytes(m_context.Storage, gltfTexture.source);
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
            m_context.TextureFactory.ImageBaseDir = prefabParentDir;
        }
    }
}
