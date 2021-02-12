using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public class GltfTextureLoader : ITextureLoader
    {
        int m_textureIndex;
        public GltfTextureLoader(int textureIndex)
        {
            m_textureIndex = textureIndex;
        }

        public Texture2D Texture
        {
            private set;
            get;
        }

        public void Dispose()
        {
        }

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

        string m_textureName;

        public IEnumerator ProcessOnMainThread(glTF gltf, IStorage storage, bool isLinear, glTFTextureSampler sampler)
        {
            Byte[] imageBytes = default;
            var task = Task.Run(() =>
            {
                var imageIndex = gltf.GetImageIndexFromTextureIndex(m_textureIndex);
                var segments = gltf.GetImageBytes(storage, imageIndex, out m_textureName);
                var m_imageBytes = ToArray(segments);
            });
            while (!task.IsCompleted)
            {
                yield break;
            }

            //
            // texture from image(png etc) bytes
            //
            Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, isLinear);
            Texture.name = m_textureName;
            if (imageBytes != null)
            {
                Texture.LoadImage(imageBytes);
            }
            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(Texture, sampler);
            }
            yield break;
        }
    }
}
