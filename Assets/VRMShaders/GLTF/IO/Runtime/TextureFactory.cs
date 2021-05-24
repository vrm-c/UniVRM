using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using ColorSpace = UniGLTF.ColorSpace;


namespace VRMShaders
{
    public class TextureFactory : IDisposable
    {
        private readonly Dictionary<SubAssetKey, Texture> m_temporaryTextures = new Dictionary<SubAssetKey, Texture>();
        private readonly Dictionary<SubAssetKey, Texture> m_textureCache = new Dictionary<SubAssetKey, Texture>();
        private readonly IReadOnlyDictionary<SubAssetKey, Texture> m_externalMap;

        /// <summary>
        /// Importer が動的に生成した Texture
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, Texture> ConvertedTextures => m_textureCache;

        /// <summary>
        ///
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, Texture> ExternalTextures => m_externalMap;

        public TextureFactory(IReadOnlyDictionary<SubAssetKey, Texture> externalTextures)
        {
            m_externalMap = externalTextures;
        }

        public void Dispose()
        {
            foreach (var kv in m_temporaryTextures)
            {
                DestroyResource(kv.Value);
            }
            m_temporaryTextures.Clear();
            m_textureCache.Clear();
        }

        /// <summary>
        /// 所有権(Dispose権)を移譲する
        /// </summary>
        /// <param name="take"></param>
        public void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            var transferredAssets = new HashSet<SubAssetKey>();
            foreach (var x in m_textureCache)
            {
                if (take(x.Value))
                {
                    transferredAssets.Add(x.Key);
                }
            }

            foreach (var key in transferredAssets)
            {
                m_textureCache.Remove(key);
            }
        }

        async Task<Texture2D> LoadTextureAsync(GetTextureBytesAsync getTextureBytesAsync, bool useMipmap, ColorSpace colorSpace)
        {
            var imageBytes = await getTextureBytesAsync();

            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, useMipmap, colorSpace == ColorSpace.Linear);
            if (imageBytes != null)
            {
                texture.LoadImage(imageBytes);
            }

            return texture;
        }

        /// <summary>
        /// テクスチャーをロード、必要であれば変換して返す。
        /// 同じものはキャッシュを返す
        /// </summary>
        /// <param name="texture_type">変換の有無を判断する: METALLIC_GLOSS_PROP</param>
        /// <param name="roughnessFactor">METALLIC_GLOSS_PROPの追加パラメーター</param>
        /// <param name="indices">gltf の texture index</param>
        /// <returns></returns>
        public async Task<Texture> GetTextureAsync(TextureImportParam param)
        {
            var subAssetKey = param.SubAssetKey;

            if (m_externalMap != null && m_externalMap.TryGetValue(subAssetKey, out var externalTexture))
            {
                return externalTexture;
            }

            if (m_textureCache.TryGetValue(subAssetKey, out var cachedTexture))
            {
                return cachedTexture;
            }

            switch (param.TextureType)
            {
                case TextureImportTypes.NormalMap:
                {
                    // Runtime/SubAsset 用に変換する
                    var rawTexture = await LoadTextureAsync(param.Index0, param.Sampler.EnableMipMap, ColorSpace.Linear);
                    var convertedTexture = NormalConverter.Import(rawTexture);
                    convertedTexture.name = subAssetKey.Name;
                    convertedTexture.SetSampler(param.Sampler);
                    m_textureCache.Add(subAssetKey, convertedTexture);
                    DestroyResource(rawTexture);
                    return convertedTexture;
                }

                case TextureImportTypes.StandardMap:
                {
                    Texture2D metallicRoughnessTexture = default;
                    Texture2D occlusionTexture = default;

                    if (param.Index0 != null)
                    {
                        metallicRoughnessTexture = await LoadTextureAsync(param.Index0, param.Sampler.EnableMipMap, ColorSpace.Linear);
                    }
                    if (param.Index1 != null)
                    {
                        occlusionTexture = await LoadTextureAsync(param.Index1, param.Sampler.EnableMipMap, ColorSpace.Linear);
                    }

                    var combinedTexture = OcclusionMetallicRoughnessConverter.Import(metallicRoughnessTexture,
                        param.MetallicFactor, param.RoughnessFactor, occlusionTexture);
                    combinedTexture.name = subAssetKey.Name;
                    combinedTexture.SetSampler(param.Sampler);
                    m_textureCache.Add(subAssetKey, combinedTexture);
                    DestroyResource(metallicRoughnessTexture);
                    DestroyResource(occlusionTexture);
                    return combinedTexture;
                }

                case TextureImportTypes.sRGB:
                {
                    var rawTexture = await LoadTextureAsync(param.Index0, param.Sampler.EnableMipMap, ColorSpace.sRGB);
                    rawTexture.name = subAssetKey.Name;
                    rawTexture.SetSampler(param.Sampler);
                    m_textureCache.Add(subAssetKey, rawTexture);
                    Debug.Log($"{rawTexture.name}:{param.Sampler.EnableMipMap}");
                    return rawTexture;
                }
                case TextureImportTypes.Linear:
                {
                    var rawTexture = await LoadTextureAsync(param.Index0, param.Sampler.EnableMipMap, ColorSpace.Linear);
                    rawTexture.name = subAssetKey.Name;
                    rawTexture.SetSampler(param.Sampler);
                    m_textureCache.Add(subAssetKey, rawTexture);
                    return rawTexture;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        private static void DestroyResource(UnityEngine.Object o)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(o);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(o);
            }
        }
    }
}
