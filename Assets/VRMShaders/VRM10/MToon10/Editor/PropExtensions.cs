using System.Collections.Generic;

namespace VRMShaders.VRM10.MToon10.Editor
{
    public static class PropExtensions
    {
        private static readonly Dictionary<Prop, string> _propertyNames = new Dictionary<Prop, string>
        {
            [Prop.AlphaMode] = "_AlphaMode",
            [Prop.TransparentWithZWrite] = "_TransparentWithZWrite",
            [Prop.AlphaCutoff] = "_Cutoff",
            [Prop.RenderQueueOffsetNumber] = "_RenderQueueOffset",
            [Prop.DoubleSided] = "_DoubleSided",

            [Prop.BaseColorFactor] = "_Color",
            [Prop.BaseColorTexture] = "_MainTex",
            [Prop.ShadeColorFactor] = "_ShadeColor",
            [Prop.ShadeColorTexture] = "_ShadeTex",
            [Prop.NormalTexture] = "_BumpMap",
            [Prop.NormalTextureScale] = "_BumpScale",
            [Prop.ShadingShiftFactor] = "_ShadingShiftFactor",
            [Prop.ShadingShiftTexture] = "_ShadingShiftTex",
            [Prop.ShadingShiftTextureScale] = "_ShadingShiftTexScale",
            [Prop.ShadingToonyFactor] = "_ShadingToonyFactor",

            [Prop.GiEqualizationFactor] = "_GiEqualization",

            [Prop.EmissiveFactor] = "_EmissionColor",
            [Prop.EmissiveTexture] = "_EmissionMap",

            [Prop.MatcapTexture] = "_MatcapTex",
            [Prop.ParametricRimColorFactor] = "_RimColor",
            [Prop.ParametricRimFresnelPowerFactor] = "_RimFresnelPower",
            [Prop.ParametricRimLiftFactor] = "_RimLift",
            [Prop.RimMultiplyTexture] = "_RimTex",
            [Prop.RimLightingMixFactor] = "_RimLightingMix",

            [Prop.OutlineWidthMode] = "_OutlineWidthMode",
            [Prop.OutlineWidthFactor] = "_OutlineWidth",
            [Prop.OutlineWidthMultiplyTexture] = "_OutlineWidthTex",
            [Prop.OutlineColorFactor] = "_OutlineColor",
            [Prop.OutlineLightingMixFactor] = "_OutlineLightingMix",

            [Prop.UvAnimationMaskTexture] = "_UvAnimMaskTex",
            [Prop.UvAnimationScrollXSpeedFactor] = "_UvAnimScrollXSpeed",
            [Prop.UvAnimationScrollYSpeedFactor] = "_UvAnimScrollYSpeed",
            [Prop.UvAnimationRotationSpeedFactor] = "_UvAnimRotationSpeed",

            [Prop.UnityCullMode] = "_M_CullMode",
            [Prop.UnitySrcBlend] = "_M_SrcBlend",
            [Prop.UnityDstBlend] = "_M_DstBlend",
            [Prop.UnityZWrite] = "_M_ZWrite",
            [Prop.UnityAlphaToMask] = "_M_AlphaToMask",
        };

        public static IReadOnlyDictionary<Prop, string> PropertyNames => _propertyNames;

        public static string ToName(this Prop prop)
        {
            return PropertyNames[prop];
        }
    }
}