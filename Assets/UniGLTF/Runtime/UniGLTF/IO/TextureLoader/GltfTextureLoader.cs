using System;
using UniGLTF.AltTask;
using UnityEngine;

namespace UniGLTF
{
    public static class GltfTextureLoader
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

        public static async Awaitable<Texture2D> LoadTextureAsync(glTF gltf, IStorage storage, int textureIndex)
        {
            string textureName = default;
            var imageBytes = await Awaitable.Run(() =>
            {
                var imageIndex = gltf.textures[textureIndex].source;
                var segments = gltf.GetImageBytes(storage, imageIndex);
                return ToArray(segments);
            });

            //
            // texture from image(png etc) bytes
            //
            var textureType = TextureIO.GetglTFTextureType(gltf, textureIndex);
            var colorSpace = TextureIO.GetColorSpace(textureType);
            var isLinear = colorSpace == RenderTextureReadWrite.Linear;
            var sampler = gltf.GetSamplerFromTextureIndex(textureIndex);

            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, isLinear);
            texture.name = textureName;
            if (imageBytes != null)
            {
                texture.LoadImage(imageBytes);
            }
            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(texture, sampler);
            }
            return texture;
        }
    }
}
