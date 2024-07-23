using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;
using VRM10.MToon10.MToon0X;
using ColorSpace = UniGLTF.ColorSpace;
using MToon0XUtils = VRM10.MToon10.MToon0X.MToon0XUtils;

namespace UniVRM10
{
    /// <summary>
    /// vrm-0 の json から vrm-0 の MToon.Definition を生成する。
    ///
    /// Texture2D は作成せずに、直接 index を操作する。
    ///
    /// </summary>
    internal sealed class Vrm0XMToonValue
    {
        public MToon0XDefinition Definition { get; }
        public Dictionary<string, float[]> TextureOffsetScales { get; }
        public Vrm0XMToonTextureIndexMap TextureIndexMap { get; }

        public Vrm0XMToonValue(JsonNode vrmMaterial)
        {
            var definition = new MToon0XDefinition
            {
                Color = new MToon0XColorDefinition { },
                Lighting = new MToon0XLightingDefinition
                {
                    LightingInfluence = new MToon0XLightingInfluenceDefinition { },
                    LitAndShadeMixing = new MToon0XLitAndShadeMixingDefinition { },
                    Normal = new MToon0XNormalDefinition { }
                },
                Emission = new MToon0XEmissionDefinition { },
                MatCap = new MToon0XMatCapDefinition { },
                Meta = new MToon0XMetaDefinition { },
                Outline = new MToon0XOutlineDefinition { },
                Rendering = new MToon0XRenderingDefinition { },
                Rim = new MToon0XRimDefinition { },
                TextureOption = new MToon0XTextureUvCoordsDefinition { }
            };

            var offsetScale = new Dictionary<string, float[]>();
            foreach (var kv in vrmMaterial["vectorProperties"].ObjectItems())
            {
                var key = kv.Key.GetString();
                switch (key)
                {
                    // Lighting
                    case "_Color":
                        definition.Color.LitColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                        break;
                    case "_ShadeColor":
                        definition.Color.ShadeColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                        break;

                    // Emission
                    case "_EmissionColor":
                        definition.Emission.EmissionColor = ToColor(kv.Value, ColorSpace.Linear, ColorSpace.Linear);
                        break;

                    // Rim Lighting
                    case "_RimColor":
                        definition.Rim.RimColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                        break;

                    // Outline
                    case "_OutlineColor":
                        definition.Outline.OutlineColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                        break;

                    // Texture ST
                    case "_MainTex":
                    case "_ShadeTexture":
                    case "_BumpMap":
                    case "_EmissionMap":
                    case "_OutlineWidthTexture":
                    case "_ReceiveShadowTexture":
                    case "_RimTexture":
                    case "_ShadingGradeTexture":
                    case "_SphereAdd":
                    case "_UvAnimMaskTexture":
                        // scale, offset
                        offsetScale.Add(key, ToFloat4(kv.Value));
                        break;

                    default:
                        if (Symbols.VRM_DEVELOP)
                        {
                            Debug.LogWarning($"vectorProperties: {kv.Key}: {kv.Value}");
                        }
                        break;
                }
            }

            foreach (var kv in vrmMaterial["floatProperties"].ObjectItems())
            {
                var value = kv.Value.GetSingle();
                switch (kv.Key.GetString())
                {
                    // Rendering
                    case "_BlendMode":
                        definition.Rendering.RenderMode = (MToon0XRenderMode)(int)value;
                        break;
                    case "_CullMode":
                        definition.Rendering.CullMode = (MToon0XCullMode)(int)value;
                        break;
                    case "_Cutoff":
                        definition.Color.CutoutThresholdValue = value;
                        break;

                    // Lighting
                    case "_BumpScale":
                        definition.Lighting.Normal.NormalScaleValue = value;
                        break;
                    case "_LightColorAttenuation":
                        definition.Lighting.LightingInfluence.LightColorAttenuationValue = value;
                        break;
                    case "_ShadeShift":
                        definition.Lighting.LitAndShadeMixing.ShadingShiftValue = value;
                        break;
                    case "_ShadeToony":
                        definition.Lighting.LitAndShadeMixing.ShadingToonyValue = value;
                        break;
                    case "_ShadingGradeRate":
                        // Not supported
                        break;
                    case "_ReceiveShadowRate":
                        // Not supported
                        break;

                    // GI
                    case "_IndirectLightIntensity":
                        definition.Lighting.LightingInfluence.GiIntensityValue = value;
                        break;

                    // Rim Lighting
                    case "_RimFresnelPower":
                        definition.Rim.RimFresnelPowerValue = value;
                        break;
                    case "_RimLift":
                        definition.Rim.RimLiftValue = value;
                        break;
                    case "_RimLightingMix":
                        definition.Rim.RimLightingMixValue = value;
                        break;

                    // Outline
                    case "_OutlineColorMode":
                        definition.Outline.OutlineColorMode = (MToon0XOutlineColorMode)value;
                        break;
                    case "_OutlineLightingMix":
                        definition.Outline.OutlineLightingMixValue = value;
                        break;
                    case "_OutlineScaledMaxDistance":
                        definition.Outline.OutlineScaledMaxDistanceValue = value;
                        break;
                    case "_OutlineWidth":
                        definition.Outline.OutlineWidthValue = value;
                        break;
                    case "_OutlineWidthMode":
                        if (value > 2)
                        {
                            value = 0;
                        }
                        definition.Outline.OutlineWidthMode = (MToon0XOutlineWidthMode)value;
                        break;

                    // UV Animation
                    case "_UvAnimRotation":
                        definition.TextureOption.UvAnimationRotationSpeedValue = value;
                        break;

                    case "_UvAnimScrollX":
                        definition.TextureOption.UvAnimationScrollXSpeedValue = value;
                        break;

                    case "_UvAnimScrollY":
                        definition.TextureOption.UvAnimationScrollYSpeedValue = value;
                        break;

                    case "_OutlineCullMode":
                    case "_ZWrite":
                    case "_DstBlend":
                    case "_SrcBlend":
                    case "_MToonVersion":
                    case "_DebugMode":
                        // Auto generated
                        break;

                    default:
                        if (Symbols.VRM_DEVELOP)
                        {
                            Debug.LogWarning($"floatProperties: {kv.Key} is unknown");
                        }
                        break;
                }
            }

            var map = new Vrm0XMToonTextureIndexMap();

            foreach (var kv in vrmMaterial["textureProperties"].ObjectItems())
            {
                var index = kv.Value.GetInt32();
                switch (kv.Key.GetString())
                {
                    // Lighting
                    case "_MainTex": map.MainTex = index; break;
                    case "_ShadeTexture": map.ShadeTexture = index; break;
                    case "_BumpMap": map.BumpMap = index; break;
                    case "_ReceiveShadowTexture": map.ReceiveShadowTexture = index; break;
                    case "_ShadingGradeTexture": map.ShadingGradeTexture = index; break;
                    // Emission
                    case "_EmissionMap": map.EmissionMap = index; break;
                    // Rim Lighting
                    case "_RimTexture": map.RimTexture = index; break;
                    case "_SphereAdd": map.SphereAdd = index; break;
                    // Outline
                    case "_OutlineWidthTexture": map.OutlineWidthTexture = index; break;
                    // UV Animation
                    case "_UvAnimMaskTexture": map.UvAnimMaskTexture = index; break;
                    default:
                        if (Symbols.VRM_DEVELOP)
                        {
                            Debug.LogWarning($"textureProperties: {kv.Key} is unknown");
                        }
                        break;
                }
            }

            definition.Rendering.RenderQueueOffsetNumber =
                vrmMaterial["renderQueue"].GetInt32() -
                MToon0XUtils.GetRenderQueueRequirement(definition.Rendering.RenderMode).DefaultValue;

            Definition = definition;
            TextureOffsetScales = offsetScale;
            TextureIndexMap = map;
        }

        private static Color ToColor(JsonNode node, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            return node.ArrayItems().Select(x => ListTreeNodeExtensions.GetSingle(x)).ToArray().ToColor4(srcColorSpace, dstColorSpace);
        }

        private static float[] ToFloat4(JsonNode node)
        {
            return node.ArrayItems().Select(x => x.GetSingle()).ToArray();
        }
    }
}