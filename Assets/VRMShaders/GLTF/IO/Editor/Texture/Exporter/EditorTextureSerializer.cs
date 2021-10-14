using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public sealed class EditorTextureSerializer : ITextureSerializer
    {
        private readonly RuntimeTextureSerializer m_runtimeSerializer = new RuntimeTextureSerializer();

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
            if (!TryGetAsEditorTexture2DAsset(texture, out var texture2D, out var textureImporter)) return false;

            // Maintain original width/height ?
            if (!IsTextureSizeMaintained(texture2D, textureImporter)) return false;

            // Equals color space ?
            if (!IsFileColorSpaceSameWithExportColorSpace(texture2D, textureImporter, exportColorSpace)) return false;

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
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public (byte[] bytes, string mime) ExportBytesWithMime(Texture2D texture, ColorSpace exportColorSpace)
        {
            if (CanExportAsEditorAssetFile(texture, exportColorSpace) && TryGetBytesWithMime(texture, out byte[] bytes, out string mime))
            {
                return (bytes, mime);
            }

            return m_runtimeSerializer.ExportBytesWithMime(texture, exportColorSpace);
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

        private bool TryGetAsEditorTexture2DAsset(Texture texture, out Texture2D texture2D, out TextureImporter assetImporter)
        {
            texture2D = texture as Texture2D;
            if (texture2D != null)
            {
                var path = AssetDatabase.GetAssetPath(texture2D);
                if (!string.IsNullOrEmpty(path))
                {
                    assetImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (assetImporter != null)
                    {
                        return true;
                    }
                }
            }

            texture2D = null;
            assetImporter = null;
            return false;
        }

        /// <summary>
        /// Texture2D の画像サイズが、オリジナルの画像サイズを維持しているかどうか
        ///
        /// TextureImporter の MaxTextureSize 設定によっては、Texture2D の画像サイズはオリジナルも小さくなりうる。
        /// </summary>
        private bool IsTextureSizeMaintained(Texture2D texture, TextureImporter textureImporter)
        {
            // private メソッド TextureImporter.GetWidthAndHeight を無理やり呼ぶ
            var getSizeMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            if (textureImporter != null && getSizeMethod != null)
            {
                var args = new object[2] { 0, 0 };
                getSizeMethod.Invoke(textureImporter, args);
                var originalWidth = (int)args[0];
                var originalHeight = (int)args[1];
                var originalSize = Mathf.Max(originalWidth, originalHeight);
                if (textureImporter.maxTextureSize >= originalSize)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFileColorSpaceSameWithExportColorSpace(Texture2D texture, TextureImporter textureImporter, ColorSpace colorSpace)
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
