using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace VRMShaders
{
    public class TextureFactory : IDisposable
    {
        private readonly ITextureDeserializer _textureDeserializer;
        private readonly IReadOnlyDictionary<SubAssetKey, Texture> _externalMap;
        private readonly Dictionary<SubAssetKey, Texture> _temporaryTextures = new Dictionary<SubAssetKey, Texture>();
        private readonly Dictionary<SubAssetKey, Texture> _textureCache = new Dictionary<SubAssetKey, Texture>();

        /// <summary>
        /// Importer が動的に生成した Texture
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, Texture> ConvertedTextures => _textureCache;

        /// <summary>
        /// 外部から渡された、すでに存在する Texture (ex. Extracted Editor Asset)
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, Texture> ExternalTextures => _externalMap;

        public TextureFactory(ITextureDeserializer textureDeserializer, IReadOnlyDictionary<SubAssetKey, Texture> externalTextures)
        {
            _textureDeserializer = textureDeserializer;
            _externalMap = externalTextures;
        }

        public void Dispose()
        {
            foreach (var kv in _temporaryTextures)
            {
                DestroyResource(kv.Value);
            }
            _temporaryTextures.Clear();
            _textureCache.Clear();
        }

        /// <summary>
        /// 所有権(Dispose権)を移譲する
        /// </summary>
        /// <param name="take"></param>
        public void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            var transferredAssets = new HashSet<SubAssetKey>();
            foreach (var x in _textureCache)
            {
                if (take(x.Value))
                {
                    transferredAssets.Add(x.Key);
                }
            }

            foreach (var key in transferredAssets)
            {
                _textureCache.Remove(key);
            }
        }

        /// <summary>
        /// テクスチャーをロード、必要であれば変換して返す。
        /// 同じものはキャッシュを返す
        /// </summary>
        /// <param name="texture_type">変換の有無を判断する: METALLIC_GLOSS_PROP</param>
        /// <param name="roughnessFactor">METALLIC_GLOSS_PROPの追加パラメーター</param>
        /// <param name="indices">gltf の texture index</param>
        /// <returns></returns>
        public async Task<Texture> GetTextureAsync(TextureDescriptor texDesc)
        {
            var subAssetKey = texDesc.SubAssetKey;

            if (_externalMap != null && _externalMap.TryGetValue(subAssetKey, out var externalTexture))
            {
                return externalTexture;
            }

            if (_textureCache.TryGetValue(subAssetKey, out var cachedTexture))
            {
                return cachedTexture;
            }

            switch (texDesc.TextureType)
            {
                case TextureImportTypes.NormalMap:
                {
                    // Runtime/SubAsset 用に変換する
                    var rawTexture = await _textureDeserializer.LoadTextureAsync(texDesc.Index0, texDesc.Sampler.EnableMipMap, ColorSpace.Linear);
                    var convertedTexture = NormalConverter.Import(rawTexture);
                    convertedTexture.name = subAssetKey.Name;
                    convertedTexture.SetSampler(texDesc.Sampler);
                    _textureCache.Add(subAssetKey, convertedTexture);
                    DestroyResource(rawTexture);
                    return convertedTexture;
                }

                case TextureImportTypes.StandardMap:
                {
                    Texture2D metallicRoughnessTexture = default;
                    Texture2D occlusionTexture = default;

                    if (texDesc.Index0 != null)
                    {
                        metallicRoughnessTexture = await _textureDeserializer.LoadTextureAsync(texDesc.Index0, texDesc.Sampler.EnableMipMap, ColorSpace.Linear);
                    }
                    if (texDesc.Index1 != null)
                    {
                        occlusionTexture = await _textureDeserializer.LoadTextureAsync(texDesc.Index1, texDesc.Sampler.EnableMipMap, ColorSpace.Linear);
                    }

                    var combinedTexture = OcclusionMetallicRoughnessConverter.Import(metallicRoughnessTexture,
                        texDesc.MetallicFactor, texDesc.RoughnessFactor, occlusionTexture);
                    combinedTexture.name = subAssetKey.Name;
                    combinedTexture.SetSampler(texDesc.Sampler);
                    _textureCache.Add(subAssetKey, combinedTexture);
                    DestroyResource(metallicRoughnessTexture);
                    DestroyResource(occlusionTexture);
                    return combinedTexture;
                }

                case TextureImportTypes.sRGB:
                {
                    var rawTexture = await _textureDeserializer.LoadTextureAsync(texDesc.Index0, texDesc.Sampler.EnableMipMap, ColorSpace.sRGB);
                    rawTexture.name = subAssetKey.Name;
                    rawTexture.SetSampler(texDesc.Sampler);
                    _textureCache.Add(subAssetKey, rawTexture);
                    return rawTexture;
                }
                case TextureImportTypes.Linear:
                {
                    var rawTexture = await _textureDeserializer.LoadTextureAsync(texDesc.Index0, texDesc.Sampler.EnableMipMap, ColorSpace.Linear);
                    rawTexture.name = subAssetKey.Name;
                    rawTexture.SetSampler(texDesc.Sampler);
                    _textureCache.Add(subAssetKey, rawTexture);
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
