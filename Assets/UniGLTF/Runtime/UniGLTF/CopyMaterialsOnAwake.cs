using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// https://github.com/vrm-c/UniVRM/issues/2675
    /// https://github.com/vrm-c/UniVRM/pull/2677
    /// 
    /// Resolves unintended material sharing between two models.
    /// 
    /// example
    /// ```cs
    /// GameObject prefab;
    /// 
    /// var vrm1 = Instantiate(prefab)
    /// var vrm2 = Instantiate(prefab)
    /// // vrm1 and vrm2 sharing same material
    /// 
    /// // use copy
    /// vrm1.AddComponent<CopyMaterialsOnAwake>();
    /// vrm2.AddComponent<CopyMaterialsOnAwake>();
    /// ```
    /// </summary>
    public class CopyMaterialsOnAwake : MonoBehaviour
    {
        Dictionary<Material, Material> copyMap = new();

        private List<Material> originalMaterials;
        private List<Material> copyMaterials;

        Material GetOrCopyMaterial(Material src)
        {
            if (!src)
            {
                return default;
            }

            if (copyMap.TryGetValue(src, out var copy))
            {
                return copy;
            }

            copy = new Material(src);
            // copy.name = src.name + ".copy";
            // Debug.Log($"copy {src} to {copy}");
            // 名前を変えてはだめ。vrm Expression が動かない
            copyMap.Add(src, copy);
            originalMaterials.Add(src);
            copyMaterials.Add(copy);
            return copy;
        }

        void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                var sharedMaterials = r.sharedMaterials;
                for (int i = 0; i < sharedMaterials.Length; ++i)
                {
                    var m = sharedMaterials[i];
                    var replace = GetOrCopyMaterial(m);
                    sharedMaterials[i] = replace;
                }
                r.sharedMaterials = sharedMaterials;
            }
        }

        void OnDestroy()
        {
            foreach (var copy in copyMaterials)
            {
                UniGLTFLogger.Log($"GetOrCopyMaterial.OnDestroy: destroy {copy}");
                Material.Destroy(copy);
            }
        }
    }
}
