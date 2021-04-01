using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;


namespace VRMShaders
{
    [Flags]
    public enum TextureLoadFlags
    {
        None = 0,
        Used = 1,
        External = 1 << 1,
    }

    public struct TextureLoadInfo
    {
        public readonly Texture2D Texture;
        public readonly TextureLoadFlags Flags;
        public bool IsUsed => Flags.HasFlag(TextureLoadFlags.Used);
        public bool IsExternal => Flags.HasFlag(TextureLoadFlags.External);

        public bool IsSubAsset => IsUsed && !IsExternal;

        public TextureLoadInfo(Texture2D texture, bool used, bool isExternal)
        {
            Texture = texture;
            var flags = TextureLoadFlags.None;
            if (used)
            {
                flags |= TextureLoadFlags.Used;
            }
            if (isExternal)
            {
                flags |= TextureLoadFlags.External;
            }
            Flags = flags;
        }
    }

    public class TextureFactory : IDisposable
    {
        public readonly Dictionary<string, Texture2D> ExternalMap;

        public TextureFactory(IEnumerable<(string, UnityEngine.Object)> externalMap)
        {
            if (externalMap != null)
            {
                ExternalMap = externalMap
                    .Select(kv => (kv.Item1, kv.Item2 as Texture2D))
                    .Where(kv => kv.Item2 != null)
                    .ToDictionary(kv => kv.Item1, kv => kv.Item2);
            }
        }

        public static Action<UnityEngine.Object> DestroyResource()
        {
            Action<UnityEngine.Object> des = (UnityEngine.Object o) => UnityEngine.Object.Destroy(o);
            Action<UnityEngine.Object> desi = (UnityEngine.Object o) => UnityEngine.Object.DestroyImmediate(o);
            Action<UnityEngine.Object> func = Application.isPlaying
                ? des
                : desi
                ;
            return func;
        }

        public void Dispose()
        {
            Action<UnityEngine.Object> destroy = DestroyResource();
            foreach (var kv in m_textureCache)
            {
                if (!kv.Value.IsExternal)
                {
#if VRM_DEVELOP
                    // Debug.Log($"Destroy {kv.Value.Texture}");
#endif
                    destroy(kv.Value.Texture);
                }
            }
            m_textureCache.Clear();
        }

        /// <summary>
        /// 所有権(Dispose権)を移譲する
        /// </summary>
        /// <param name="take"></param>
        public void TransferOwnership(Func<UnityEngine.Object, bool> take)
        {
            var keys = new List<string>();
            foreach (var x in m_textureCache)
            {
                if (x.Value.IsUsed && !x.Value.IsExternal)
                {
                    // マテリアルから参照されていて
                    // 外部のAssetからロードしていない。
                    if (take(x.Value.Texture))
                    {
                        keys.Add(x.Key);
                    }
                }
            }
            foreach (var x in keys)
            {
                m_textureCache.Remove(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="TextureLoadInfo"></typeparam>
        /// <returns></returns>
        Dictionary<string, TextureLoadInfo> m_textureCache = new Dictionary<string, TextureLoadInfo>();

        public IEnumerable<TextureLoadInfo> Textures => m_textureCache.Values;


        async Task<TextureLoadInfo> GetOrCreateBaseTexture(TextureImportParam param, GetTextureBytesAsync getTextureBytesAsync, RenderTextureReadWrite colorSpace, bool used)
        {
            var name = param.GltfName;
            if (m_textureCache.TryGetValue(name, out TextureLoadInfo cacheInfo))
            {
                return cacheInfo;
            }

            // not found. load new
            var imageBytes = await getTextureBytesAsync();

            //
            // texture from image(png etc) bytes
            //
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, colorSpace == RenderTextureReadWrite.Linear);
            texture.name = name;
            if (imageBytes != null)
            {
                texture.LoadImage(imageBytes);
            }

            SetSampler(texture, param);

            cacheInfo = new TextureLoadInfo(texture, used, false);
            m_textureCache.Add(name, cacheInfo);
            return cacheInfo;
        }

        public static void SetSampler(Texture2D texture, TextureImportParam param)
        {
            if (texture == null)
            {
                return;
            }

            if (param.Sampler.WrapModes != null)
            {
                foreach (var (key, value) in param.Sampler.WrapModes)
                {
                    switch (key)
                    {
                        case SamplerWrapType.All:
                            texture.wrapMode = value;
                            break;

                        case SamplerWrapType.U:
                            texture.wrapModeU = value;
                            break;

                        case SamplerWrapType.V:
                            texture.wrapModeV = value;
                            break;

                        case SamplerWrapType.W:
                            texture.wrapModeW = value;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            texture.filterMode = param.Sampler.FilterMode;
        }

        /// <summary>
        /// テクスチャーをロード、必要であれば変換して返す。
        /// 同じものはキャッシュを返す
        /// </summary>
        /// <param name="texture_type">変換の有無を判断する: METALLIC_GLOSS_PROP</param>
        /// <param name="roughnessFactor">METALLIC_GLOSS_PROPの追加パラメーター</param>
        /// <param name="indices">gltf の texture index</param>
        /// <returns></returns>
        public async Task<Texture2D> GetTextureAsync(TextureImportParam param)
        {
            //
            // ExtractKey で External とのマッチングを試みる
            // 
            // Normal => GltfName
            // Standard => ConvertedName
            // sRGB => GltfName 
            // Linear => GltfName 
            //
            if (param.Index0 != null && ExternalMap != null)
            {
                if (ExternalMap.TryGetValue(param.ExtractKey, out Texture2D external))
                {
                    return external;
                }
            }

            switch (param.TextureType)
            {
                case TextureImportTypes.NormalMap:
                    // Runtime/SubAsset 用に変換する
                    {
                        if (!m_textureCache.TryGetValue(param.ConvertedName, out TextureLoadInfo info))
                        {
                            var baseTexture = await GetOrCreateBaseTexture(param, param.Index0, RenderTextureReadWrite.Linear, false);
                            var converted = NormalConverter.Import(baseTexture.Texture);
                            converted.name = param.ConvertedName;
                            info = new TextureLoadInfo(converted, true, false);
                            m_textureCache.Add(converted.name, info);
                        }
                        return info.Texture;
                    }

                case TextureImportTypes.StandardMap:
                    // 変換する
                    {
                        if (!m_textureCache.TryGetValue(param.ConvertedName, out TextureLoadInfo info))
                        {
                            TextureLoadInfo baseTexture = default;
                            if (param.Index0 != null)
                            {
                                baseTexture = await GetOrCreateBaseTexture(param, param.Index0, RenderTextureReadWrite.Linear, false);
                            }
                            TextureLoadInfo occlusionBaseTexture = default;
                            if (param.Index1 != null)
                            {
                                occlusionBaseTexture = await GetOrCreateBaseTexture(param, param.Index1, RenderTextureReadWrite.Linear, false);
                            }
                            var converted = OcclusionMetallicRoughnessConverter.Import(baseTexture.Texture, param.MetallicFactor, param.RoughnessFactor, occlusionBaseTexture.Texture);
                            converted.name = param.ConvertedName;
                            info = new TextureLoadInfo(converted, true, false);
                            m_textureCache.Add(converted.name, info);
                        }
                        return info.Texture;
                    }

                default:
                    {
                        var baseTexture = await GetOrCreateBaseTexture(param, param.Index0, RenderTextureReadWrite.sRGB, true);
                        return baseTexture.Texture;
                    }
            }

            throw new NotImplementedException();
        }


    }
}
