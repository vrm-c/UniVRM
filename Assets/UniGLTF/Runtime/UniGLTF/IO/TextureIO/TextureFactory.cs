using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using VRMShaders;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniGLTF
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

    public delegate Task<Texture2D> GetTextureAsyncFunc(IAwaitCaller awaitCaller, glTF gltf, GetTextureParam param);
    public class TextureFactory : IDisposable
    {
        glTF m_gltf;
        IStorage m_storage;

        public readonly Dictionary<string, Texture2D> ExternalMap;

        public bool TryGetExternal(GetTextureParam param, bool used, out Texture2D external)
        {
            if (param.Index0!=null && ExternalMap != null)
            {
                var cacheName = param.ConvertedName;
                if (param.TextureType == TextureImportTypes.NormalMap)
                {
                    cacheName = param.GltfName;
                    if (m_textureCache.TryGetValue(cacheName, out TextureLoadInfo normalInfo))
                    {
                        external = normalInfo.Texture;
                        return true;
                    }
                }
                if (ExternalMap.TryGetValue(cacheName, out external))
                {
                    m_textureCache.Add(cacheName, new TextureLoadInfo(external, used, true));
                    return true;
                }
            }
            external = default;
            return false;
        }

        public TextureFactory(glTF gltf, IStorage storage, IEnumerable<(string, UnityEngine.Object)> externalMap)
        {
            m_gltf = gltf;
            m_storage = storage;

            if (externalMap != null)
            {
                ExternalMap = externalMap
                    .Select(kv => (kv.Item1, kv.Item2 as Texture2D))
                    .Where(kv => kv.Item2 != null)
                    .ToDictionary(kv => kv.Item1, kv => kv.Item2);
            }
        }

        public void Dispose()
        {
            Action<UnityEngine.Object> destroy = UnityResourceDestroyer.DestroyResource();
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
        public void TransferOwnership(TakeOwnershipFunc take)
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

        Dictionary<string, TextureLoadInfo> m_textureCache = new Dictionary<string, TextureLoadInfo>();

        public IEnumerable<TextureLoadInfo> Textures => m_textureCache.Values;

        static Byte[] ToArray(ArraySegment<byte> bytes)
        {
            if (bytes.Array == null)
            {
                return new byte[] { };
            }
            else if (bytes.Offset == 0 && bytes.Count == bytes.Array.Length)
            {
                return bytes.Array;
            }
            else
            {
                Byte[] result = new byte[bytes.Count];
                Buffer.BlockCopy(bytes.Array, bytes.Offset, result, 0, result.Length);
                return result;
            }
        }

        async Task<TextureLoadInfo> GetOrCreateBaseTexture(IAwaitCaller awaitCaller, GetTextureParam param, GetTextureBytesAsync getTextureBytesAsync, RenderTextureReadWrite colorSpace, bool used)
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
                texture.LoadImage(ToArray(imageBytes));
            }

            SetSampler(texture, param);

            cacheInfo = new TextureLoadInfo(texture, used, false);
            m_textureCache.Add(name, cacheInfo);
            return cacheInfo;
        }

        public static void SetSampler(Texture2D texture, GetTextureParam param)
        {
            if (texture == null)
            {
                return;
            }

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
        public async Task<Texture2D> GetTextureAsync(IAwaitCaller awaitCaller, glTF gltf, GetTextureParam param)
        {
            if (m_textureCache.TryGetValue(param.ConvertedName, out TextureLoadInfo cacheInfo))
            {
                return cacheInfo.Texture;
            }
            if (TryGetExternal(param, true, out Texture2D external))
            {
                return external;
            }

            switch (param.TextureType)
            {
                case TextureImportTypes.NormalMap:
                    {
                        var baseTexture = await GetOrCreateBaseTexture(awaitCaller, param, param.Index0, RenderTextureReadWrite.Linear, false);
                        var converted = NormalConverter.Import(baseTexture.Texture);
                        converted.name = param.ConvertedName;
                        var info = new TextureLoadInfo(converted, true, false);
                        m_textureCache.Add(converted.name, info);
                        return info.Texture;
                    }

                case TextureImportTypes.StandardMap:
                    {
                        TextureLoadInfo baseTexture = default;
                        if (param.Index0!=null)
                        {
                            baseTexture = await GetOrCreateBaseTexture(awaitCaller, param, param.Index0, RenderTextureReadWrite.Linear, false);
                        }
                        TextureLoadInfo occlusionBaseTexture = default;
                        if (param.Index1!=null)
                        {
                            occlusionBaseTexture = await GetOrCreateBaseTexture(awaitCaller, param, param.Index1, RenderTextureReadWrite.Linear, false);
                        }
                        var converted = OcclusionMetallicRoughnessConverter.Import(baseTexture.Texture, param.MetallicFactor, param.RoughnessFactor, occlusionBaseTexture.Texture);
                        converted.name = param.ConvertedName;
                        var info = new TextureLoadInfo(converted, true, false);
                        m_textureCache.Add(converted.name, info);
                        return info.Texture;
                    }

                default:
                    {
                        var baseTexture = await GetOrCreateBaseTexture(awaitCaller, param, param.Index0, RenderTextureReadWrite.sRGB, true);
                        return baseTexture.Texture;
                    }

                    throw new NotImplementedException();
            }
        }

        public static GetTextureParam CreateSRGB(GltfParser parser, int textureIndex)
        {
            var name = CreateNameExt(parser.GLTF, textureIndex, TextureImportTypes.sRGB);
            var sampler = CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex);
            return new GetTextureParam(name, sampler, TextureImportTypes.sRGB, default, default, getTextureBytesAsync, default, default, default, default, default);
        }

        public static GetTextureParam CreateNormal(GltfParser parser, int textureIndex)
        {
            var name = CreateNameExt(parser.GLTF, textureIndex, TextureImportTypes.NormalMap);
            var sampler = CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex);
            return new GetTextureParam(name, sampler, TextureImportTypes.NormalMap, default, default, getTextureBytesAsync, default, default, default, default, default);
        }

        public static GetTextureParam CreateStandard(GltfParser parser, int? metallicRoughnessTextureIndex, int? occlusionTextureIndex, float metallicFactor, float roughnessFactor)
        {
            GetTextureParam.NameExt name = default;

            GetTextureBytesAsync getMetallicRoughnessAsync = default;
            SamplerParam sampler = default;
            if (metallicRoughnessTextureIndex.HasValue)
            {
                name = CreateNameExt(parser.GLTF, metallicRoughnessTextureIndex.Value, TextureImportTypes.StandardMap);
                sampler = CreateSampler(parser.GLTF, metallicRoughnessTextureIndex.Value);
                getMetallicRoughnessAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, metallicRoughnessTextureIndex.Value);
            }

            GetTextureBytesAsync getOcclusionAsync = default;
            if (occlusionTextureIndex.HasValue)
            {
                if(string.IsNullOrEmpty(name.GltfName)){
                    name = CreateNameExt(parser.GLTF, occlusionTextureIndex.Value, TextureImportTypes.StandardMap);
                }
                sampler = CreateSampler(parser.GLTF, occlusionTextureIndex.Value);
                getOcclusionAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, occlusionTextureIndex.Value);
            }

            return new GetTextureParam(name, sampler, TextureImportTypes.StandardMap, metallicFactor, roughnessFactor, getMetallicRoughnessAsync, getOcclusionAsync, default, default, default, default);
        }

        public static GetTextureParam Create(GltfParser parser, int index, string prop, float metallicFactor, float roughnessFactor)
        {
            switch (prop)
            {
                case GetTextureParam.NORMAL_PROP:
                    return CreateNormal(parser, index);

                case GetTextureParam.OCCLUSION_PROP:
                case GetTextureParam.METALLIC_GLOSS_PROP:
                    return CreateStandard(parser, index, default, metallicFactor, roughnessFactor);

                default:
                    return CreateSRGB(parser, index);
            }
        }

        public static GetTextureParam.NameExt CreateNameExt(glTF gltf, int textureIndex, TextureImportTypes textureType)
        {
            if (textureIndex < 0 || textureIndex >= gltf.textures.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            var gltfTexture = gltf.textures[textureIndex];
            if (gltfTexture.source < 0 || gltfTexture.source >= gltf.images.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            var gltfImage = gltf.images[gltfTexture.source];
            var convertedName = GetTextureParam.NameExt.Convert(gltfTexture.name, textureType);
            return new GetTextureParam.NameExt(gltfTexture.name, convertedName, gltfImage.GetExt(), gltfImage.uri);
        }

        public static SamplerParam CreateSampler(glTF gltf, int index)
        {
            var gltfTexture = gltf.textures[index];
            if (gltfTexture.sampler < 0 || gltfTexture.sampler >= gltf.samplers.Count)
            {
                // default
                return new SamplerParam
                {
                    FilterMode = FilterMode.Bilinear,
                    WrapModes = new (SamplerWrapType, TextureWrapMode)[] { },
                };
            }

            var gltfSampler = gltf.samplers[gltfTexture.sampler];
            return new SamplerParam
            {
                WrapModes = GetUnityWrapMode(gltfSampler).ToArray(),
                FilterMode = ImportFilterMode(gltfSampler.minFilter),
            };
        }

        public static IEnumerable<(SamplerWrapType, TextureWrapMode)> GetUnityWrapMode(glTFTextureSampler sampler)
        {
            if (sampler.wrapS == sampler.wrapT)
            {
                switch (sampler.wrapS)
                {
                    case glWrap.NONE: // default
                        yield return (SamplerWrapType.All, TextureWrapMode.Repeat);
                        break;

                    case glWrap.CLAMP_TO_EDGE:
                        yield return (SamplerWrapType.All, TextureWrapMode.Clamp);
                        break;

                    case glWrap.REPEAT:
                        yield return (SamplerWrapType.All, TextureWrapMode.Repeat);
                        break;

                    case glWrap.MIRRORED_REPEAT:
                        yield return (SamplerWrapType.All, TextureWrapMode.Mirror);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (sampler.wrapS)
                {
                    case glWrap.NONE: // default
                        yield return (SamplerWrapType.U, TextureWrapMode.Repeat);
                        break;

                    case glWrap.CLAMP_TO_EDGE:
                        yield return (SamplerWrapType.U, TextureWrapMode.Clamp);
                        break;

                    case glWrap.REPEAT:
                        yield return (SamplerWrapType.U, TextureWrapMode.Repeat);
                        break;

                    case glWrap.MIRRORED_REPEAT:
                        yield return (SamplerWrapType.U, TextureWrapMode.Mirror);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                switch (sampler.wrapT)
                {
                    case glWrap.NONE: // default
                        yield return (SamplerWrapType.V, TextureWrapMode.Repeat);
                        break;

                    case glWrap.CLAMP_TO_EDGE:
                        yield return (SamplerWrapType.V, TextureWrapMode.Clamp);
                        break;

                    case glWrap.REPEAT:
                        yield return (SamplerWrapType.V, TextureWrapMode.Repeat);
                        break;

                    case glWrap.MIRRORED_REPEAT:
                        yield return (SamplerWrapType.V, TextureWrapMode.Mirror);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public static FilterMode ImportFilterMode(glFilter filterMode)
        {
            switch (filterMode)
            {
                case glFilter.NEAREST:
                case glFilter.NEAREST_MIPMAP_LINEAR:
                case glFilter.NEAREST_MIPMAP_NEAREST:
                    return FilterMode.Point;

                case glFilter.NONE:
                case glFilter.LINEAR:
                case glFilter.LINEAR_MIPMAP_NEAREST:
                    return FilterMode.Bilinear;

                case glFilter.LINEAR_MIPMAP_LINEAR:
                    return FilterMode.Trilinear;

                default:
                    throw new NotImplementedException();
            }
        }

    }
}
