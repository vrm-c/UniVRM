using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;


namespace VRMShaders
{
    /// <summary>
    /// glTF にエクスポートする Texture2D を蓄えて index を確定させる。
    /// Exporter の最後でまとめて Texture2D から bytes 列を得て出力する。
    /// </summary>
    public class TextureExporter : IDisposable, ITextureExporter
    {
        private ITextureSerializer m_textureSerializer;

        public TextureExporter(ITextureSerializer textureSerializer)
        {
            m_textureSerializer = textureSerializer;
        }

        public void Dispose()
        {
            // TODO: export 用にコピー・変換したテクスチャーをここで解放したい
        }

        struct ExportKey
        {
            public readonly Texture Src;
            public readonly TextureExportTypes TextureType;

            public ExportKey(Texture src, TextureExportTypes type)
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

        /// <summary>
        /// Export する Texture2D のリスト。これが gltf.textures になる
        /// </summary>
        /// <typeparam name="Texture2D"></typeparam>
        /// <returns></returns>
        public readonly List<(Texture2D, ColorSpace)> Exported = new List<(Texture2D, ColorSpace)>();

        /// <summary>
        /// Texture の export index を得る
        /// </summary>
        /// <param name="src"></param>
        /// <param name="textureType"></param>
        /// <returns></returns>
        public int GetTextureIndex(Texture src, TextureExportTypes textureType)
        {
            if (src == null)
            {
                return -1;
            }
            return m_exportMap[new ExportKey(src, textureType)];
        }

        /// <summary>
        /// sRGBなテクスチャーを処理し、index を確定させる
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public int ExportSRGB(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, TextureExportTypes.None), out var index))
            {
                return index;
            }

            // get Texture2D
            index = Exported.Count;
            var texture2D = src as Texture2D;
            if (m_textureSerializer.CanExportAsEditorAssetFile(texture2D))
            {
                // do nothing
            }
            else
            {
                texture2D = TextureConverter.CopyTexture(src, TextureImportTypes.sRGB, null);
            }
            Exported.Add((texture2D, ColorSpace.sRGB));
            m_exportMap.Add(new ExportKey(src, TextureExportTypes.None), index);

            return index;
        }

        /// <summary>
        /// Linearなテクスチャーを処理し、index を確定させる
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public int ExportLinear(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            var exportKey = new ExportKey(src, TextureExportTypes.None);

            // search cache
            if (m_exportMap.TryGetValue(exportKey, out var index))
            {
                return index;
            }

            index = Exported.Count;
            var texture2d = src as Texture2D;
            if (m_textureSerializer.CanExportAsEditorAssetFile(texture2d))
            {
                // do nothing
            }
            else
            {
                texture2d = TextureConverter.CopyTexture(src, TextureImportTypes.Linear, null);
            }
            Exported.Add((texture2d, ColorSpace.Linear));
            m_exportMap.Add(exportKey, index);

            return index;
        }

        /// <summary>
        /// Standard の Metallic, Smoothness, Occlusion をまとめ、index を確定させる
        /// </summary>
        /// <param name="metallicSmoothTexture"></param>
        /// <param name="smoothness"></param>
        /// <param name="occlusionTexture"></param>
        /// <returns></returns>
        public int ExportMetallicSmoothnessOcclusion(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture == null && occlusionTexture == null)
            {
                return -1;
            }

            // cache
            if (metallicSmoothTexture != null && m_exportMap.TryGetValue(new ExportKey(metallicSmoothTexture, TextureExportTypes.OcclusionMetallicRoughness), out var index))
            {
                return index;
            }
            if (occlusionTexture != null && m_exportMap.TryGetValue(new ExportKey(occlusionTexture, TextureExportTypes.OcclusionMetallicRoughness), out index))
            {
                return index;
            }

            //
            // Unity と glTF で互換性が無いので必ず変換が必用
            //
            index = Exported.Count;
            var texture2D = OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture);

            Exported.Add((texture2D, ColorSpace.Linear));
            if (metallicSmoothTexture != null)
            {
                m_exportMap.Add(new ExportKey(metallicSmoothTexture, TextureExportTypes.OcclusionMetallicRoughness), index);
            }
            if (occlusionTexture != null && occlusionTexture != metallicSmoothTexture)
            {
                m_exportMap.Add(new ExportKey(occlusionTexture, TextureExportTypes.OcclusionMetallicRoughness), index);
            }

            return index;
        }

        /// <summary>
        /// Normal のテクスチャを変換し index を確定させる
        /// </summary>
        /// <param name="normalTexture"></param>
        /// <returns></returns>
        public int ExportNormal(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, TextureExportTypes.Normal), out var index))
            {
                return index;
            }

            // get Texture2D
            index = Exported.Count;
            var texture2D = src as Texture2D;
            if (m_textureSerializer.CanExportAsEditorAssetFile(texture2D))
            {
                // EditorAsset を使うので変換不要
            }
            else
            {
                // 後で Bitmap を使うために変換する
                texture2D = NormalConverter.Export(src);
            }

            Exported.Add((texture2D, ColorSpace.Linear));
            m_exportMap.Add(new ExportKey(src, TextureExportTypes.Normal), index);

            return index;
        }
    }
}
