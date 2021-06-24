using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// glTF にエクスポートする変換方式を蓄えて index を確定させる。
    /// Exporter の最後で Export() でまとめて変換する。
    /// </summary>
    public sealed class TextureExporter : IDisposable, ITextureExporter
    {
        private readonly ITextureSerializer m_textureSerializer;
        private readonly List<TextureExportParam> _exportingList = new List<TextureExportParam>();

        public TextureExporter(ITextureSerializer textureSerializer)
        {
            m_textureSerializer = textureSerializer;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// 実際にテクスチャを変換する
        /// </summary>
        public List<(Texture2D, ColorSpace)> Export()
        {
            var exportedTextures = new List<(Texture2D, ColorSpace)>();
            for (var idx = 0; idx < _exportingList.Count; ++idx)
            {
                var exporting = _exportingList[idx];
                var texture = exporting.Creator();
                exportedTextures.Add((texture, exporting.ExportColorSpace));
            }
            return exportedTextures;
        }

        public int RegisterExportingAsSRgb(Texture src, bool needsAlpha)
        {
            return RegisterExportingSimple(src, needsAlpha, isLinear: false);
        }

        public int RegisterExportingAsLinear(Texture src, bool needsAlpha)
        {
            return RegisterExportingSimple(src, needsAlpha, isLinear: true);
        }

        private int RegisterExportingSimple(Texture src, bool needsAlpha, bool isLinear)
        {
            if (src == null)
            {
                return -1;
            }

            var exportType = isLinear ? TextureExportTypes.Linear : TextureExportTypes.Srgb;
            var colorSpace = isLinear ? ColorSpace.Linear : ColorSpace.sRGB;

            var param = new TextureExportParam(exportType, colorSpace, src, default, default,
                needsAlpha, () => ConvertTextureSimple(src, needsAlpha, colorSpace));

            if (TryGetExistsParam(param, out var existsIdx))
            {
                // already marked as exporting
                var cached = _exportingList[existsIdx];

                if (needsAlpha && !cached.NeedsAlpha)
                {
                    // アルファチャンネルを必要とする使用用途が表れたため、アルファチャンネル付きで出力するように上書きする
                    _exportingList[existsIdx] = param;
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
                _exportingList.Add(param);
                return _exportingList.Count - 1;
            }
        }

        public int RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture == null && occlusionTexture == null)
            {
                return -1;
            }

            var param = new TextureExportParam(TextureExportTypes.OcclusionMetallicRoughness, ColorSpace.Linear,
                metallicSmoothTexture, occlusionTexture, smoothness, false,
                () => OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture));
            if (TryGetExistsParam(param, out var existsIdx))
            {
                // Return cacehd
                return existsIdx;
            }
            else
            {
                // Add
                _exportingList.Add(param);
                return _exportingList.Count - 1;
            }
        }

        public int RegisterExportingAsNormal(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            var param = new TextureExportParam(TextureExportTypes.Normal, ColorSpace.Linear, src, default, default,
                false, () => NormalConverter.Export(src));
            if (TryGetExistsParam(param, out var existsIdx))
            {
                // Return cached;
                return existsIdx;
            }
            else
            {
                // Add
                // NormalMap Property のテクスチャは必ず NormalMap として解釈してコピーする。
                // Texture Asset の設定に依らず、Standard Shader で得られる見た目と同じ結果を得るため。
                _exportingList.Add(param);
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

        private bool TryGetExistsParam(TextureExportParam param, out int existsIdx)
        {
            existsIdx = _exportingList.FindIndex(param.EqualsAsKey);
            return existsIdx != -1;
        }
    }
}
