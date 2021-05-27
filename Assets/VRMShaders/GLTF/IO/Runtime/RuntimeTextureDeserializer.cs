using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    public sealed class RuntimeTextureDeserializer : ITextureDeserializer
    {
        public async Task<Texture2D> LoadTextureAsync(GetTextureBytesAsync getTextureBytesAsync, bool useMipmap, ColorSpace colorSpace)
        {
            var imageBytes = await getTextureBytesAsync();

            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, useMipmap, colorSpace == ColorSpace.Linear);
            if (imageBytes != null)
            {
                texture.LoadImage(imageBytes);
            }

            return texture;
        }
    }
}
