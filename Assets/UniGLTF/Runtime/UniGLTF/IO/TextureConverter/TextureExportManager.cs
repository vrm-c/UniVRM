using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniGLTF
{
    /// <summary>
    /// glTF にエクスポートする Texture2D を蓄えて index を確定させる
    /// </summary>
    public class TextureExportManager
    {
        struct ExportKey
        {
            public readonly Texture Src;
            public readonly glTFTextureTypes TextureType;

            public ExportKey(Texture src, glTFTextureTypes type)
            {
                if (src == null)
                {
                    throw new ArgumentNullException();
                }
                Src = src;
                TextureType = type;
            }
        }
        Dictionary<ExportKey, int> m_exportMap = new Dictionary<ExportKey, int>();
        List<Texture2D> m_exported = new List<Texture2D>();

        public IReadOnlyList<Texture2D> Exported => m_exported;

        static bool CopyIfMaxTextureSizeIsSmaller(Texture src/*, glTFTextureTypes textureType, out Texture2D dst*/)
        {
#if UNITY_EDITOR            
            var textureImporter = AssetImporter.GetAtPath(UnityPath.FromAsset(src).Value) as TextureImporter;
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
                    // export resized texture.
                    // this has textureImporter.maxTextureSize
                    // dst = TextureConverter.CopyTexture(src, textureType, null);
                    return true;
                }
            }
#endif

            // dst = default;
            return false;
        }

        /// <summary>
        /// Texture の export index を得る
        /// </summary>
        /// <param name="src"></param>
        /// <param name="textureType"></param>
        /// <returns></returns>
        public int GetTextureIndex(Texture src, glTFTextureTypes textureType)
        {
            return m_exportMap[new ExportKey(src, textureType)];
        }

        /// <summary>
        /// sRGBなテクスチャーを処理する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public int ExportSRGB(Texture src)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, glTFTextureTypes.SRGB), out var index))
            {
                return index;
            }

            // get Texture2D
            index = m_exported.Count;
            if (src is Texture2D texture2D && !CopyIfMaxTextureSizeIsSmaller(src))
            {
                // do nothing                
            }
            else
            {
                texture2D = TextureConverter.CopyTexture(src, glTFTextureTypes.SRGB, null);
            }
            m_exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(src, glTFTextureTypes.SRGB), index);

            return index;
        }

        /// <summary>
        /// Standard の Metallic, Smoothness, Occlusion をまとめる
        /// </summary>
        /// <param name="metallicSmoothTexture"></param>
        /// <param name="smoothness"></param>
        /// <param name="occlusionTexture"></param>
        /// <returns></returns>
        public int ExportMetallicSmoothnessOcclusion(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture == null && occlusionTexture == null)
            {
                throw new ArgumentNullException();
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(metallicSmoothTexture, glTFTextureTypes.OcclusionMetallicRoughness), out var index))
            {
                return index;
            }
            if (m_exportMap.TryGetValue(new ExportKey(occlusionTexture, glTFTextureTypes.OcclusionMetallicRoughness), out index))
            {
                return index;
            }

            //
            // Unity と glTF で互換性が無いので必ず変換が必用
            //
            index = m_exported.Count;
            var texture2D = OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture);

            m_exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(metallicSmoothTexture, glTFTextureTypes.OcclusionMetallicRoughness), index);
            if (occlusionTexture != metallicSmoothTexture && occlusionTexture != null)
            {
                m_exportMap.Add(new ExportKey(occlusionTexture, glTFTextureTypes.OcclusionMetallicRoughness), index);
            }

            return index;
        }

        static bool UseNormalAsset(Texture src, out Texture2D texture2D)
        {
#if UNITY_EDITOR
            // asset として存在して textureImporter.textureType = TextureImporterType.NormalMap
            texture2D = src as Texture2D;
            if (texture2D != null && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(src)))
            {
                if (CopyIfMaxTextureSizeIsSmaller(src))
                {
                    return false;
                }
                return true;
            }
#endif

            texture2D = default;
            return false;
        }

        /// <summary>
        /// Normal のテクスチャを変換する
        /// </summary>
        /// <param name="normalTexture"></param>
        /// <returns></returns>
        public int ExportNormal(Texture src)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, glTFTextureTypes.Normal), out var index))
            {
                return index;
            }

            // get Texture2D
            index = m_exported.Count;
            Texture2D texture2D = default;
            if (UseNormalAsset(src, out texture2D))
            {
                // EditorAsset を使うので変換不要
            }
            else
            {
                // 後で Bitmap を使うために変換する
                texture2D = NormalConverter.Export(src);
            }

            m_exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(src, glTFTextureTypes.Normal), index);

            return index;
        }
    }
}
