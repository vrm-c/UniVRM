using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public delegate Task<ArraySegment<byte>> GetTextureBytesAsync();

    /// <summary>
    /// STANDARD(Pbr) texture = occlusion + metallic + smoothness
    /// </summary>
    public struct GetTextureParam
    {
        public const string NORMAL_PROP = "_BumpMap";
        public const string NORMAL_SUFFIX = ".normal";

        public const string METALLIC_GLOSS_PROP = "_MetallicGlossMap";
        public const string OCCLUSION_PROP = "_OcclusionMap";
        public const string STANDARD_SUFFIX = ".standard";

        public enum TextureTypes
        {
            sRGB,
            NormalMap,
            // Occlusion + Metallic + Smoothness
            StandardMap,
            Linear,
        }

        public static string RemoveSuffix(string src)
        {
            if (src.EndsWith(NORMAL_SUFFIX))
            {
                return src.Substring(0, src.Length - NORMAL_SUFFIX.Length);
            }
            else if (src.EndsWith(STANDARD_SUFFIX))
            {
                return src.Substring(0, src.Length - STANDARD_SUFFIX.Length);
            }
            else
            {
                return src;
            }
        }
        
        public struct NameExt
        {
            public readonly string GltfName;
            public readonly string ConvertedName;

            public readonly string Ext;
            public string Uri;
            
            public NameExt(string gltfName, string convertedName, string ext, string uri)
            {
                GltfName = gltfName;
                ConvertedName = convertedName;
                Ext = ext;
                Uri = uri;
            }
            
            public string GltfFileName => $"{GltfName}{Ext}";
            
            public string ConvertedFileName => $"{ConvertedName}.png";
            
            public static NameExt Create(glTF gltf, int textureIndex, TextureTypes textureType)
            {
                if (textureIndex < 0 || textureIndex >= gltf.textures.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                var gltfTexture = gltf.textures[textureIndex];
                if(gltfTexture.source < 0 || gltfTexture.source >=gltf.images.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                var gltfImage = gltf.images[gltfTexture.source];
                return new NameExt(gltfTexture.name, Convert(gltfTexture.name, textureType), gltfImage.GetExt(), gltfImage.uri);
            }

            static string Convert(string name, TextureTypes textureType) 
            {
                switch (textureType)
                {
                    case TextureTypes.StandardMap: return $"{name}{STANDARD_SUFFIX}";
                    case TextureTypes.NormalMap: return $"{name}{NORMAL_SUFFIX}";
                    default: return name;
                }
            }
        }
        
        public readonly NameExt Name;
        public string GltfName => Name.GltfName;
        public string GltfFileName => Name.GltfFileName; 
        public string ConvertedName => Name.ConvertedName;
        public string ConvertedFileName => Name.ConvertedFileName;
        public string Uri => Name.Uri;

        public struct TextureSamplerParam
        {
            public enum TextureWrapType
            {
                All,
                U,
                V,
                W,
            }

            public (TextureWrapType, TextureWrapMode)[] WrapModes;
            public FilterMode FilterMode;

            public static TextureSamplerParam Create(glTF gltf, int index)
            {
                var gltfTexture = gltf.textures[index];
                if(gltfTexture.sampler<0 || gltfTexture.sampler>= gltf.samplers.Count)
                {
                    // default
                    return new TextureSamplerParam
                    {
                        FilterMode = FilterMode.Bilinear,
                        WrapModes = new (TextureWrapType, TextureWrapMode)[]{},
                    };
                }

                var gltfSampler = gltf.samplers[gltfTexture.sampler];
                return new TextureSamplerParam
                {
                    WrapModes = GetUnityWrapMode(gltfSampler).ToArray(),
                    FilterMode = ImportFilterMode(gltfSampler.minFilter),
                };
            }

            public static IEnumerable<(TextureWrapType, TextureWrapMode)> GetUnityWrapMode(glTFTextureSampler sampler)
            {
                if (sampler.wrapS == sampler.wrapT)
                {
                    switch (sampler.wrapS)
                    {
                        case glWrap.NONE: // default
                            yield return (TextureWrapType.All, TextureWrapMode.Repeat);
                            break;

                        case glWrap.CLAMP_TO_EDGE:
                            yield return (TextureWrapType.All, TextureWrapMode.Clamp);
                            break;

                        case glWrap.REPEAT:
                            yield return (TextureWrapType.All, TextureWrapMode.Repeat);
                            break;

                        case glWrap.MIRRORED_REPEAT:
                            yield return (TextureWrapType.All, TextureWrapMode.Mirror);
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
                            yield return (TextureWrapType.U, TextureWrapMode.Repeat);
                            break;

                        case glWrap.CLAMP_TO_EDGE:
                            yield return (TextureWrapType.U, TextureWrapMode.Clamp);
                            break;

                        case glWrap.REPEAT:
                            yield return (TextureWrapType.U, TextureWrapMode.Repeat);
                            break;

                        case glWrap.MIRRORED_REPEAT:
                            yield return (TextureWrapType.U, TextureWrapMode.Mirror);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    switch (sampler.wrapT)
                    {
                        case glWrap.NONE: // default
                            yield return (TextureWrapType.V, TextureWrapMode.Repeat);
                            break;

                        case glWrap.CLAMP_TO_EDGE:
                            yield return (TextureWrapType.V, TextureWrapMode.Clamp);
                            break;

                        case glWrap.REPEAT:
                            yield return (TextureWrapType.V, TextureWrapMode.Repeat);
                            break;

                        case glWrap.MIRRORED_REPEAT:
                            yield return (TextureWrapType.V, TextureWrapMode.Mirror);
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

        public TextureSamplerParam Sampler;

        public readonly TextureTypes TextureType;
        public readonly float MetallicFactor;
        public readonly float RoughnessFactor;
        
        public readonly GetTextureBytesAsync Index0;
        public readonly GetTextureBytesAsync Index1;
        public readonly GetTextureBytesAsync Index2;
        public readonly GetTextureBytesAsync Index3;
        public readonly GetTextureBytesAsync Index4;
        public readonly GetTextureBytesAsync Index5;

        /// <summary>
        /// この種類は RGB チャンネルの組み換えが必用
        /// </summary>
        public bool ExtractConverted => TextureType == TextureTypes.StandardMap;

        public GetTextureParam(NameExt name, TextureSamplerParam sampler, TextureTypes textureType, float metallicFactor, float roughnessFactor,
            GetTextureBytesAsync i0,
            GetTextureBytesAsync i1,
            GetTextureBytesAsync i2,
            GetTextureBytesAsync i3,
            GetTextureBytesAsync i4,
            GetTextureBytesAsync i5)
        {
            Name = name;
            Sampler = sampler;
            TextureType = textureType;
            MetallicFactor = metallicFactor;
            RoughnessFactor = roughnessFactor;
            Index0 = i0;
            Index1 = i1;
            Index2 = i2;
            Index3 = i3;
            Index4 = i4;
            Index5 = i5;
        }

        public static GetTextureParam CreateSRGB(GltfParser parser, int textureIndex)
        {
            var name = NameExt.Create(parser.GLTF, textureIndex, TextureTypes.sRGB);
            var sampler = TextureSamplerParam.Create(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex);
            return new GetTextureParam(name, sampler, TextureTypes.sRGB, default, default, getTextureBytesAsync, default, default, default, default, default);
        }

        public static GetTextureParam CreateNormal(GltfParser parser, int textureIndex)
        {
            var name = NameExt.Create(parser.GLTF, textureIndex, TextureTypes.NormalMap);
            var sampler = TextureSamplerParam.Create(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex);
            return new GetTextureParam(name, sampler, TextureTypes.NormalMap, default, default, getTextureBytesAsync, default, default, default, default, default);
        }

        public static GetTextureParam CreateStandard(GltfParser parser, int? metallicRoughnessTextureIndex, int? occlusionTextureIndex, float metallicFactor, float roughnessFactor)
        {
            NameExt name = default;

            GetTextureBytesAsync getMetallicRoughnessAsync = default;
            TextureSamplerParam sampler = default;
            if (metallicRoughnessTextureIndex.HasValue)
            {
                name = NameExt.Create(parser.GLTF, metallicRoughnessTextureIndex.Value, TextureTypes.StandardMap);
                sampler = TextureSamplerParam.Create(parser.GLTF, metallicRoughnessTextureIndex.Value);
                getMetallicRoughnessAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, metallicRoughnessTextureIndex.Value);
            }

            GetTextureBytesAsync getOcclusionAsync = default;
            if (occlusionTextureIndex.HasValue)
            {
                if(string.IsNullOrEmpty(name.GltfName)){
                    name = NameExt.Create(parser.GLTF, occlusionTextureIndex.Value, TextureTypes.StandardMap);
                }
                sampler = TextureSamplerParam.Create(parser.GLTF, occlusionTextureIndex.Value);
                getOcclusionAsync = async () => parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, occlusionTextureIndex.Value);
            }

            return new GetTextureParam(name, sampler, TextureTypes.StandardMap, metallicFactor, roughnessFactor, getMetallicRoughnessAsync, getOcclusionAsync, default, default, default, default);
        }

        public static GetTextureParam Create(GltfParser parser, int index, string prop, float metallicFactor, float roughnessFactor)
        {
            switch (prop)
            {
                case NORMAL_PROP:
                    return CreateNormal(parser, index);

                case OCCLUSION_PROP:
                case METALLIC_GLOSS_PROP:
                    return CreateStandard(parser, index, default, metallicFactor, roughnessFactor);

                default:
                    return CreateSRGB(parser, index);
            }
        }
    }
}
