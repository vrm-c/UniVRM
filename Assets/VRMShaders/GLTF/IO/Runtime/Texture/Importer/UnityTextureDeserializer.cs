using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// Runtime での Texture2D 生成を実現するデフォルトの実装
    /// </summary>
    public sealed class UnityTextureDeserializer : ITextureDeserializer
    {
        private readonly UnitySupportedImageTypeDeserializer _unitySupportedDeserializer = new();
        private readonly KtxTextureDeserializer _ktxTextureDeserializer = new();

        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            Texture2D texture = null;
            switch (textureInfo.DataMimeType)
            {
                case "image/png":
                    texture = await _unitySupportedDeserializer.LoadTextureAsync(textureInfo, awaitCaller);
                    break;
                case "image/jpeg":
                    texture = await _unitySupportedDeserializer.LoadTextureAsync(textureInfo, awaitCaller);
                    break;
                case "image/ktx2":
                    texture = await _ktxTextureDeserializer.LoadTextureAsync(textureInfo, awaitCaller);
                    break;
                default:
                    if (string.IsNullOrEmpty(textureInfo.DataMimeType))
                    {
                        Debug.Log($"Texture image MIME type is empty.");
                    }
                    else
                    {
                        Debug.Log($"Texture image MIME type `{textureInfo.DataMimeType}` is not supported.");
                    }
                    break;
            }

            if (texture == null)
            {
                Debug.Log($"Failed to load texture from image data.");
                texture = new Texture2D(2, 2, TextureFormat.ARGB32, textureInfo.UseMipmap, textureInfo.ColorSpace == ColorSpace.Linear);
            }

            return texture;
        }
    }
}
