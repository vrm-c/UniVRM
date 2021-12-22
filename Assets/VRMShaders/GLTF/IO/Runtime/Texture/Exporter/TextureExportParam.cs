using System;
using UnityEngine;

namespace VRMShaders
{
    internal sealed class TextureExportParam
    {
        public TextureExportTypes ExportType { get; }
        public ColorSpace ExportColorSpace { get; }
        public Texture PrimaryTexture { get; }
        public Texture SecondaryTexture { get; }
        public float OptionFactor { get; }

        public bool NeedsAlpha { get; set; }
        public Func<Texture2D> Creator { get; set; }

        public TextureExportParam(TextureExportTypes exportType, ColorSpace exportColorSpace, Texture primaryTexture, Texture secondaryTexture, float optionFactor, bool needsAlpha, Func<Texture2D> creator)
        {
            ExportType = exportType;
            ExportColorSpace = exportColorSpace;
            PrimaryTexture = primaryTexture;
            SecondaryTexture = secondaryTexture;
            OptionFactor = optionFactor;
            NeedsAlpha = needsAlpha;
            Creator = creator;
        }

        public bool EqualsAsKey(TextureExportParam other)
        {
            if (ExportType != other.ExportType) return false;

            switch (ExportType)
            {
                case TextureExportTypes.Srgb:
                case TextureExportTypes.Linear:
                case TextureExportTypes.Normal:
                    return PrimaryTexture == other.PrimaryTexture;
                case TextureExportTypes.OcclusionMetallicRoughness:
                    return PrimaryTexture == other.PrimaryTexture &&
                           SecondaryTexture == other.SecondaryTexture;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}