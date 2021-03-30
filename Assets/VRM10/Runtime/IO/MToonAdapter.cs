using System;
using System.Collections.Generic;
using VrmLib.MToon;
using VrmLib;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;


namespace UniVRM10
{
    public static class MToonAdapter
    {
        static (string, bool) GetRenderMode(RenderMode mode)
        {
            switch (mode)
            {
                case RenderMode.Opaque: return ("OPAQUE", false);
                case RenderMode.Cutout: return ("MASK", false);
                case RenderMode.Transparent: return ("BLEND", false);
                case RenderMode.TransparentWithZWrite: return ("BLEND", true);
            }

            throw new NotImplementedException();
        }

        public static glTFMaterial MToonToGltf(this MToonMaterial mtoon, List<Texture> textures)
        {
            var material = mtoon.UnlitToGltf(textures);

            var dst = new VRMC_materials_mtoon();

            // Color
            material.pbrMetallicRoughness.baseColorFactor = mtoon.Definition.Color.LitColor.ToFloat4();
            if (mtoon.Definition.Color.LitMultiplyTexture != null)
            {
                material.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                {
                    index = mtoon.Definition.Color.LitMultiplyTexture.ToIndex(textures).Value
                };
            }
            dst.ShadeFactor = mtoon.Definition.Color.ShadeColor.ToFloat3();
            dst.ShadeMultiplyTexture = mtoon.Definition.Color.ShadeMultiplyTexture.ToIndex(textures);
            material.alphaCutoff = mtoon.Definition.Color.CutoutThresholdValue;

            // Outline
            dst.OutlineColorMode = (UniGLTF.Extensions.VRMC_materials_mtoon.OutlineColorMode)mtoon.Definition.Outline.OutlineColorMode;
            dst.OutlineFactor = mtoon.Definition.Outline.OutlineColor.ToFloat3();
            dst.OutlineLightingMixFactor = mtoon.Definition.Outline.OutlineLightingMixValue;
            dst.OutlineScaledMaxDistanceFactor = mtoon.Definition.Outline.OutlineScaledMaxDistanceValue;
            dst.OutlineWidthMode = (UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode)mtoon.Definition.Outline.OutlineWidthMode;
            dst.OutlineWidthFactor = mtoon.Definition.Outline.OutlineWidthValue;
            dst.OutlineWidthMultiplyTexture = mtoon.Definition.Outline.OutlineWidthMultiplyTexture.ToIndex(textures);

            // Emission
            material.emissiveFactor = mtoon.Definition.Emission.EmissionColor.ToFloat3();
            if (mtoon.Definition.Emission.EmissionMultiplyTexture != null)
            {
                material.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                {
                    index = textures.IndexOfNullable(mtoon.Definition.Emission.EmissionMultiplyTexture.Texture).Value
                };
            }

            // Light
            dst.GiIntensityFactor = mtoon.Definition.Lighting.LightingInfluence.GiIntensityValue;
            dst.LightColorAttenuationFactor = mtoon.Definition.Lighting.LightingInfluence.LightColorAttenuationValue;
            dst.ShadingShiftFactor = mtoon.Definition.Lighting.LitAndShadeMixing.ShadingShiftValue;
            dst.ShadingToonyFactor = mtoon.Definition.Lighting.LitAndShadeMixing.ShadingToonyValue;
            if (mtoon.Definition.Lighting.Normal.NormalTexture != null)
            {
                material.normalTexture = new glTFMaterialNormalTextureInfo
                {
                    index = textures.IndexOfNullable(mtoon.Definition.Lighting.Normal.NormalTexture.Texture).Value,
                    scale = mtoon.Definition.Lighting.Normal.NormalScaleValue
                };
            }

            // matcap
            dst.AdditiveTexture = mtoon.Definition.MatCap.AdditiveTexture.ToIndex(textures);

            // rendering
            switch (mtoon.Definition.Rendering.CullMode)
            {
                case CullMode.Back:
                    material.doubleSided = false;
                    break;

                case CullMode.Off:
                    material.doubleSided = true;
                    break;

                case CullMode.Front:
                    // GLTF not support
                    material.doubleSided = false;
                    break;

                default:
                    throw new NotImplementedException();
            }
            (material.alphaMode, dst.TransparentWithZWrite) = GetRenderMode(mtoon.Definition.Rendering.RenderMode);
            dst.RenderQueueOffsetNumber = mtoon.Definition.Rendering.RenderQueueOffsetNumber;

            // rim
            dst.RimFactor = mtoon.Definition.Rim.RimColor.ToFloat3();
            dst.RimMultiplyTexture = mtoon.Definition.Rim.RimMultiplyTexture.ToIndex(textures);
            dst.RimLiftFactor = mtoon.Definition.Rim.RimLiftValue;
            dst.RimFresnelPowerFactor = mtoon.Definition.Rim.RimFresnelPowerValue;
            dst.RimLightingMixFactor = mtoon.Definition.Rim.RimLightingMixValue;

            // texture option
            dst.UvAnimationMaskTexture = mtoon.Definition.TextureOption.UvAnimationMaskTexture.ToIndex(textures);
            dst.UvAnimationRotationSpeedFactor = mtoon.Definition.TextureOption.UvAnimationRotationSpeedValue;
            dst.UvAnimationScrollXSpeedFactor = mtoon.Definition.TextureOption.UvAnimationScrollXSpeedValue;
            dst.UvAnimationScrollYSpeedFactor = mtoon.Definition.TextureOption.UvAnimationScrollYSpeedValue;
            if (material.pbrMetallicRoughness.baseColorTexture != null)
            {
                var offset = mtoon.Definition.TextureOption.MainTextureLeftBottomOriginOffset;
                var scale = mtoon.Definition.TextureOption.MainTextureLeftBottomOriginScale;
                glTF_KHR_texture_transform.Serialize(
                    material.pbrMetallicRoughness.baseColorTexture,
                    (offset.X, offset.Y),
                    (scale.X, scale.Y)
                    );
            }

            UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref material.extensions, dst);

            return material;
        }
    }
}
