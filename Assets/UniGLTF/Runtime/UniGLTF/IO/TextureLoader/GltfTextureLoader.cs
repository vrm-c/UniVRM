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

        public static async Task<Texture2D> LoadTextureAsync(glTF gltf, IStorage storage, int index)
        {
            string m_textureName = default;

            var imageIndex = gltf.GetImageIndexFromTextureIndex(index);
            var segments = gltf.GetImageBytes(storage, imageIndex, out m_textureName);
            var imageBytes = ToArray(segments);

            //
            // texture from image(png etc) bytes
            //
            var textureType = TextureIO.GetglTFTextureType(gltf, index);
            var colorSpace = TextureIO.GetColorSpace(textureType);
            var isLinear = colorSpace == RenderTextureReadWrite.Linear;
            var sampler = gltf.GetSamplerFromTextureIndex(index);

            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, isLinear);
            texture.name = m_textureName;
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
