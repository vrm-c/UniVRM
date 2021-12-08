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
            switch (textureInfo.DataMimeType)
            {
                case "image/png":
                    break;
                case "image/jpeg":
                    break;
                default:
                    Debug.LogWarning($"Texture image MIME type `{textureInfo.DataMimeType}` is not supported.");
                    break;
            }

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
