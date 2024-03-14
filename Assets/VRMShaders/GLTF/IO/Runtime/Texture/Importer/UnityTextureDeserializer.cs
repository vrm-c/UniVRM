using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
#if USE_COM_UNITY_CLOUD_KTX
using KtxUnity;
#endif

namespace VRMShaders
{
    /// <summary>
    /// Unity の ImageConversion.LoadImage を用いて PNG/JPG の読み込みを実現する
    /// </summary>
    public sealed class UnityTextureDeserializer : ITextureDeserializer
    {
        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            Texture2D texture = null;
            switch (textureInfo.DataMimeType)
            {
                case "image/png":
                    break;
                case "image/jpeg":
                    break;
#if USE_COM_UNITY_CLOUD_KTX
                case "image/ktx":
                    var ktxTexture = new KtxTexture();
                    var nativeBytes = new NativeArray<byte>(textureInfo.ImageData, Allocator.Temp);
                    try
                    {
                        var nativeSlice = new NativeSlice<byte>(nativeBytes);
                        var result = await ktxTexture.LoadFromBytes(nativeSlice, textureInfo.ColorSpace == ColorSpace.Linear);
                        if (result != null && result.errorCode == ErrorCode.Success)
                        {
                            texture = result.texture;
                        }
                        break;
                    }
                    finally
                    {
                        nativeBytes.Dispose();
                    }
#endif
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
                texture = new Texture2D(2, 2, TextureFormat.ARGB32, textureInfo.UseMipmap, textureInfo.ColorSpace == ColorSpace.Linear);
            }
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
