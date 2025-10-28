using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    internal static class EditorTextureUtility
    {
        public static bool TryGetAsEditorTexture2DAsset(Texture texture, out Texture2D texture2D, out TextureImporter assetImporter)
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

        public static bool TryGetOriginalTexturePixelSize(TextureImporter textureImporter, out Vector2Int size)
        {
            // private メソッド TextureImporter.GetWidthAndHeight を無理やり呼ぶ
            var getSizeMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            if (textureImporter != null && getSizeMethod != null)
            {
                var args = new object[2] { 0, 0 };
                getSizeMethod.Invoke(textureImporter, args);
                var originalWidth = (int)args[0];
                var originalHeight = (int)args[1];

                size = new Vector2Int(originalWidth, originalHeight);
                return true;
            }

            size = default;
            return false;
        }
    }
}