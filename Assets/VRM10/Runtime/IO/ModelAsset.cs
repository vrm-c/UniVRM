using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{

    [Serializable]
    public class ModelAsset : IDisposable
    {
        public GameObject Root;
        public Avatar HumanoidAvatar;
        public List<Texture2D> Textures = new List<Texture2D>();
        public List<Material> Materials = new List<Material>();
        public List<Mesh> Meshes = new List<Mesh>();
        public List<Renderer> Renderers = new List<Renderer>();

        public readonly ModelMap Map = new ModelMap();
        public List<ScriptableObject> ScriptableObjects = new List<ScriptableObject>();

        private Animator _animator;
        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = Root.GetComponent<Animator>();
                }
                return _animator;
            }
        }

        public void Dispose()
        {
            GameObject.Destroy(Root);
            UnityEngine.Object.Destroy(HumanoidAvatar);
            foreach (var v in Textures)
            {
                UnityEngine.Object.DestroyImmediate(v);
            }
            foreach (var v in Materials)
            {
                UnityEngine.Object.DestroyImmediate(v);
            }
            foreach (var v in Meshes)
            {
                UnityEngine.Object.DestroyImmediate(v);
            }
            foreach (var v in ScriptableObjects)
            {
                ScriptableObject.DestroyImmediate(v);
            }
        }

#if UNITY_EDITOR
        public void DisposeEditor()
        {
            if (!Application.isPlaying)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Root)))
                {
                    GameObject.DestroyImmediate(Root);
                }
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(HumanoidAvatar)))
                {
                    UnityEngine.Object.DestroyImmediate(HumanoidAvatar);
                }

                foreach (var v in Textures)
                {
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(v)))
                    {
                        UnityEngine.Object.DestroyImmediate(v);
                    }
                }
                foreach (var v in Materials)
                {
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(v)))
                    {
                        UnityEngine.Object.DestroyImmediate(v);
                    }
                }
                foreach (var v in Meshes)
                {
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(v)))
                    {
                        UnityEngine.Object.DestroyImmediate(v);
                    }
                }
                foreach (var v in ScriptableObjects)
                {
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(v)))
                    {
                        ScriptableObject.DestroyImmediate(v);
                    }
                }
            }
        }
#endif
    }
}
