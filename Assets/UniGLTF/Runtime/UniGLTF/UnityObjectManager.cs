using System;
using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// Mesh, Material, Texture などを抱えておいて確実に破棄できるようにする
    /// </summary>
    public class UnityObjectManager : MonoBehaviour, IResponsibilityForDestroyObjects
    {
        List<(SubAssetKey, UnityEngine.Object)> m_resources = new List<(SubAssetKey, UnityEngine.Object)>();

        public static UnityObjectManager AttachTo(GameObject go, ImporterContext context)
        {
            var loaded = go.AddComponent<UnityObjectManager>();
            context.TransferOwnership((k, o) =>
            {
                loaded.m_resources.Add((k, o));
            });
            return loaded;
        }

        public void ShowMeshes()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }

        public void EnableUpdateWhenOffscreen()
        {
            foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.updateWhenOffscreen = true;
            }
        }

        void OnDestroy()
        {
            Debug.Log("UnityResourceDestroyer.OnDestroy");
            foreach (var (key, x) in m_resources)
            {
                UnityObjectDestoyer.DestroyRuntimeOrEditor(x);
            }
        }

        public void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            foreach (var (key, x) in m_resources.ToArray())
            {
                take(key, x);
                m_resources.Remove((key, x));
            }
        }

        public void Dispose()
        {
            UnityObjectDestoyer.DestroyRuntimeOrEditor(this.gameObject);
        }
    }
}
