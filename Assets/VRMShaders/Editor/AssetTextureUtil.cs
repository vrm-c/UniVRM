using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public static class AssetTextureUtil
    {
        /// <summary>
        /// TextureImporter.maxTextureSize が オリジナルの画像Sizeより小さいか
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool IsMaxTextureSizeSmallerThanOriginalTextureSize(Texture2D src)
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

        /// <summary>
        /// Export するときに オリジナルのテクスチャーアセット(png/jpg)を使用するか否か。
        /// 条件は、
        /// 
        /// * TextureAsset が存在する
        /// * TextureImporter の maxSize
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static bool IsTextureEditorAsset(Texture texture)
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
    }
}
