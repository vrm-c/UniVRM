// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_materials_mtoon
{

    public enum OutlineWidthMode
    {
        none,
        worldCoordinates,
        screenCoordinates,

    }

    public enum OutlineColorMode
    {
        fixedColor,
        mixedLighting,

    }

    public class VRMC_materials_mtoon
    {
        public const string ExtensionName = "VRMC_materials_mtoon";
        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        // Meta
        public string Version;

        // enable depth buffer when renderMode is transparent
        public bool? TransparentWithZWrite;

        public int? RenderQueueOffsetNumber;

        public float[] ShadeFactor;

        public int? ShadeMultiplyTexture;

        // Lighting
        public float? ShadingShiftFactor;

        public float? ShadingToonyFactor;

        public float? LightColorAttenuationFactor;

        public float? GiIntensityFactor;

        // MatCap
        public int? AdditiveTexture;

        // Rim
        public float[] RimFactor;

        public int? RimMultiplyTexture;

        public float? RimLightingMixFactor;

        public float? RimFresnelPowerFactor;

        public float? RimLiftFactor;

        // Outline
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public OutlineWidthMode OutlineWidthMode;

        public float? OutlineWidthFactor;

        public int? OutlineWidthMultiplyTexture;

        public float? OutlineScaledMaxDistanceFactor;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public OutlineColorMode OutlineColorMode;

        public float[] OutlineFactor;

        public float? OutlineLightingMixFactor;

        public int? UvAnimationMaskTexture;

        public float? UvAnimationScrollXSpeedFactor;

        public float? UvAnimationScrollYSpeedFactor;

        public float? UvAnimationRotationSpeedFactor;
    }
}
