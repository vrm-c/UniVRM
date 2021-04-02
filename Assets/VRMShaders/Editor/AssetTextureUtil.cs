using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public static class AssetTextureUtil
    {
        /// <summary>
        /// TextureImporter.maxTextureSize が元のテクスチャーより小さいか否かの判定
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool CopyIfMaxTextureSizeIsSmaller(Texture src)
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
        /// 元の Asset が存在して、 TextureImporter に設定された画像サイズが小さくない
        /// </summary>
        /// <param name="src"></param>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static bool UseAsset(Texture texture)
        {
            if (texture != null && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(texture)))
            {
                if (CopyIfMaxTextureSizeIsSmaller(texture))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
