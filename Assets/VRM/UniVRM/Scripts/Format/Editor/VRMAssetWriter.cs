#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public static class VRMAssetWriter
    {
        interface ISubAssetWriter
        {
            void WriteIfHas(GameObject go);
        }

        class SubAssetWriter<T>: ISubAssetWriter where T : UnityEngine.Object
        {
            HashSet<T> m_set = new HashSet<T>();

            String m_assetPath;
            public delegate IEnumerable<T> GetterFunc(GameObject go);
            GetterFunc m_getter;
            public SubAssetWriter(string assetPath, GetterFunc getter)
            {
                m_assetPath = assetPath;
                m_getter = getter;
            }

            public void WriteIfHas(GameObject go)
            {
                foreach(var o in m_getter(go))
                {
                    if (!m_set.Contains(o))
                    {
                        AssetDatabase.AddObjectToAsset(o, m_assetPath);
                        m_set.Add(o);
                    }
                }
            }
        }

        static IEnumerable<Mesh> GetMeshs(GameObject go)
        {
            var skinnedMesh = go.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMesh != null)
            {
                yield return skinnedMesh.sharedMesh;
            }

            var filter = go.GetComponent<MeshFilter>();
            if (filter != null)
            {
                yield return filter.sharedMesh;
            }
        }

        static IEnumerable<Material> GetMaterials(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.sharedMaterials;
            }
            else
            {
                return Enumerable.Empty<Material>();
            }
        }

        static IEnumerable<Texture2D> GetTextures(GameObject go)
        {
            foreach (var m in GetMaterials(go))
            {
                foreach(Texture2D x in m.GetTextures())
                {
                    if (x != null)
                    {
                        yield return x;
                    }
                }
            }
        }

        static IEnumerable<Avatar> GetAvatars(GameObject go)
        {
            var animator = go.GetComponent<Animator>();
            if(animator!=null && animator.avatar != null)
            {
                yield return animator.avatar;
            }
        }

        static IEnumerable<BlendShapeClip> GetBlendShapeClips(GameObject go)
        {
            var proxy = go.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null && proxy.BlendShapeAvatar != null)
            {
                return proxy.BlendShapeAvatar.Clips;
            }
            else
            {
                return Enumerable.Empty<BlendShapeClip>();
            }
        }

        static IEnumerable<BlendShapeAvatar> GetBlendShapeAvatars(GameObject go)
        {
            var proxy = go.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null && proxy.BlendShapeAvatar != null)
            {
                yield return proxy.BlendShapeAvatar;
            }
        }

        static IEnumerable<UniHumanoid.AvatarDescription> GetAvatarDecriptions(GameObject go)
        {
            var humanoid = go.GetComponent<VRMHumanoidDescription>();
            if (humanoid!=null && humanoid.Description != null)
            {
                yield return humanoid.Description;
            }
            else
            {
                var animator = go.GetComponent<Animator>();
                if(animator!=null && animator.avatar)
                {
                    var description= UniHumanoid.AvatarDescription.CreateFrom(animator.avatar);
                    if (description != null)
                    {
                        description.name = "AvatarDescription";
                        yield return description;
                    }
                }
            }
        }

        static IEnumerable<Texture2D> GetThumbnails(GameObject go)
        {
            var meta = go.GetComponent<VRMMetaInformation>();
            if (meta != null && meta.Thumbnail != null)
            {
                yield return meta.Thumbnail;
            }
        }

        static IEnumerable<UnityEngine.Object> GetSubAssets(String prefabPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(prefabPath);
        }

        public static void SaveAsPrefab(GameObject root, String path)
        {
            var prefabPath = path.ToUnityRelativePath();
            Debug.LogFormat("SaveAsPrefab: {0}", prefabPath);

            // clear subassets
            if (File.Exists(prefabPath))
            {
                //Debug.LogFormat("Exist: {0}", m_prefabPath);

                // clear subassets
                foreach (var x in GetSubAssets(prefabPath))
                {
                    if (x is Transform
                        || x is GameObject)
                    {
                        continue;
                    }
                    GameObject.DestroyImmediate(x, true);
                }
            }

            // add subassets
            var writers = new ISubAssetWriter[]{
                new SubAssetWriter<Texture2D>(prefabPath, GetTextures),
                new SubAssetWriter<Material>(prefabPath, GetMaterials),
                new SubAssetWriter<Mesh>(prefabPath, GetMeshs),
                new SubAssetWriter<Avatar>(prefabPath, GetAvatars),
                // VRM Objects
                new SubAssetWriter<BlendShapeClip>(prefabPath, GetBlendShapeClips),
                new SubAssetWriter<BlendShapeAvatar>(prefabPath, GetBlendShapeAvatars),
                new SubAssetWriter<UniHumanoid.AvatarDescription>(prefabPath, GetAvatarDecriptions),
                new SubAssetWriter<Texture2D>(prefabPath, GetThumbnails),
            };
            foreach (var x in root.transform.Traverse())
            {
                foreach (var writer in writers)
                {
                    writer.WriteIfHas(x.gameObject);
                }
            }

            ///
            /// create prefab, after subasset AssetDatabase.AddObjectToAsset
            ///
            if (File.Exists(prefabPath))
            {
                //Debug.LogFormat("ReplacePrefab: {0}", m_prefabPath);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                PrefabUtility.ReplacePrefab(root, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                //Debug.LogFormat("CreatePrefab: {0}", m_prefabPath);
                PrefabUtility.CreatePrefab(prefabPath, root, ReplacePrefabOptions.ConnectToPrefab);
            }
        }
    }
}
#endif