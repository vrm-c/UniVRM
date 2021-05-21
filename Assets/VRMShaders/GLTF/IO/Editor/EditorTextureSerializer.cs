using System.IO;
using System.Reflection;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;

namespace VRMShaders
{
    public sealed class EditorTextureSerializer : ITextureSerializer
    {
        private readonly RuntimeTextureSerializer m_runtimeSerializer = new RuntimeTextureSerializer();

        /// <summary>
        /// Export するときに オリジナルのテクスチャーアセット(png/jpg)を使用するか否か。
        /// 条件は、
        ///
        /// * TextureAsset が存在する
        /// * TextureImporter の maxSize
        ///
        /// </summary>
        public bool CanExportAsEditorAssetFile(Texture texture, ColorSpace exportColorSpace)
        {
            if (texture is Texture2D texture2D && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(texture2D)))
            {
                // exists Texture2D asset
                if (IsMaxTextureSizeSmallerThanOriginalTextureSize(texture2D))
                {
                    // Texture Inspector の MaxSize 設定で、テクスチャをオリジナルサイズよりも小さいサイズで Texture 化する指示を行っているため
                    // glTF Exporter もそれにしたがって、解釈をする
                    //
                    // 4096x4096 のような巨大なテクスチャーがそのまま出力されることを、Unityの TextureImporter.maxSize により防止する
                    //
                    return false;
                }

                // use Texture2D asset. EncodeToPng
                return true;
            }

            // not Texture2D or not exists Texture2D asset. EncodeToPng
            return false;
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

        /// <summary>
        /// TextureImporter.maxTextureSize が オリジナルの画像Sizeより小さいか
        /// </summary>
        private bool IsMaxTextureSizeSmallerThanOriginalTextureSize(Texture2D src)
        {
            var path = AssetDatabase.GetAssetPath(src);
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            // private メソッド TextureImporter.GetWidthAndHeight を無理やり呼ぶ
            var getSizeMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            if (textureImporter != null && getSizeMethod != null)
            {
                var args = new object[2] { 0, 0 };
                getSizeMethod.Invoke(textureImporter, args);
                var originalWidth = (int)args[0];
                var originalHeight = (int)args[1];
                var originalSize = Mathf.Max(originalWidth, originalHeight);
                if (textureImporter.maxTextureSize < originalSize)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
