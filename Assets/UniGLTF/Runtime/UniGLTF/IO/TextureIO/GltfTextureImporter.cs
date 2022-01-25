using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    /// <summary>
    /// glTFTexture を TextureImportParam に変換する
    /// </summary>
    public static class GltfTextureImporter
    {
        /// <summary>
        /// glTF の Texture が存在せず Image のみのものを、Texture として扱いたい場合の関数.
        /// </summary>
        public static (SubAssetKey, TextureDescriptor) CreateSrgbFromOnlyImage(GltfData data, int imageIndex, string uniqueName, string uri)
        {
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.sRGB, uniqueName, uri);
            var texDesc = new TextureDescriptor(
                name,
                Vector2.zero,
                Vector2.one,
                default,
                TextureImportTypes.sRGB,
                default,
                default,
                () =>
                {
                    var imageBytes = data.GetBytesFromImage(imageIndex);
                    return Task.FromResult<(byte[], string)?>((ToArray(imageBytes?.binary ?? default), null));
                },
                default, default, default, default, default);
            return (texDesc.SubAssetKey, texDesc);
        }

        public static (SubAssetKey, TextureDescriptor) CreateSrgb(GltfData data, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = data.GLTF.textures[textureIndex];
            var gltfImage = data.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.sRGB, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(data.GLTF, textureIndex);
            var param = new TextureDescriptor(
                name,
                offset, scale,
                sampler,
                TextureImportTypes.sRGB,
                default,
                default,
                () => Task.FromResult(GetImageBytesFromTextureIndex(data, textureIndex)),
                default, default, default, default, default);
            return (param.SubAssetKey, param);
        }

        public static (SubAssetKey, TextureDescriptor) CreateLinear(GltfData data, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = data.GLTF.textures[textureIndex];
            var gltfImage = data.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.Linear, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(data.GLTF, textureIndex);
            var param = new TextureDescriptor(
                name,
                offset,
                scale,
                sampler,
                TextureImportTypes.Linear,
                default,
                default,
                () => Task.FromResult(GetImageBytesFromTextureIndex(data, textureIndex)),
                default, default, default, default, default);
            return (param.SubAssetKey, param);
        }

        public static (SubAssetKey, TextureDescriptor) CreateNormal(GltfData data, int textureIndex, Vector2 offset, Vector2 scale)
        {
            var gltfTexture = data.GLTF.textures[textureIndex];
            var gltfImage = data.GLTF.images[gltfTexture.source];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.NormalMap, gltfTexture.name, gltfImage.uri);
            var sampler = TextureSamplerUtil.CreateSampler(data.GLTF, textureIndex);
            var param = new TextureDescriptor(
                name,
                offset,
                scale,
                sampler,
                TextureImportTypes.NormalMap,
                default,
                default,
                () => Task.FromResult(GetImageBytesFromTextureIndex(data, textureIndex)),
                default, default, default, default, default);
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
                getMetallicRoughnessAsync = () => Task.FromResult(GetImageBytesFromTextureIndex(data, metallicRoughnessTextureIndex.Value));
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
                getOcclusionAsync = () => Task.FromResult(GetImageBytesFromTextureIndex(data, occlusionTextureIndex.Value));
            }

            var texDesc = new TextureDescriptor(
                name,
                offset,
                scale,
                sampler,
                TextureImportTypes.StandardMap,
                metallicFactor,
                roughnessFactor,
                getMetallicRoughnessAsync,
                getOcclusionAsync,
                default, default, default, default);
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

                (scale, offset) = TextureTransform.VerticalFlipScaleOffset(scale, offset);
            }

            return (offset, scale);
        }

        public static (byte[] binary, string mimeType)? GetImageBytesFromTextureIndex(GltfData data, int textureIndex)
        {
            if (Application.isPlaying)
            {
                // NOTE: Runtime の場合は、拡張を考える.
                var imageIndex = GetImageIndexFromTexture(data, textureIndex);

                if (textureIndex >= 0 && textureIndex < data.GLTF.textures.Count)
                {
                    var texture = data.GLTF.textures[textureIndex];
                    if (glTF_KHR_texture_basisu.TryGet(texture, out var basisuExtension))
                    {
                        imageIndex = basisuExtension.source;
                    }
                }

                if (!imageIndex.HasValue) return default;

                var imageBytes = data.GetBytesFromImage(imageIndex.Value);
                return (ToArray(imageBytes?.binary ?? default), imageBytes?.mimeType);
            }
            else
            {
                // NOTE: Editor の場合は通常通り読み込む.
                var imageIndex = GetImageIndexFromTexture(data, textureIndex);
                if (!imageIndex.HasValue) return default;

                var imageBytes = data.GetBytesFromImage(imageIndex.Value);
                return (ToArray(imageBytes?.binary ?? default), imageBytes?.mimeType);
            }
        }

        private static int? GetImageIndexFromTexture(GltfData data, int textureIndex)
        {
            if (textureIndex < 0 || textureIndex >= data.GLTF.textures.Count) return default;

            return data.GLTF.textures[textureIndex].source;
        }

        private static byte[] ToArray(NativeArray<byte> bytes)
        {
            // if (bytes.Array == null)
            // {
            //     return new byte[] { };
            // }
            // else if (bytes.Offset == 0 && bytes.Count == bytes.Array.Length)
            // {
            //     return bytes.Array;
            // }
            // else
            // {
            //     var result = new byte[bytes.Count];
            //     Buffer.BlockCopy(bytes.Array, bytes.Offset, result, 0, result.Length);
            //     return result;
            // }
            return bytes.ToArray();
        }
    }
}
