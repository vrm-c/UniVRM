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
    public sealed class TextureExporter : IDisposable, ITextureExporter
    {
        private readonly ITextureSerializer m_textureSerializer;
        private readonly Dictionary<ExportKey, int> m_exportMap = new Dictionary<ExportKey, int>();
        private readonly List<(Texture2D, ColorSpace)> m_exported = new List<(Texture2D, ColorSpace)>();

        public IReadOnlyList<(Texture2D, ColorSpace)> Exported => m_exported;

        public TextureExporter(ITextureSerializer textureSerializer)
        {
            m_textureSerializer = textureSerializer;
        }

        public void Dispose()
        {
            // TODO: export 用にコピー・変換したテクスチャーをここで解放したい
        }

        enum ExportTypes
        {
            // sRGB テクスチャとして出力
            Srgb,
            // Linear テクスチャとして出力
            Linear,
            // Unity Standard様式 から glTF PBR様式への変換
            OcclusionMetallicRoughness,
            // Assetを使うときはそのバイト列を無変換で、それ以外は DXT5nm 形式からのデコードを行う
            Normal,
        }

        readonly struct ExportKey
        {
            public readonly Texture Src;
            public readonly ExportTypes TextureType;

            public ExportKey(Texture src, ExportTypes type)
            {
                if (src == null)
                {
                    throw new ArgumentNullException();
                }
                Src = src;
                TextureType = type;
            }
        }

        public int ExportAsSRgb(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, ExportTypes.Srgb), out var index))
            {
                return index;
            }

            // get Texture2D
            index = m_exported.Count;
            var texture2D = src as Texture2D;
            if (m_textureSerializer.CanExportAsEditorAssetFile(texture2D, ColorSpace.sRGB))
            {
                // do nothing
            }
            else
            {
                texture2D = TextureConverter.CopyTexture(src, ColorSpace.sRGB, null);
            }
            m_exported.Add((texture2D, ColorSpace.sRGB));
            m_exportMap.Add(new ExportKey(src, ExportTypes.Srgb), index);

            return index;
        }

        public int ExportAsLinear(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            var exportKey = new ExportKey(src, ExportTypes.Linear);

            // search cache
            if (m_exportMap.TryGetValue(exportKey, out var index))
            {
                return index;
            }

            index = m_exported.Count;
            var texture2d = src as Texture2D;
            if (m_textureSerializer.CanExportAsEditorAssetFile(texture2d, ColorSpace.Linear))
            {
                // do nothing
            }
            else
            {
                texture2d = TextureConverter.CopyTexture(src, ColorSpace.Linear, null);
            }
            m_exported.Add((texture2d, ColorSpace.Linear));
            m_exportMap.Add(exportKey, index);

            return index;
        }

        public int ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture == null && occlusionTexture == null)
            {
                return -1;
            }

            // cache
            // TODO 厳密なチェックをしていない
            if (metallicSmoothTexture != null && m_exportMap.TryGetValue(new ExportKey(metallicSmoothTexture, ExportTypes.OcclusionMetallicRoughness), out var index))
            {
                return index;
            }
            if (occlusionTexture != null && m_exportMap.TryGetValue(new ExportKey(occlusionTexture, ExportTypes.OcclusionMetallicRoughness), out index))
            {
                return index;
            }

            //
            // Unity と glTF で互換性が無いので必ず変換が必用
            //
            index = m_exported.Count;
            var texture2D = OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture);

            m_exported.Add((texture2D, ColorSpace.Linear));
            if (metallicSmoothTexture != null)
            {
                m_exportMap.Add(new ExportKey(metallicSmoothTexture, ExportTypes.OcclusionMetallicRoughness), index);
            }
            if (occlusionTexture != null && occlusionTexture != metallicSmoothTexture)
            {
                m_exportMap.Add(new ExportKey(occlusionTexture, ExportTypes.OcclusionMetallicRoughness), index);
            }

            return index;
        }

        public int ExportAsNormal(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, ExportTypes.Normal), out var index))
            {
                return index;
            }

            index = m_exported.Count;
            // NormalMap Property のテクスチャは必ず NormalMap として解釈してコピーする。
            // Texture Asset の設定に依らず、Standard Shader で得られる見た目と同じ結果を得るため。
            var texture2D = NormalConverter.Export(src);

            m_exported.Add((texture2D, ColorSpace.Linear));
            m_exportMap.Add(new ExportKey(src, ExportTypes.Normal), index);

            return index;
        }
    }
}
