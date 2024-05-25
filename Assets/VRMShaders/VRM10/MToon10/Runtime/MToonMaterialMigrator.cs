using System;
using UnityEngine;
using VRMShaders.VRM10.MToon10.Runtime;
using VRMShaders.VRM10.MToon10.Runtime.MToon0X;
using Object = UnityEngine.Object;

namespace VRMShaders.VRM10.MToon10.Editor
{
    public sealed class MToonMaterialMigrator
    {
        public MToonMaterialMigrator()
        {

        }

        public bool TryMigrate(Material material, bool validateShaderName = true)
        {
            var tempMat = new Material(Shader.Find("VRM10/MToon10"));
            try
            {
                if (validateShaderName && material.shader.name != MToon0XUtils.ShaderName)
                {
                    throw new ArgumentException($"The shader of the material is not {MToon0XUtils.ShaderName}");
                }

                var src = MToon0XUtils.GetMToonParametersFromMaterial(material);
                var dst = new MToon10Context(tempMat);

                dst.AlphaMode = src.Rendering.RenderMode switch
                {
                    MToon0XRenderMode.Opaque => MToon10AlphaMode.Opaque,
                    MToon0XRenderMode.Cutout => MToon10AlphaMode.Cutout,
                    MToon0XRenderMode.Transparent => MToon10AlphaMode.Transparent,
                    MToon0XRenderMode.TransparentWithZWrite => MToon10AlphaMode.Transparent,
                    _ => throw new ArgumentOutOfRangeException()
                };
                dst.TransparentWithZWriteMode = src.Rendering.RenderMode switch
                {
                    MToon0XRenderMode.Opaque => MToon10TransparentWithZWriteMode.Off,
                    MToon0XRenderMode.Cutout => MToon10TransparentWithZWriteMode.Off,
                    MToon0XRenderMode.Transparent => MToon10TransparentWithZWriteMode.Off,
                    MToon0XRenderMode.TransparentWithZWrite => MToon10TransparentWithZWriteMode.On,
                    _ => throw new ArgumentOutOfRangeException()
                };
                dst.AlphaCutoff = src.Color.CutoutThresholdValue;
                dst.RenderQueueOffsetNumber = src.Rendering.RenderQueueOffsetNumber; // NOTE: Breaking Change
                dst.DoubleSidedMode = src.Rendering.CullMode switch
                {
                    MToon0XCullMode.Back => MToon10DoubleSidedMode.Off,
                    MToon0XCullMode.Off => MToon10DoubleSidedMode.On,
                    MToon0XCullMode.Front => MToon10DoubleSidedMode.On, // NOTE: Breaking Change
                    _ => throw new ArgumentOutOfRangeException()
                };
                dst.BaseColorFactorSrgb = src.Color.LitColor; // NOTE: gamma -> gamma
                dst.BaseColorTexture = src.Color.LitMultiplyTexture;
                dst.ShadeColorFactorSrgb = src.Color.ShadeColor; // NOTE: gamma -> gamma
                dst.ShadeColorTexture = src.Color.ShadeMultiplyTexture;
                dst.NormalTexture = src.Lighting.Normal.NormalTexture;
                dst.NormalTextureScale = src.Lighting.Normal.NormalScaleValue;
                dst.ShadingShiftFactor = MToon10Migrator.MigrateToShadingShift(src.Lighting.LitAndShadeMixing.ShadingToonyValue, src.Lighting.LitAndShadeMixing.ShadingShiftValue);
                dst.ShadingShiftTexture = src.Lighting.LitAndShadeMixing.LitAndShadeMixingMultiplierMultiplyTexture;
                dst.ShadingShiftTexture = null;
                dst.ShadingShiftTextureScale = 1f;
                dst.ShadingToonyFactor = MToon10Migrator.MigrateToShadingToony(src.Lighting.LitAndShadeMixing.ShadingToonyValue, src.Lighting.LitAndShadeMixing.ShadingShiftValue);
                dst.GiEqualizationFactor = MToon10Migrator.MigrateToGiEqualization(src.Lighting.LightingInfluence.GiIntensityValue);
                dst.EmissiveFactorLinear = src.Emission.EmissionColor; // NOTE: linear -> linear
                dst.EmissiveTexture = src.Emission.EmissionMultiplyTexture;
                dst.MatcapColorFactorSrgb = src.MatCap.AdditiveTexture != null ? new Color(1, 1, 1) : new Color(0, 0, 0); // NOTE: gamma -> gamma
                dst.MatcapTexture = src.MatCap.AdditiveTexture;
                dst.ParametricRimColorFactorSrgb = src.Rim.RimColor; // NOTE: gamma -> gamma
                dst.ParametricRimFresnelPowerFactor = src.Rim.RimFresnelPowerValue;
                dst.ParametricRimLiftFactor = src.Rim.RimLiftValue;
                dst.RimMultiplyTexture = src.Rim.RimMultiplyTexture;
                dst.RimLightingMixFactor = 1.0f; // NOTE: Breaking Change
                dst.OutlineWidthMode = src.Outline.OutlineWidthMode switch
                {
                    MToon0XOutlineWidthMode.None => MToon10OutlineMode.None,
                    MToon0XOutlineWidthMode.WorldCoordinates => MToon10OutlineMode.World,
                    MToon0XOutlineWidthMode.ScreenCoordinates => MToon10OutlineMode.Screen,
                    _ => throw new ArgumentOutOfRangeException()
                };
                const float centimeterToMeter = 0.01f;
                const float oneHundredth = 0.01f;
                dst.OutlineWidthFactor = src.Outline.OutlineWidthMode switch
                {
                    MToon0XOutlineWidthMode.None => 0f,
                    MToon0XOutlineWidthMode.WorldCoordinates => src.Outline.OutlineWidthValue * centimeterToMeter,
                    MToon0XOutlineWidthMode.ScreenCoordinates => src.Outline.OutlineWidthValue * oneHundredth * 0.5f,
                    _ => throw new ArgumentOutOfRangeException()
                };
                dst.OutlineWidthMultiplyTexture = src.Outline.OutlineWidthMultiplyTexture;
                dst.OutlineColorFactorSrgb = src.Outline.OutlineColor; // NOTE: gamma -> gamma
                dst.OutlineLightingMixFactor = src.Outline.OutlineColorMode switch
                {
                    MToon0XOutlineColorMode.FixedColor => 0f,
                    MToon0XOutlineColorMode.MixedLighting => src.Outline.OutlineLightingMixValue,
                    _ => throw new ArgumentOutOfRangeException()
                };
                dst.UvAnimationMaskTexture = src.TextureOption.UvAnimationMaskTexture;
                dst.UvAnimationScrollXSpeedFactor = src.TextureOption.UvAnimationScrollXSpeedValue;
                dst.UvAnimationScrollYSpeedFactor = src.TextureOption.UvAnimationScrollYSpeedValue;
                dst.UvAnimationRotationSpeedFactor = src.TextureOption.UvAnimationRotationSpeedValue;
                dst.TextureScale = src.TextureOption.MainTextureLeftBottomOriginScale;
                dst.TextureOffset = src.TextureOption.MainTextureLeftBottomOriginOffset;

                dst.Validate();

                material.shader = tempMat.shader;
                material.CopyPropertiesFromMaterial(tempMat);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(tempMat);
                }
                else
                {
                    Object.DestroyImmediate(tempMat);
                }
            }
        }
    }
}