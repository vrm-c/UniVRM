using System;
using System.Collections;
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

        Byte[] m_imageBytes;
        string m_textureName;
        public void ProcessOnAnyThread(glTF gltf, IStorage storage)
        {
            var imageIndex = gltf.GetImageIndexFromTextureIndex(m_textureIndex);
            var segments = gltf.GetImageBytes(storage, imageIndex, out m_textureName);
            m_imageBytes = ToArray(segments);
        }

        public IEnumerator ProcessOnMainThread(bool isLinear, glTFTextureSampler sampler)
        {
            //
            // texture from image(png etc) bytes
            //
            Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, isLinear);
            Texture.name = m_textureName;
            if (m_imageBytes != null)
            {
                Texture.LoadImage(m_imageBytes);
            }
            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(Texture, sampler);
            }
            yield break;
        }
    }
}
