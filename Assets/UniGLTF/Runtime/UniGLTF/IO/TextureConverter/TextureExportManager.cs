using System;
using System.Collections.Generic;
using UnityEngine;


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
            if (src is Texture2D texture2D)
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
            m_exportMap.Add(new ExportKey(occlusionTexture, glTFTextureTypes.OcclusionMetallicRoughness), index);

            return index;
        }

        static bool UseNormalAsset(Texture src, out Texture2D texture2D)
        {
#if UNITY_EDITOR
            // asset として存在して textureImporter.textureType = TextureImporterType.NormalMap
            texture2D = src as Texture2D;
            if (texture2D != null && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(src)))
            {
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
