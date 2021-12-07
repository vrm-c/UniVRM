using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// Unity の ImageConversion.LoadImage を用いて PNG/JPG の読み込みを実現する
    /// </summary>
    public sealed class UnityTextureDeserializer : ITextureDeserializer
    {
        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, textureInfo.UseMipmap, textureInfo.ColorSpace == ColorSpace.Linear);
            if (textureInfo.ImageData != null)
            {
                texture.LoadImage(textureInfo.ImageData);
                texture.wrapModeU = textureInfo.WrapModeU;
                texture.wrapModeV = textureInfo.WrapModeV;
                texture.filterMode = textureInfo.FilterMode;
                await awaitCaller.NextFrame();
            }

            return texture;
        }
    }
}
