using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public sealed class EditorTextureSerializer : ITextureSerializer
    {
        private readonly RuntimeTextureSerializer _runtimeSerializer = new RuntimeTextureSerializer();

        /// <summary>
        /// Texture をオリジナルのテクスチャアセット(png/jpg)ファイルのバイト列そのまま出力してよいかどうか判断する。
        /// 具体的な条件は以下
        ///
        /// * TextureAsset が存在する
        /// * TextureImporter の maxSize が画像の縦横サイズ以上
        /// * TextureImporter の色空間設定が exportColorSpace と一致する
        /// * 各 Texture Type ごとの判定
        ///
        /// Unity の Texture2D のデータは、その参照元であるテクスチャアセットファイルのデータと一致することはむしろ稀。
        /// </summary>
        public bool CanExportAsEditorAssetFile(Texture texture, ColorSpace exportColorSpace)
        {
            // Exists as UnityEditor Texture2D Assets ?
            if (!EditorTextureUtility.TryGetAsEditorTexture2DAsset(texture, out var texture2D, out var textureImporter)) return false;

            // Maintain original width/height ?
            if (!IsTextureSizeMaintained(textureImporter)) return false;

            // Equals color space ?
            if (!IsFileColorSpaceSameWithExportColorSpace(textureImporter, exportColorSpace)) return false;

            // Each Texture Importer Type Validation
            switch (textureImporter.textureType)
            {
                case TextureImporterType.Default:
                    break;
                case TextureImporterType.NormalMap:
                    // A texture has "Normal map" TextureType is ALWAYS converted into normalized normal pixel by Unity.
                    // So we must copy it.
                    return false;
                default:
                    // Not Supported TextureImporterType
                    throw new ArgumentException($"{texture}: textureImporter.textureType {textureImporter.textureType} is not supported. Only Default or NormalMap is supported");
            }

            return true;
        }

        public (byte[] bytes, string mime) ExportBytesWithMime(Texture2D texture, ColorSpace exportColorSpace)
        {
            if (CanExportAsEditorAssetFile(texture, exportColorSpace) && TryGetBytesWithMime(texture, out byte[] bytes, out string mime))
            {
                return (bytes, mime);
            }

            return _runtimeSerializer.ExportBytesWithMime(texture, exportColorSpace);
        }

        /// <summary>
        /// 出力に使用したいテクスチャに対して、Unity のエディタアセットとしての圧縮設定を OFF にする。
        /// </summary>
        public void ModifyTextureAssetBeforeExporting(Texture texture)
        {
            if (EditorTextureUtility.TryGetAsEditorTexture2DAsset(texture, out var texture2D, out var assetImporter))
            {
                assetImporter.textureCompression = TextureImporterCompression.Uncompressed;
                assetImporter.SaveAndReimport();
            }
        }

        /// <summary>
        /// Assetから画像のバイト列を得る
        /// </summary>
        private bool TryGetBytesWithMime(Texture2D texture, out byte[] bytes, out string mime)
        {
            var path = AssetDatabase.GetAssetOrScenePath(texture);
            if (string.IsNullOrEmpty(path))
            {
                bytes = default;
                mime = default;
                return false;
            }

            var ext = Path.GetExtension(path).ToLower();

            switch (ext)
            {
                case ".png":
                    bytes = System.IO.File.ReadAllBytes(path);
                    mime = "image/png";
                    return true;

                case ".jpg":
                    bytes = System.IO.File.ReadAllBytes(path);
                    mime = "image/jpeg";
                    return true;
            }

            // dds ? astc ? tga ?

            bytes = default;
            mime = default;
            return false;
        }

        /// <summary>
        /// Texture2D の画像サイズが、オリジナルの画像サイズを維持しているかどうか
        ///
        /// TextureImporter の MaxTextureSize 設定によっては、Texture2D の画像サイズはオリジナルも小さくなりうる。
        /// </summary>
        private bool IsTextureSizeMaintained(TextureImporter textureImporter)
        {
            if (EditorTextureUtility.TryGetOriginalTexturePixelSize(textureImporter, out var originalSize))
            {
                var originalMaxSize = Mathf.Max(originalSize.x, originalSize.y);
                if (textureImporter.maxTextureSize >= originalMaxSize)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFileColorSpaceSameWithExportColorSpace(TextureImporter textureImporter, ColorSpace colorSpace)
        {
            switch (colorSpace)
            {
                case ColorSpace.sRGB:
                    return textureImporter.sRGBTexture == true;
                case ColorSpace.Linear:
                    return textureImporter.sRGBTexture == false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorSpace), colorSpace, null);
            }
        }
    }
}
