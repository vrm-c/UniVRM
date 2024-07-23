using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MToon
{
    public static partial class Utils
    {
        public const string ShaderName = "VRM/MToon";

        public const string PropVersion = "_MToonVersion";
        public const string PropDebugMode = "_DebugMode";
        public const string PropOutlineWidthMode = "_OutlineWidthMode";
        public const string PropOutlineColorMode = "_OutlineColorMode";
        public const string PropBlendMode = "_BlendMode";
        public const string PropCullMode = "_CullMode";
        public const string PropOutlineCullMode = "_OutlineCullMode";
        public const string PropCutoff = "_Cutoff";
        public const string PropColor = "_Color";
        public const string PropShadeColor = "_ShadeColor";
        public const string PropMainTex = "_MainTex";
        public const string PropShadeTexture = "_ShadeTexture";
        public const string PropBumpScale = "_BumpScale";
        public const string PropBumpMap = "_BumpMap";
        public const string PropReceiveShadowRate = "_ReceiveShadowRate";
        public const string PropReceiveShadowTexture = "_ReceiveShadowTexture";
        public const string PropShadingGradeRate = "_ShadingGradeRate";
        public const string PropShadingGradeTexture = "_ShadingGradeTexture";
        public const string PropShadeShift = "_ShadeShift";
        public const string PropShadeToony = "_ShadeToony";
        public const string PropLightColorAttenuation = "_LightColorAttenuation";
        public const string PropIndirectLightIntensity = "_IndirectLightIntensity";
        public const string PropRimColor = "_RimColor";
        public const string PropRimTexture = "_RimTexture";
        public const string PropRimLightingMix = "_RimLightingMix";
        public const string PropRimFresnelPower = "_RimFresnelPower";
        public const string PropRimLift = "_RimLift";
        public const string PropSphereAdd = "_SphereAdd";
        public const string PropEmissionColor = "_EmissionColor";
        public const string PropEmissionMap = "_EmissionMap";
        public const string PropOutlineWidthTexture = "_OutlineWidthTexture";
        public const string PropOutlineWidth = "_OutlineWidth";
        public const string PropOutlineScaledMaxDistance = "_OutlineScaledMaxDistance";
        public const string PropOutlineColor = "_OutlineColor";
        public const string PropOutlineLightingMix = "_OutlineLightingMix";
        public const string PropUvAnimMaskTexture = "_UvAnimMaskTexture";
        public const string PropUvAnimScrollX = "_UvAnimScrollX";
        public const string PropUvAnimScrollY = "_UvAnimScrollY";
        public const string PropUvAnimRotation = "_UvAnimRotation";
        public const string PropSrcBlend = "_SrcBlend";
        public const string PropDstBlend = "_DstBlend";
        public const string PropZWrite = "_ZWrite";
        public const string PropAlphaToMask = "_AlphaToMask";

        public const string KeyNormalMap = "_NORMALMAP";
        public const string KeyAlphaTestOn = "_ALPHATEST_ON";
        public const string KeyAlphaBlendOn = "_ALPHABLEND_ON";
        public const string KeyAlphaPremultiplyOn = "_ALPHAPREMULTIPLY_ON";
        public const string KeyOutlineWidthWorld = "MTOON_OUTLINE_WIDTH_WORLD";
        public const string KeyOutlineWidthScreen = "MTOON_OUTLINE_WIDTH_SCREEN";
        public const string KeyOutlineColorFixed = "MTOON_OUTLINE_COLOR_FIXED";
        public const string KeyOutlineColorMixed = "MTOON_OUTLINE_COLOR_MIXED";
        public const string KeyDebugNormal = "MTOON_DEBUG_NORMAL";
        public const string KeyDebugLitShadeRate = "MTOON_DEBUG_LITSHADERATE";

        public const string TagRenderTypeKey = "RenderType";
        public const string TagRenderTypeValueOpaque = "Opaque";
        public const string TagRenderTypeValueTransparentCutout = "TransparentCutout";
        public const string TagRenderTypeValueTransparent = "Transparent";

        public const int DisabledIntValue = 0;
        public const int EnabledIntValue = 1;
        
        public static RenderQueueRequirement GetRenderQueueRequirement(RenderMode renderMode)
        {
            const int shaderDefaultQueue = -1;
            const int firstTransparentQueue = 2501;
            const int spanOfQueue = 50;
            
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = shaderDefaultQueue,
                        MinValue = shaderDefaultQueue,
                        MaxValue = shaderDefaultQueue,
                    };
                case RenderMode.Cutout:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = (int) RenderQueue.AlphaTest,
                        MinValue = (int) RenderQueue.AlphaTest,
                        MaxValue = (int) RenderQueue.AlphaTest,
                    };
                case RenderMode.Transparent:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = (int) RenderQueue.Transparent,
                        MinValue = (int) RenderQueue.Transparent - spanOfQueue + 1,
                        MaxValue = (int) RenderQueue.Transparent,
                    };
                case RenderMode.TransparentWithZWrite:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = firstTransparentQueue,
                        MinValue = firstTransparentQueue,
                        MaxValue = firstTransparentQueue + spanOfQueue - 1,
                    };
                default:
                    throw new ArgumentOutOfRangeException("renderMode", renderMode, null);
            }
        }
        
    }
}