using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    /// <summary>
    /// glTFTexture を TextureImportParam に変換する
    /// </summary>
    public static class GltfTextureImporter
    {
        public static Byte[] ToArray(ArraySegment<byte> bytes)
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

        public static (SubAssetKey, TextureImportParam Param) CreateSRGB(GltfParser parser, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = parser.GLTF.textures[textureIndex];
            var gltfImage = parser.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.sRGB, gltfTexture.name, gltfImage.uri);
            var sampler = CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex)));
            var key = new SubAssetKey(typeof(Texture2D), name);
            var param = new TextureImportParam(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.sRGB, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (key, param);
        }

        public static (SubAssetKey, TextureImportParam Param) CreateNormal(GltfParser parser, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = parser.GLTF.textures[textureIndex];
            var gltfImage = parser.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.NormalMap, gltfTexture.name, gltfImage.uri);
            var sampler = CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex)));
            var key = new SubAssetKey(typeof(Texture2D), name);
            var param = new TextureImportParam(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.NormalMap, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (key, param);
        }

        public static TextureImportParam CreateStandard(GltfParser parser, int? metallicRoughnessTextureIndex, int? occlusionTextureIndex, Vector2 offset, Vector2 scale, float metallicFactor, float roughnessFactor)
        {
            string name = default;

            GetTextureBytesAsync getMetallicRoughnessAsync = default;
            SamplerParam sampler = default;
            if (metallicRoughnessTextureIndex.HasValue)
            {
                var gltfTexture = parser.GLTF.textures[metallicRoughnessTextureIndex.Value];
                name = TextureImportName.GetUnityObjectName(TextureImportTypes.StandardMap, gltfTexture.name, parser.GLTF.images[gltfTexture.source].uri);
                sampler = CreateSampler(parser.GLTF, metallicRoughnessTextureIndex.Value);
                getMetallicRoughnessAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, metallicRoughnessTextureIndex.Value)));
            }

            GetTextureBytesAsync getOcclusionAsync = default;
            if (occlusionTextureIndex.HasValue)
            {
                var gltfTexture = parser.GLTF.textures[occlusionTextureIndex.Value];
                if (string.IsNullOrEmpty(name))
                {
                    name = TextureImportName.GetUnityObjectName(TextureImportTypes.StandardMap, gltfTexture.name, parser.GLTF.images[gltfTexture.source].uri);
                }
                sampler = CreateSampler(parser.GLTF, occlusionTextureIndex.Value);
                getOcclusionAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, occlusionTextureIndex.Value)));
            }

            return new TextureImportParam(name, ".png", null, offset, scale, sampler, TextureImportTypes.StandardMap, metallicFactor, roughnessFactor, getMetallicRoughnessAsync, getOcclusionAsync, default, default, default, default);
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