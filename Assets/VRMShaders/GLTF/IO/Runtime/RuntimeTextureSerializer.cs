using System;
using UnityEngine;
using VRMShaders;

namespace VRMShaders
{
    public sealed class RuntimeTextureSerializer : ITextureSerializer
    {
        public bool CanExportAsEditorAssetFile(Texture texture, ColorSpace exportColorSpace)
        {
            return false;
        }

        public (byte[] bytes, string mime) ExportBytesWithMime(Texture2D texture, ColorSpace exportColorSpace)
        {
            try
            {
                var png = texture.EncodeToPNG();
                if (png != null)
                {
                    return (png, "image/png");
                }
                else
                {
                    // Failed, because texture is compressed.
                    // ex. ".DDS" file, or Compression is enabled in Texture Import Settings.
                    return CopyTextureAndGetBytesWithMime(texture, exportColorSpace);
                }
            }
            catch (ArgumentException ex)
            {
                // System.ArgumentException: not readable, the texture memory can not be accessed from scripts. You can make the texture readable in the Texture Import Settings.

                // Failed, because texture is not readable.
                Debug.LogWarning(ex);

                // 単純に EncodeToPNG できないため、コピーしてから EncodeToPNG する。
                return CopyTextureAndGetBytesWithMime(texture, exportColorSpace);
            }
        }

        private static (byte[] bytes, string mime) CopyTextureAndGetBytesWithMime(Texture2D texture, ColorSpace colorSpace)
        {
            var needsAlpha = texture.format != TextureFormat.RGB24;
            var copiedTex = TextureConverter.CopyTexture(texture, colorSpace, needsAlpha, null);
            var bytes = copiedTex.EncodeToPNG();
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(copiedTex);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(copiedTex);
            }

            return (bytes, "image/png");
        }
    }
}
