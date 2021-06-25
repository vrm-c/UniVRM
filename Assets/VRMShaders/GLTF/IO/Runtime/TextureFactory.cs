using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace VRMShaders
{
    public class TextureFactory : IResponsibilityForDestroyObjects
    {
        private readonly IReadOnlyDictionary<SubAssetKey, Texture> _externalMap;
        private readonly bool _isLegacySquaredRoughness;
        private readonly Dictionary<SubAssetKey, Texture> _textureCache = new Dictionary<SubAssetKey, Texture>();

        public ITextureDeserializer TextureDeserializer { get; }

        /// <summary>
        /// Importer が動的に生成した Texture
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, Texture> ConvertedTextures => _textureCache;

        /// <summary>
        /// 外部から渡された、すでに存在する Texture (ex. Extracted Editor Asset)
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, Texture> ExternalTextures => _externalMap;

        public TextureFactory(
            ITextureDeserializer textureDeserializer,
            IReadOnlyDictionary<SubAssetKey, Texture> externalTextures,
            bool isLegacySquaredRoughness)
        {
            TextureDeserializer = textureDeserializer;
            _externalMap = externalTextures;
            _isLegacySquaredRoughness = isLegacySquaredRoughness;
        }

        public void Dispose()
        {
            _textureCache.Clear();
        }

        /// <summary>
        /// 所有権(Dispose権)を移譲する
        /// </summary>
        /// <param name="take"></param>
        public void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            foreach (var (k, v) in _textureCache.ToArray())
            {
                take(k, v);
                _textureCache.Remove(k);
            }
        }

        /// <summary>
        /// テクスチャ生成情報を基に、テクスチャ生成を行う。
        /// SubAssetKey が同じ場合はキャッシュを返す。
        /// </summary>
        public async Task<Texture> GetTextureAsync(TextureDescriptor texDesc, IAwaitCaller awaitCaller)
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
                        // no conversion. Unity's normal map is same with glTF's.
                        //
                        // > contrary to Unity’s usual convention of using Y as “up”
                        // https://docs.unity3d.com/2018.4/Documentation/Manual/StandardShaderMaterialParameterNormalMap.html
                        var data0 = await texDesc.Index0();
                        var rawTexture = await TextureDeserializer.LoadTextureAsync(data0, texDesc.Sampler.EnableMipMap, ColorSpace.Linear, awaitCaller);
                        rawTexture.name = subAssetKey.Name;
                        rawTexture.SetSampler(texDesc.Sampler);
                        _textureCache.Add(subAssetKey, rawTexture);
                        return rawTexture;
                    }

                case TextureImportTypes.StandardMap:
                    {
                        Texture2D metallicRoughnessTexture = default;
                        Texture2D occlusionTexture = default;

                        if (texDesc.Index0 != null)
                        {
                            var data0 = await texDesc.Index0();
                            metallicRoughnessTexture = await TextureDeserializer.LoadTextureAsync(data0, texDesc.Sampler.EnableMipMap, ColorSpace.Linear, awaitCaller);
                        }
                        if (texDesc.Index1 != null)
                        {
                            var data1 = await texDesc.Index1();
                            occlusionTexture = await TextureDeserializer.LoadTextureAsync(data1, texDesc.Sampler.EnableMipMap, ColorSpace.Linear, awaitCaller);
                        }

                        var combinedTexture = OcclusionMetallicRoughnessConverter.Import(metallicRoughnessTexture,
                            texDesc.MetallicFactor, texDesc.RoughnessFactor, occlusionTexture, _isLegacySquaredRoughness);
                        combinedTexture.name = subAssetKey.Name;
                        combinedTexture.SetSampler(texDesc.Sampler);
                        _textureCache.Add(subAssetKey, combinedTexture);
                        UnityObjectDestoyer.DestroyRuntimeOrEditor(metallicRoughnessTexture);
                        UnityObjectDestoyer.DestroyRuntimeOrEditor(occlusionTexture);
                        return combinedTexture;
                    }

                case TextureImportTypes.sRGB:
                    {
                        var data0 = await texDesc.Index0();
                        var rawTexture = await TextureDeserializer.LoadTextureAsync(data0, texDesc.Sampler.EnableMipMap, ColorSpace.sRGB, awaitCaller);
                        rawTexture.name = subAssetKey.Name;
                        rawTexture.SetSampler(texDesc.Sampler);
                        _textureCache.Add(subAssetKey, rawTexture);
                        return rawTexture;
                    }
                case TextureImportTypes.Linear:
                    {
                        var data0 = await texDesc.Index0();
                        var rawTexture = await TextureDeserializer.LoadTextureAsync(data0, texDesc.Sampler.EnableMipMap, ColorSpace.Linear, awaitCaller);
                        rawTexture.name = subAssetKey.Name;
                        rawTexture.SetSampler(texDesc.Sampler);
                        _textureCache.Add(subAssetKey, rawTexture);
                        return rawTexture;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
