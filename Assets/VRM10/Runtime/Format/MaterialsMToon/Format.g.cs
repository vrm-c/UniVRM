// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_materials_mtoon
{

    public class TextureInfo
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The index of the texture.
        public int? Index;

        // The set index of texture's TEXCOORD attribute used for texture coordinate mapping.
        public int? TexCoord;
    }

    public class ShadingShiftTextureInfo
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The index of the texture.
        public int? Index;

        // The set index of texture's TEXCOORD attribute used for texture coordinate mapping.
        public int? TexCoord;

        // The scalar multiplier applied to the texture.
        public float? Scale;
    }

    public enum OutlineWidthMode
    {
        none,
        worldCoordinates,
        screenCoordinates,

    }

    public class VRMC_materials_mtoon
    {
        public const string ExtensionName = "VRMC_materials_mtoon";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specification version of VRMC_materials_mtoon
        public string SpecVersion;

        // enable depth buffer when `alphaMode` is `BLEND`
        public bool? TransparentWithZWrite;

        public int? RenderQueueOffsetNumber;

        public float[] ShadeColorFactor;

        public TextureInfo ShadeMultiplyTexture;

        // Lighting
        public float? ShadingShiftFactor;

        // Reference to a texture.
        public ShadingShiftTextureInfo ShadingShiftTexture;

        public float? ShadingToonyFactor;

        public float? GiEqualizationFactor;

        public float[] MatcapFactor;

        // MatCap
        public TextureInfo MatcapTexture;

        // Rim
        public float[] ParametricRimColorFactor;

        public TextureInfo RimMultiplyTexture;

        public float? RimLightingMixFactor;

        public float? ParametricRimFresnelPowerFactor;

        public float? ParametricRimLiftFactor;

        // Outline
        public OutlineWidthMode OutlineWidthMode;

        public float? OutlineWidthFactor;

        public TextureInfo OutlineWidthMultiplyTexture;

        public float[] OutlineColorFactor;

        public float? OutlineLightingMixFactor;

        public TextureInfo UvAnimationMaskTexture;

        public float? UvAnimationScrollXSpeedFactor;

        public float? UvAnimationScrollYSpeedFactor;

        public float? UvAnimationRotationSpeedFactor;
    }
}
