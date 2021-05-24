using System;
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
            var sampler = TextureSamplerUtil.CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex)));
            var key = new SubAssetKey(typeof(Texture2D), name);
            var param = new TextureImportParam(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.sRGB, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (key, param);
        }

        public static (SubAssetKey, TextureImportParam Param) CreateLinear(GltfParser parser, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = parser.GLTF.textures[textureIndex];
            var gltfImage = parser.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.Linear, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(parser.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, textureIndex)));
            var key = new SubAssetKey(typeof(Texture2D), name);
            var param = new TextureImportParam(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.Linear, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (key, param);
        }

        public static (SubAssetKey, TextureImportParam Param) CreateNormal(GltfParser parser, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = parser.GLTF.textures[textureIndex];
            var gltfImage = parser.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.NormalMap, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(parser.GLTF, textureIndex);
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
                sampler = TextureSamplerUtil.CreateSampler(parser.GLTF, metallicRoughnessTextureIndex.Value);
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
                sampler = TextureSamplerUtil.CreateSampler(parser.GLTF, occlusionTextureIndex.Value);
                getOcclusionAsync = () => Task.FromResult(ToArray(parser.GLTF.GetImageBytesFromTextureIndex(parser.Storage, occlusionTextureIndex.Value)));
            }

            return new TextureImportParam(name, ".png", null, offset, scale, sampler, TextureImportTypes.StandardMap, metallicFactor, roughnessFactor, getMetallicRoughnessAsync, getOcclusionAsync, default, default, default, default);
        }
    }
}
