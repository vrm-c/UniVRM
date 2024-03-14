using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
#if USE_COM_UNITY_CLOUD_KTX
using KtxUnity;
#endif

namespace VRMShaders
{
    public sealed class KtxTextureDeserializer : ITextureDeserializer
    {
        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
#if USE_COM_UNITY_CLOUD_KTX
            if (textureInfo.ImageData == null) return null;

            // NOTE: IAwaitCaller を無視するので、同期読み込みを期待する環境で同期読み込みができない
            try
            {
                var ktxTexture = new KtxTexture();
                using var nativeBytes = new NativeArray<byte>(textureInfo.ImageData, Allocator.Persistent);
                var result = await ktxTexture.LoadFromBytes(
                    nativeBytes,
                    linear: textureInfo.ColorSpace == ColorSpace.Linear,
                    mipChain: textureInfo.UseMipmap
                );
                if (result is { errorCode: ErrorCode.Success })
                {
                    result.texture.wrapModeU = textureInfo.WrapModeU;
                    result.texture.wrapModeV = textureInfo.WrapModeV;
                    result.texture.filterMode = textureInfo.FilterMode;
                    return result.texture;
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
#else
            return null;
#endif
        }
    }
}