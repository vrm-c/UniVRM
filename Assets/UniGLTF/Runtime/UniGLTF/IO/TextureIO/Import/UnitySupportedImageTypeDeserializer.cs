using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Unity の ImageConversion.LoadImage を用いて PNG/JPG の読み込みを実現する
    /// </summary>
    public sealed class UnitySupportedImageTypeDeserializer : ITextureDeserializer
    {
        /// <summary>
        /// `true` を指定すると、テクスチャを Non-Readable なものとしてデシリアライズする。
        /// </summary>
        /// <remarks>
        /// `UnityEngine.ImageConversion.LoadImage` の第二引数 `markNonReadable` に相当。
        /// デフォルト値は `false`。
        /// テクスチャ編集を行わないアプリケーションプログラム等では、
        /// この値を `true` にすることでメモリ使用量の削減を期待できる。
        /// このフラグの効用については `UnityEngine.Texture2D.Apply` に記述がある。
        /// </remarks>
        public bool MarkNonReadable { get; set; } = false;

        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            if (textureInfo.ImageData == null) return null;

            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, textureInfo.UseMipmap, textureInfo.ColorSpace == ColorSpace.Linear);
                texture.LoadImage(textureInfo.ImageData, MarkNonReadable);
                await awaitCaller.NextFrame();

                texture.wrapModeU = textureInfo.WrapModeU;
                texture.wrapModeV = textureInfo.WrapModeV;
                texture.filterMode = textureInfo.FilterMode;
                return texture;
            }
            catch (Exception e)
            {
                UniGLTFLogger.Exception(e);
                return null;
            }
        }
    }
}