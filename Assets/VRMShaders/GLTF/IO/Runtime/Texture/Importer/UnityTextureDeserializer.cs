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
        public async Task<Texture2D> LoadTextureAsync(byte[] imageData, bool useMipmap, ColorSpace colorSpace, IAwaitCaller awaitCaller)
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, useMipmap, colorSpace == ColorSpace.Linear);
            if (imageData != null)
            {
                texture.LoadImage(imageData);
                await awaitCaller.NextFrame();
            }

            return texture;
        }
    }
}
