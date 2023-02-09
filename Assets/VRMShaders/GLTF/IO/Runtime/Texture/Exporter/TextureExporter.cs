using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// glTF にエクスポートする変換方式を蓄えて index を確定させる。
    /// Exporter の最後で Export() でまとめて変換する。
    /// </summary>
    public sealed class TextureExporter : ITextureExporter
    {
        private readonly ITextureSerializer _textureSerializer;
        private readonly List<TextureExportParam> _exportingList = new List<TextureExportParam>();
        private readonly List<UnityEngine.Texture2D> _disposables = new List<UnityEngine.Texture2D>();

        public TextureExporter(ITextureSerializer textureSerializer)
        {
            _textureSerializer = textureSerializer;
        }

        public void Dispose()
        {
            foreach (var o in _disposables)
            {
                if (Application.isEditor)
                {
                    GameObject.DestroyImmediate(o);
                }
                else
                {
                    GameObject.Destroy(o);
                }
            }
        }

        private void PushDisposable(UnityEngine.Texture2D disposable)
        {
            _disposables.Add(disposable);
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
                var (texture, isDisposable) = exporting.Creator();
                if (isDisposable)
                {
                    PushDisposable(texture);
                }
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
                () =>
                {
                    _textureSerializer.ModifyTextureAssetBeforeExporting(metallicSmoothTexture);
                    _textureSerializer.ModifyTextureAssetBeforeExporting(occlusionTexture);
                    return (OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness,
                        occlusionTexture), true);
                });
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
                false, () =>
                {
                    _textureSerializer.ModifyTextureAssetBeforeExporting(src);
                    return (NormalConverter.Export(src), true);
                });
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

        private (Texture2D, bool IsDisposable) ConvertTextureSimple(Texture src, bool needsAlpha, ColorSpace exportColorSpace)
        {
            // get Texture2D
            var texture2D = src as Texture2D;
            var isDisposable = false;
            if (_textureSerializer.CanExportAsEditorAssetFile(texture2D, exportColorSpace))
            {
                // NOTE: 生のファイルとして出力可能な場合、何もせずそのまま Texture2D を渡す。
                //       ただし、この場合のみ圧縮設定をオフにしなかった場合、挙動としてバグっぽく見えるので、これもオフにする。
                _textureSerializer.ModifyTextureAssetBeforeExporting(src);
            }
            else
            {
                _textureSerializer.ModifyTextureAssetBeforeExporting(src);
                texture2D = TextureConverter.CopyTexture(src, exportColorSpace, needsAlpha, null);
                isDisposable = true;
            }
            return (texture2D, isDisposable);
        }

        private bool TryGetExistsParam(TextureExportParam param, out int existsIdx)
        {
            existsIdx = _exportingList.FindIndex(param.EqualsAsKey);
            return existsIdx != -1;
        }
    }
}
