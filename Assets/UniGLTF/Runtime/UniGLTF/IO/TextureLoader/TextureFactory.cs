using System;
using System.Collections.Generic;
using UniGLTF.AltTask;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public delegate Awaitable<Texture2D> GetTextureAsyncFunc(GetTextureParam param);
    public class TextureFactory : IDisposable
    {
        glTF m_gltf;
        IStorage m_storage;

        public UnityPath ImageBaseDir { get; set; }

        public TextureFactory(glTF gltf, IStorage storage)
        {
            m_gltf = gltf;
            m_storage = storage;
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
        }

        Dictionary<GetTextureParam, Texture2D> m_textureCache = new Dictionary<GetTextureParam, Texture2D>();
        public IEnumerable<Texture2D> Textures => m_textureCache.Values;

        public virtual Awaitable<Texture2D> LoadTextureAsync(int index)
        {
#if UNIGLTF_USE_WEBREQUEST_TEXTURELOADER
            return UnityWebRequestTextureLoader.LoadTextureAsync(index);
#else
            return GltfTextureLoader.LoadTextureAsync(m_gltf, m_storage, index);
#endif
        }

        /// <summary>
        /// テクスチャーをロード、必要であれば変換して返す。
        /// 同じものはキャッシュを返す
        /// </summary>
        /// <param name="texture_type">変換の有無を判断する: METALLIC_GLOSS_PROP</param>
        /// <param name="roughnessFactor">METALLIC_GLOSS_PROPの追加パラメーター</param>
        /// <param name="indices">gltf の texture index</param>
        /// <returns></returns>
        public async Awaitable<Texture2D> GetTextureAsync(GetTextureParam param)
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
                            converted.name = $"{converted.name}.{GetTextureParam.NORMAL_PROP}";
                            return converted;
                        }
                        else
                        {
#if UNITY_EDITOR
                            var textureAssetPath = AssetDatabase.GetAssetPath(texture);
                            if (!string.IsNullOrEmpty(textureAssetPath))
                            {
                                TextureIO.MarkTextureAssetAsNormalMap(textureAssetPath);
                                texture.name = $"{texture.name}.{GetTextureParam.NORMAL_PROP}";
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
                        converted.name = $"{converted.name}.{GetTextureParam.METALLIC_GLOSS_PROP}";
                        m_textureCache.Add(param, converted);
                        return converted;
                    }

                case GetTextureParam.OCCLUSION_PROP:
                    {
                        var converted = new OcclusionConverter().GetImportTexture(texture);
                        converted.name = $"{converted.name}.{GetTextureParam.OCCLUSION_PROP}";
                        m_textureCache.Add(param, converted);
                        return converted;
                    }

                default:
                    return texture;
            }

            throw new NotImplementedException();
        }
    }
}
