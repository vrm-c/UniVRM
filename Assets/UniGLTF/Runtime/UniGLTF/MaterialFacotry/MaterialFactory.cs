using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniGLTF
{
    public class MaterialFactory : IDisposable
    {
        glTF m_gltf;
        IStorage m_storage;
        public MaterialFactory(glTF gltf, IStorage storage)
        {
            m_gltf = gltf;
            m_storage = storage;
        }

        public UnityPath ImageBaseDir { get; set; }

        public delegate Task<Material> CreateMaterialAsyncFunc(glTF glTF, int i, GetTextureAsyncFunc getTexture);
        CreateMaterialAsyncFunc m_createMaterialAsync;
        public CreateMaterialAsyncFunc CreateMaterialAsync
        {
            set
            {
                m_createMaterialAsync = value;
            }
            get
            {
                if (m_createMaterialAsync == null)
                {
                    m_createMaterialAsync = MaterialItemBase.DefaultCreateMaterialAsync;
                }
                return m_createMaterialAsync;
            }
        }

        List<Texture2D> m_textuers = new List<Texture2D>();
        Dictionary<GetTextureParam, Texture2D> m_textureCache = new Dictionary<GetTextureParam, Texture2D>();

        /// <summary>
        /// テクスチャーをロード、必要であれば変換して返す。
        /// 同じものはキャッシュを返す
        /// </summary>
        /// <param name="texture_type">変換の有無を判断する: METALLIC_GLOSS_PROP</param>
        /// <param name="roughnessFactor">METALLIC_GLOSS_PROPの追加パラメーター</param>
        /// <param name="indices">gltf の texture index</param>
        /// <returns></returns>
        public async Task<Texture2D> GetTextureAsync(GetTextureParam param)
        {
            if (m_textureCache.TryGetValue(param, out Texture2D texture))
            {
                return texture;
            }

            {
                var defaultParam = GetTextureParam.Create(param.Index0.Value);
                if (!m_textureCache.TryGetValue(defaultParam, out texture))
                {
                    texture = await LoadTextureAsync(param.Index0.Value);
                    m_textureCache.Add(defaultParam, texture);
                }
            }

            switch (param.TextureType)
            {
                case GetTextureParam.NORMAL_PROP:
                    {
                        if (Application.isPlaying)
                        {
                            var converted = new NormalConverter().GetImportTexture(texture);
                            m_textureCache.Add(param, converted);
                            return converted;
                        }
                        else
                        {
#if UNITY_EDITOR
                            var textureAssetPath = AssetDatabase.GetAssetPath(texture);
                            if (!string.IsNullOrEmpty(textureAssetPath))
                            {
                                TextureIO.MarkTextureAssetAsNormalMap(textureAssetPath);
                            }
                            else
                            {
                                Debug.LogWarningFormat("no asset for {0}", texture);
                            }
#endif
                            m_textureCache.Add(param, texture);
                            return texture;
                        }
                    }

                case GetTextureParam.METALLIC_GLOSS_PROP:
                    {
                        // Bake roughnessFactor values into a texture.
                        var converted = new MetallicRoughnessConverter(param.MetallicFactor).GetImportTexture(texture);
                        m_textureCache.Add(param, converted);
                        return converted;
                    }

                case GetTextureParam.OCCLUSION_PROP:
                    {
                        var converted = new OcclusionConverter().GetImportTexture(texture);
                        m_textureCache.Add(param, converted);
                        return converted;
                    }

                default:
                    return texture;
            }

            throw new NotImplementedException();
        }

        List<Material> m_materials = new List<Material>();
        public IReadOnlyList<Material> Materials => m_materials;
        public void AddMaterial(Material material)
        {
            var originalName = material.name;
            int j = 2;
            while (m_materials.Any(x => x.name == material.name))
            {
                material.name = string.Format("{0}({1})", originalName, j++);
            }
            m_materials.Add(material);
        }
        public Material GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index];
        }

        public virtual Task<Texture2D> LoadTextureAsync(int index)
        {
#if UNIGLTF_USE_WEBREQUEST_TEXTURELOADER
            return UnityWebRequestTextureLoader.LoadTextureAsync(index);
#else
            return GltfTextureLoader.LoadTextureAsync(m_gltf, m_storage, index);
#endif
        }

        public IEnumerator LoadMaterials()
        {
            // using (MeasureTime("LoadMaterials"))
            {
                if (m_gltf.materials == null || m_gltf.materials.Count == 0)
                {
                    var task = CreateMaterialAsync(m_gltf, 0, GetTextureAsync);

                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
                    AddMaterial(task.Result);
                }
                else
                {
                    for (int i = 0; i < m_gltf.materials.Count; ++i)
                    {
                        var task = CreateMaterialAsync(m_gltf, i, GetTextureAsync);
                        while (!task.IsCompleted)
                        {
                            yield return null;
                        }
                        AddMaterial(task.Result);
                    }
                }
            }
            yield return null;
        }

        public void Dispose()
        {
            foreach (var x in ObjectsForSubAsset())
            {
                UnityEngine.Object.DestroyImmediate(x, true);
            }
        }

        public IEnumerable<UnityEngine.Object> ObjectsForSubAsset()
        {
            foreach (var kv in m_textureCache)
            {
                yield return kv.Value;
            }
            foreach (var x in m_materials)
            {
                yield return x;
            }
        }
    }
}
