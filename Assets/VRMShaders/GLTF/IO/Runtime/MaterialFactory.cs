using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace VRMShaders
{
    public delegate Task<Texture2D> GetTextureAsyncFunc(TextureImportParam param);

    public class MaterialFactory : IDisposable
    {
        Dictionary<string, Material> m_externalMap;

        public MaterialFactory(IEnumerable<(string, UnityEngine.Object)> externalMap)
        {
            if (externalMap == null)
            {
                externalMap = Enumerable.Empty<(string, UnityEngine.Object)>();
            }
            m_externalMap = externalMap
                .Select(kv => (kv.Item1, kv.Item2 as Material))
                .Where(kv => kv.Item2 != null)
                .ToDictionary(kv => kv.Item1, kv => kv.Item2)
                ;
        }

        public struct MaterialLoadInfo
        {
            public readonly Material Asset;
            public readonly bool UseExternal;

            public bool IsSubAsset => !UseExternal;

            public MaterialLoadInfo(Material asset, bool useExternal)
            {
                Asset = asset;
                UseExternal = useExternal;
            }
        }

        List<MaterialLoadInfo> m_materials = new List<MaterialLoadInfo>();
        public IReadOnlyList<MaterialLoadInfo> Materials => m_materials;
        void Remove(Material material)
        {
            var index = m_materials.FindIndex(x => x.Asset == material);
            if (index >= 0)
            {
                m_materials.RemoveAt(index);

            }
        }

        public void Dispose()
        {
            foreach (var x in m_materials)
            {
                if (!x.UseExternal)
                {
                    // 外部の '.asset' からロードしていない
#if VRM_DEVELOP
                    // Debug.Log($"Destroy {x.Asset}");
#endif
                    UnityEngine.Object.DestroyImmediate(x.Asset, false);
                }
            }
        }

        /// <summary>
        /// 所有権(Dispose権)を移譲する
        /// 
        /// 所有権を移動する関数。
        /// 
        /// * 所有権が移動する。return true => ImporterContext.Dispose の対象から外れる
        /// * 所有権が移動しない。return false => Importer.Context.Dispose でDestroyされる
        /// 
        /// </summary>
        /// <param name="take"></param>
        public void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            var list = new List<Material>();
            foreach (var x in m_materials)
            {
                if (!x.UseExternal)
                {
                    // 外部の '.asset' からロードしていない
                    if (take(x.Asset))
                    {
                        list.Add(x.Asset);
                    }
                }
            }
            foreach (var x in list)
            {
                Remove(x);
            }
        }

        public Material GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index].Asset;
        }

        public async Task<Material> LoadAsync(MaterialImportParam param, GetTextureAsyncFunc getTexture)
        {
            if (m_externalMap.TryGetValue(param.Name, out Material material))
            {
                m_materials.Add(new MaterialLoadInfo(material, true));
                return material;
            }

            if (getTexture == null)
            {
                getTexture = (_) => Task.FromResult<Texture2D>(null);
            }

            material = new Material(Shader.Find(param.ShaderName));
            material.name = param.Name;

            foreach(var kv in param.TextureSlots)
            {
                var texture = await getTexture(kv.Value);
                if(texture!=null){
                    material.SetTexture(kv.Key, texture);
                    SetTextureOffsetAndScale(material, kv.Key, kv.Value.Offset, kv.Value.Scale);
                }
            }

            foreach(var kv in param.Colors)
            {
                material.SetColor(kv.Key, kv.Value);
            }

            foreach(var kv in param.Vectors)
            {
                material.SetVector(kv.Key, kv.Value);
            }

            foreach(var kv in param.FloatValues)
            {
                material.SetFloat(kv.Key, kv.Value);
            }

            foreach(var action in param.Actions)
            {
                action(material);
            }

            m_materials.Add(new MaterialLoadInfo(material, false));

            return material;
        }

        public static void SetTextureOffsetAndScale(Material material, string propertyName, Vector2 offset, Vector2 scale)
        {
            material.SetTextureOffset(propertyName, offset);
            material.SetTextureScale(propertyName, scale);
        }
    }
}
