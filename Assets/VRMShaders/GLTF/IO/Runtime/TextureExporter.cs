using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// glTF にエクスポートする Texture2D を蓄えて index を確定させる。
    /// Exporter の最後でまとめて Texture2D から bytes 列を得て出力する。
    /// </summary>
    public sealed class TextureExporter : IDisposable, ITextureExporter
    {
        private readonly ITextureSerializer m_textureSerializer;

        private readonly List<(TextureExportKey key, bool needsAlpha, Func<Texture2D> creator)> _exportingList =
            new List<(TextureExportKey key, bool needsAlpha, Func<Texture2D> creator)>();

        public TextureExporter(ITextureSerializer textureSerializer)
        {
            m_textureSerializer = textureSerializer;
        }

        public void Dispose()
        {
            // TODO: export 用にコピー・変換したテクスチャーをここで解放したい
        }

        /// <summary>
        /// 実際にテクスチャを変換する
        /// </summary>
        public List<(Texture2D, ColorSpace)> Export()
        {
            var exported = new List<(Texture2D, ColorSpace)>();
            for (var idx = 0; idx < _exportingList.Count; ++idx)
            {
                var (key, needsAlpha, creator) = _exportingList[idx];
                var colorSpace = key.TextureType == TextureExportTypes.Srgb ? ColorSpace.sRGB : ColorSpace.Linear;
                var texture = creator();
                exported.Add((creator(), colorSpace));
            }
            return exported;
        }

        public int ExportAsSRgb(Texture src, bool needsAlpha)
        {
            return ExportSimple(src, needsAlpha, isLinear: false);
        }

        public int ExportAsLinear(Texture src, bool needsAlpha)
        {
            return ExportSimple(src, needsAlpha, isLinear: true);
        }

        private int ExportSimple(Texture src, bool needsAlpha, bool isLinear)
        {
            if (src == null)
            {
                return -1;
            }

            var exportType = isLinear ? TextureExportTypes.Linear : TextureExportTypes.Srgb;
            var colorSpace = isLinear ? ColorSpace.Linear : ColorSpace.sRGB;

            var key = new TextureExportKey(src, exportType);
            var existsIdx = _exportingList.FindIndex(x => key.Equals(x.key));
            if (existsIdx != -1)
            {
                // already marked as exporting
                var cached = _exportingList[existsIdx];

                if (needsAlpha && !cached.needsAlpha)
                {
                    // アルファチャンネルを必要とする使用用途が表れたため、アルファチャンネル付きで出力するように上書きする
                    _exportingList[existsIdx] = (cached.key, true, () => ConvertTextureSimple(src, true, colorSpace));
                    return existsIdx;
                }
                else
                {
                    // Return cached
                    return existsIdx;
                }
            }
            else
            {
                // Add
                _exportingList.Add((key, needsAlpha, () => ConvertTextureSimple(src, needsAlpha, colorSpace)));
                return _exportingList.Count - 1;
            }
        }

        public int ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture != null)
            {
                // metallicSmoothness is available
                var key = new TextureExportKey(metallicSmoothTexture, TextureExportTypes.OcclusionMetallicRoughness);
                var existsIdx = _exportingList.FindIndex(x => key.Equals(x.key));
                if (existsIdx != -1)
                {
                    // Return cached
                    return existsIdx;
                }
                else
                {
                    // Add
                    _exportingList.Add((key, false, () => OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture)));
                    return _exportingList.Count - 1;
                }
            }
            else if (occlusionTexture != null)
            {
                // TODO 厳密なチェックをしていない
                // occlusion is available
                var key = new TextureExportKey(occlusionTexture, TextureExportTypes.OcclusionMetallicRoughness);
                var existsIdx = _exportingList.FindIndex(x => key.Equals(x.key));
                if (existsIdx != -1)
                {
                    // Return cached
                    return existsIdx;
                }
                else
                {
                    // Add
                    _exportingList.Add((key, false, () => OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture)));
                    return _exportingList.Count - 1;
                }
            }
            else
            {
                return -1;
            }
        }

        public int ExportAsNormal(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            var key = new TextureExportKey(src, TextureExportTypes.Normal);
            var existsIdx = _exportingList.FindIndex(x => key.Equals(x.key));

            if (existsIdx != -1)
            {
                // Return cached;
                return existsIdx;
            }
            else
            {
                // Add
                // NormalMap Property のテクスチャは必ず NormalMap として解釈してコピーする。
                // Texture Asset の設定に依らず、Standard Shader で得られる見た目と同じ結果を得るため。
                _exportingList.Add((key, false, () => NormalConverter.Export(src)));
                return _exportingList.Count - 1;
            }
        }

        private Texture2D ConvertTextureSimple(Texture src, bool needsAlpha, ColorSpace exportColorSpace)
        {
            // get Texture2D
            var texture2D = src as Texture2D;
            if (m_textureSerializer.CanExportAsEditorAssetFile(texture2D, exportColorSpace))
            {
                // do nothing
            }
            else
            {
                texture2D = TextureConverter.CopyTexture(src, exportColorSpace, needsAlpha, null);
            }
            return texture2D;
        }
    }
}
