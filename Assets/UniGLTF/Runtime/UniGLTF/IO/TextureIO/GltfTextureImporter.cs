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

        public static (SubAssetKey, TextureDescriptor) CreateSRGB(GltfData data, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = data.GLTF.textures[textureIndex];
            var gltfImage = data.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.sRGB, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(data.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(data.GLTF.GetImageBytesFromTextureIndex(data.Storage, textureIndex)));
            var param = new TextureDescriptor(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.sRGB, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (param.SubAssetKey, param);
        }

        public static (SubAssetKey, TextureDescriptor) CreateLinear(GltfData data, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = data.GLTF.textures[textureIndex];
            var gltfImage = data.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.Linear, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(data.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(data.GLTF.GetImageBytesFromTextureIndex(data.Storage, textureIndex)));
            var param = new TextureDescriptor(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.Linear, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (param.SubAssetKey, param);
        }

        public static (SubAssetKey, TextureDescriptor) CreateNormal(GltfData data, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = data.GLTF.textures[textureIndex];
            var gltfImage = data.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.NormalMap, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(data.GLTF, textureIndex);
            GetTextureBytesAsync getTextureBytesAsync = () => Task.FromResult(ToArray(data.GLTF.GetImageBytesFromTextureIndex(data.Storage, textureIndex)));
            var param = new TextureDescriptor(name, gltfImage.GetExt(), gltfImage.uri, offset, scale, sampler, TextureImportTypes.NormalMap, default, default, getTextureBytesAsync, default, default, default, default, default);
            return (param.SubAssetKey, param);
        }

        public static (SubAssetKey, TextureDescriptor) CreateStandard(GltfData data, int? metallicRoughnessTextureIndex, int? occlusionTextureIndex, Vector2 offset, Vector2 scale, float metallicFactor, float roughnessFactor)
        {
            string name = default;

            GetTextureBytesAsync getMetallicRoughnessAsync = default;
            SamplerParam sampler = default;
            if (metallicRoughnessTextureIndex.HasValue)
            {
                var gltfTexture = data.GLTF.textures[metallicRoughnessTextureIndex.Value];
                name = TextureImportName.GetUnityObjectName(TextureImportTypes.StandardMap, gltfTexture.name, data.GLTF.images[gltfTexture.source].uri);
                sampler = TextureSamplerUtil.CreateSampler(data.GLTF, metallicRoughnessTextureIndex.Value);
                getMetallicRoughnessAsync = () => Task.FromResult(ToArray(data.GLTF.GetImageBytesFromTextureIndex(data.Storage, metallicRoughnessTextureIndex.Value)));
            }

            GetTextureBytesAsync getOcclusionAsync = default;
            if (occlusionTextureIndex.HasValue)
            {
                var gltfTexture = data.GLTF.textures[occlusionTextureIndex.Value];
                if (string.IsNullOrEmpty(name))
                {
                    name = TextureImportName.GetUnityObjectName(TextureImportTypes.StandardMap, gltfTexture.name, data.GLTF.images[gltfTexture.source].uri);
                }
                sampler = TextureSamplerUtil.CreateSampler(data.GLTF, occlusionTextureIndex.Value);
                getOcclusionAsync = () => Task.FromResult(ToArray(data.GLTF.GetImageBytesFromTextureIndex(data.Storage, occlusionTextureIndex.Value)));
            }

            var texDesc = new TextureDescriptor(name, ".png", null, offset, scale, sampler, TextureImportTypes.StandardMap, metallicFactor, roughnessFactor, getMetallicRoughnessAsync, getOcclusionAsync, default, default, default, default);
            return (texDesc.SubAssetKey, texDesc);
        }

        public static (Vector2, Vector2) GetTextureOffsetAndScale(glTFTextureInfo textureInfo)
        {
            if (glTF_KHR_texture_transform.TryGet(textureInfo, out var textureTransform))
            {
                return GetTextureOffsetAndScale(textureTransform);
            }
            return (new Vector2(0, 0), new Vector2(1, 1));
        }

        public static (Vector2, Vector2) GetTextureOffsetAndScale(glTF_KHR_texture_transform textureTransform)
        {
            var offset = new Vector2(0, 0);
            var scale = new Vector2(1, 1);

            if (textureTransform != null)
            {
                if (textureTransform.offset != null && textureTransform.offset.Length == 2)
                {
                    offset = new Vector2(textureTransform.offset[0], textureTransform.offset[1]);
                }

                if (textureTransform.scale != null && textureTransform.scale.Length == 2)
                {
                    scale = new Vector2(textureTransform.scale[0], textureTransform.scale[1]);
                }

                // UV Coordinate Conversion: glTF(top-left origin) to Unity(bottom-left origin)
                // Formula: https://github.com/vrm-c/UniVRM/issues/930
                offset.y = 1.0f - offset.y - scale.y;
            }

            return (offset, scale);
        }
    }
}
