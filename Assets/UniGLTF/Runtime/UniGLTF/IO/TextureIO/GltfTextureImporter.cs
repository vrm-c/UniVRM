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

        public static TextureImportParam CreateSRGB(GltfParser parser, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var name = CreateNameExt(parser.GLTF, textureIndex, TextureImportTypes.sRGB);
            var sampler = CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex)));
            return new TextureImportParam(name, offset, scale, sampler, TextureImportTypes.sRGB, default, default, getTextureBytesAsync, default, default, default, default, default);
        }

        public static TextureImportParam CreateNormal(GltfParser parser, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var name = CreateNameExt(parser.GLTF, textureIndex, TextureImportTypes.NormalMap);
            var sampler = CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex)));
            return new TextureImportParam(name, offset, scale, sampler, TextureImportTypes.NormalMap, default, default, getTextureBytesAsync, default, default, default, default, default);
        }

        public static TextureImportParam CreateStandard(GltfParser parser, int? metallicRoughnessTextureIndex, int? occlusionTextureIndex, Vector2 offset, Vector2 scale, float metallicFactor, float roughnessFactor)
        {
            TextureImportName name = default;

            GetTextureBytesAsync getMetallicRoughnessAsync = default;
            SamplerParam sampler = default;
            if (metallicRoughnessTextureIndex.HasValue)
            {
                name = CreateNameExt(parser.GLTF, metallicRoughnessTextureIndex.Value, TextureImportTypes.StandardMap);
                sampler = CreateSampler(parser.GLTF, metallicRoughnessTextureIndex.Value);
                getMetallicRoughnessAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, metallicRoughnessTextureIndex.Value)));
            }

            GetTextureBytesAsync getOcclusionAsync = default;
            if (occlusionTextureIndex.HasValue)
            {
                if (string.IsNullOrEmpty(name.GltfName))
                {
                    name = CreateNameExt(parser.GLTF, occlusionTextureIndex.Value, TextureImportTypes.StandardMap);
                }
                sampler = CreateSampler(parser.GLTF, occlusionTextureIndex.Value);
                getOcclusionAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, occlusionTextureIndex.Value)));
            }

            return new TextureImportParam(name, offset, scale, sampler, TextureImportTypes.StandardMap, metallicFactor, roughnessFactor, getMetallicRoughnessAsync, getOcclusionAsync, default, default, default, default);
        }

        public static TextureImportName CreateNameExt(glTF gltf, int textureIndex, TextureImportTypes textureType)
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
            return new TextureImportName(textureType, gltfTexture.name, gltfImage.GetExt(), gltfImage.uri);
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