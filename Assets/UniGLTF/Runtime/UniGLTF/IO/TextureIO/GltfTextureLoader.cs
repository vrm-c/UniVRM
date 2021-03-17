using System;
using System.Threading.Tasks;
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

        public static async Task<Texture2D> LoadTextureAsync(IAwaitCaller awaitCaller, glTF gltf, IStorage storage, int textureIndex)
        {
            var imageBytes = await awaitCaller.Run(() =>
            {
                var imageIndex = gltf.textures[textureIndex].source;
                var segments = gltf.GetImageBytes(storage, imageIndex);
                return ToArray(segments);
            });

            //
            // texture from image(png etc) bytes
            //
            var colorSpace = gltf.GetColorSpace(textureIndex);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, colorSpace == RenderTextureReadWrite.Linear);
            texture.name = gltf.textures[textureIndex].name;
            if (imageBytes != null)
            {
                texture.LoadImage(imageBytes);
            }

            var sampler = gltf.GetSamplerFromTextureIndex(textureIndex);
            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(texture, sampler);
            }
            return texture;
        }
    }
}
