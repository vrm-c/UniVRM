using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// ImporterContext の Load 結果の GltfModel
    /// 
    /// Runtime でモデルを Destory したときに関連リソース(Texture, Material...などの UnityEngine.Object)を自動的に Destroy する。
    /// </summary>
    public class RuntimeGltfInstance : MonoBehaviour, IResponsibilityForDestroyObjects
    {
        /// <summary>
        /// this is UniGLTF root gameObject
        /// </summary>
        public GameObject Root => this.gameObject;

        List<(SubAssetKey, UnityEngine.Object)> m_resources = new List<(SubAssetKey, UnityEngine.Object)>();

        public IReadOnlyList<(SubAssetKey, UnityEngine.Object)> RuntimeResources => m_resources;

        public static RuntimeGltfInstance AttachTo(GameObject go, ImporterContext context)
        {
            var loaded = go.AddComponent<RuntimeGltfInstance>();
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
