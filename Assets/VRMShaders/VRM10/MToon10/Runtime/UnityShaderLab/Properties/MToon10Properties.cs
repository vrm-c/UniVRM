using System.Collections.Generic;

namespace VRMShaders.VRM10.MToon10.Runtime
{
    public static class MToon10Properties
    {
        private static readonly Dictionary<MToon10Prop, string> _unityShaderLabNames = new Dictionary<MToon10Prop, string>
        {
            [MToon10Prop.AlphaMode] = "_AlphaMode",
            [MToon10Prop.TransparentWithZWrite] = "_TransparentWithZWrite",
            [MToon10Prop.AlphaCutoff] = "_Cutoff",
            [MToon10Prop.RenderQueueOffsetNumber] = "_RenderQueueOffset",
            [MToon10Prop.DoubleSided] = "_DoubleSided",

            [MToon10Prop.BaseColorFactor] = "_Color",
            [MToon10Prop.BaseColorTexture] = "_MainTex",
            [MToon10Prop.ShadeColorFactor] = "_ShadeColor",
            [MToon10Prop.ShadeColorTexture] = "_ShadeTex",
            [MToon10Prop.NormalTexture] = "_BumpMap",
            [MToon10Prop.NormalTextureScale] = "_BumpScale",
            [MToon10Prop.ShadingShiftFactor] = "_ShadingShiftFactor",
            [MToon10Prop.ShadingShiftTexture] = "_ShadingShiftTex",
            [MToon10Prop.ShadingShiftTextureScale] = "_ShadingShiftTexScale",
            [MToon10Prop.ShadingToonyFactor] = "_ShadingToonyFactor",

            [MToon10Prop.GiEqualizationFactor] = "_GiEqualization",

            [MToon10Prop.EmissiveFactor] = "_EmissionColor",
            [MToon10Prop.EmissiveTexture] = "_EmissionMap",

            [MToon10Prop.MatcapColorFactor] = "_MatcapColor",
            [MToon10Prop.MatcapTexture] = "_MatcapTex",
            [MToon10Prop.ParametricRimColorFactor] = "_RimColor",
            [MToon10Prop.ParametricRimFresnelPowerFactor] = "_RimFresnelPower",
            [MToon10Prop.ParametricRimLiftFactor] = "_RimLift",
            [MToon10Prop.RimMultiplyTexture] = "_RimTex",
            [MToon10Prop.RimLightingMixFactor] = "_RimLightingMix",

            [MToon10Prop.OutlineWidthMode] = "_OutlineWidthMode",
            [MToon10Prop.OutlineWidthFactor] = "_OutlineWidth",
            [MToon10Prop.OutlineWidthMultiplyTexture] = "_OutlineWidthTex",
            [MToon10Prop.OutlineColorFactor] = "_OutlineColor",
            [MToon10Prop.OutlineLightingMixFactor] = "_OutlineLightingMix",

            [MToon10Prop.UvAnimationMaskTexture] = "_UvAnimMaskTex",
            [MToon10Prop.UvAnimationScrollXSpeedFactor] = "_UvAnimScrollXSpeed",
            [MToon10Prop.UvAnimationScrollYSpeedFactor] = "_UvAnimScrollYSpeed",
            [MToon10Prop.UvAnimationRotationSpeedFactor] = "_UvAnimRotationSpeed",

            [MToon10Prop.UnityCullMode] = "_M_CullMode",
            [MToon10Prop.UnitySrcBlend] = "_M_SrcBlend",
            [MToon10Prop.UnityDstBlend] = "_M_DstBlend",
            [MToon10Prop.UnityZWrite] = "_M_ZWrite",
            [MToon10Prop.UnityAlphaToMask] = "_M_AlphaToMask",

            [MToon10Prop.EditorEditMode] = "_M_EditMode",
        };

        public static IReadOnlyDictionary<MToon10Prop, string> UnityShaderLabNames => _unityShaderLabNames;

        public static string ToUnityShaderLabName(this MToon10Prop prop)
        {
            return UnityShaderLabNames[prop];
        }
    }
}