using UnityEngine;

namespace VRM10.MToon10
{
    public sealed class MToon10Context
    {
        private readonly Material _material;

        public MToon10Context(Material material)
        {
            _material = material;
        }

        public void Validate()
        {
            new MToonValidator(_material).Validate();
        }

        // Rendering
        public MToon10AlphaMode AlphaMode
        {
            get => (MToon10AlphaMode) _material.GetInt(MToon10Prop.AlphaMode);
            set => _material.SetInt(MToon10Prop.AlphaMode, (int) value);
        }

        public MToon10TransparentWithZWriteMode TransparentWithZWriteMode
        {
            get => (MToon10TransparentWithZWriteMode) _material.GetInt(MToon10Prop.TransparentWithZWrite);
            set => _material.SetInt(MToon10Prop.TransparentWithZWrite, (int) value);
        }

        public float AlphaCutoff
        {
            get => _material.GetFloat(MToon10Prop.AlphaCutoff);
            set => _material.SetFloat(MToon10Prop.AlphaCutoff, value);
        }

        public int RenderQueueOffsetNumber
        {
            get => _material.GetInt(MToon10Prop.RenderQueueOffsetNumber);
            set => _material.SetInt(MToon10Prop.RenderQueueOffsetNumber, value);
        }

        public MToon10DoubleSidedMode DoubleSidedMode
        {
            get => (MToon10DoubleSidedMode) _material.GetInt(MToon10Prop.DoubleSided);
            set => _material.SetInt(MToon10Prop.DoubleSided, (int) value);
        }

        // Lighting
        public Color BaseColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.BaseColorFactor);
            set => _material.SetColor(MToon10Prop.BaseColorFactor, value);
        }

        public Texture BaseColorTexture
        {
            get => _material.GetTexture(MToon10Prop.BaseColorTexture);
            set => _material.SetTexture(MToon10Prop.BaseColorTexture, value);
        }

        public Color ShadeColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.ShadeColorFactor);
            set => _material.SetColor(MToon10Prop.ShadeColorFactor, value);
        }

        public Texture ShadeColorTexture
        {
            get => _material.GetTexture(MToon10Prop.ShadeColorTexture);
            set => _material.SetTexture(MToon10Prop.ShadeColorTexture, value);
        }

        public Texture NormalTexture
        {
            get => _material.GetTexture(MToon10Prop.NormalTexture);
            set => _material.SetTexture(MToon10Prop.NormalTexture, value);
        }

        public float NormalTextureScale
        {
            get => _material.GetFloat(MToon10Prop.NormalTextureScale);
            set => _material.SetFloat(MToon10Prop.NormalTextureScale, value);
        }

        public float ShadingShiftFactor
        {
            get => _material.GetFloat(MToon10Prop.ShadingShiftFactor);
            set => _material.SetFloat(MToon10Prop.ShadingShiftFactor, value);
        }

        public Texture ShadingShiftTexture
        {
            get => _material.GetTexture(MToon10Prop.ShadingShiftTexture);
            set => _material.SetTexture(MToon10Prop.ShadingShiftTexture, value);
        }

        public float ShadingShiftTextureScale
        {
            get => _material.GetFloat(MToon10Prop.ShadingShiftTextureScale);
            set => _material.SetFloat(MToon10Prop.ShadingShiftTextureScale, value);
        }

        public float ShadingToonyFactor
        {
            get => _material.GetFloat(MToon10Prop.ShadingToonyFactor);
            set => _material.SetFloat(MToon10Prop.ShadingToonyFactor, value);
        }

        // GI
        public float GiEqualizationFactor
        {
            get => _material.GetFloat(MToon10Prop.GiEqualizationFactor);
            set => _material.SetFloat(MToon10Prop.GiEqualizationFactor, value);
        }

        // Emission
        public Color EmissiveFactorLinear
        {
            // Emissive factor is stored in Linear space
            get => _material.GetColor(MToon10Prop.EmissiveFactor);
            set => _material.SetColor(MToon10Prop.EmissiveFactor, value);
        }

        public Texture EmissiveTexture
        {
            get => _material.GetTexture(MToon10Prop.EmissiveTexture);
            set => _material.SetTexture(MToon10Prop.EmissiveTexture, value);
        }
        // Rim Lighting
        public Color MatcapColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.MatcapColorFactor);
            set => _material.SetColor(MToon10Prop.MatcapColorFactor, value);
        }
        public Texture MatcapTexture
        {
            get => _material.GetTexture(MToon10Prop.MatcapTexture);
            set => _material.SetTexture(MToon10Prop.MatcapTexture, value);
        }

        public Color ParametricRimColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.ParametricRimColorFactor);
            set => _material.SetColor(MToon10Prop.ParametricRimColorFactor, value);
        }

        public float ParametricRimFresnelPowerFactor
        {
            get => _material.GetFloat(MToon10Prop.ParametricRimFresnelPowerFactor);
            set => _material.SetFloat(MToon10Prop.ParametricRimFresnelPowerFactor, value);
        }

        public float ParametricRimLiftFactor
        {
            get => _material.GetFloat(MToon10Prop.ParametricRimLiftFactor);
            set => _material.SetFloat(MToon10Prop.ParametricRimLiftFactor, value);
        }

        public Texture RimMultiplyTexture
        {
            get => _material.GetTexture(MToon10Prop.RimMultiplyTexture);
            set => _material.SetTexture(MToon10Prop.RimMultiplyTexture, value);
        }

        public float RimLightingMixFactor
        {
            get => _material.GetFloat(MToon10Prop.RimLightingMixFactor);
            set => _material.SetFloat(MToon10Prop.RimLightingMixFactor, value);
        }

        // Outline
        public MToon10OutlineMode OutlineWidthMode
        {
            get => (MToon10OutlineMode) _material.GetInt(MToon10Prop.OutlineWidthMode);
            set => _material.SetInt(MToon10Prop.OutlineWidthMode, (int) value);
        }

        public float OutlineWidthFactor
        {
            get => _material.GetFloat(MToon10Prop.OutlineWidthFactor);
            set => _material.SetFloat(MToon10Prop.OutlineWidthFactor, value);
        }

        public Texture OutlineWidthMultiplyTexture
        {
            get => _material.GetTexture(MToon10Prop.OutlineWidthMultiplyTexture);
            set => _material.SetTexture(MToon10Prop.OutlineWidthMultiplyTexture, value);
        }

        public Color OutlineColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.OutlineColorFactor);
            set => _material.SetColor(MToon10Prop.OutlineColorFactor, value);
        }

        public float OutlineLightingMixFactor
        {
            get => _material.GetFloat(MToon10Prop.OutlineLightingMixFactor);
            set => _material.SetFloat(MToon10Prop.OutlineLightingMixFactor, value);
        }

        // UV Animation
        public Texture UvAnimationMaskTexture
        {
            get => _material.GetTexture(MToon10Prop.UvAnimationMaskTexture);
            set => _material.SetTexture(MToon10Prop.UvAnimationMaskTexture, value);
        }

        public float UvAnimationScrollXSpeedFactor
        {
            get => _material.GetFloat(MToon10Prop.UvAnimationScrollXSpeedFactor);
            set => _material.SetFloat(MToon10Prop.UvAnimationScrollXSpeedFactor, value);
        }

        public float UvAnimationScrollYSpeedFactor
        {
            get => _material.GetFloat(MToon10Prop.UvAnimationScrollYSpeedFactor);
            set => _material.SetFloat(MToon10Prop.UvAnimationScrollYSpeedFactor, value);
        }

        public float UvAnimationRotationSpeedFactor
        {
            get => _material.GetFloat(MToon10Prop.UvAnimationRotationSpeedFactor);
            set => _material.SetFloat(MToon10Prop.UvAnimationRotationSpeedFactor, value);
        }

        // etc
        public Vector2 TextureScale
        {
            get => _material.GetTextureScale(MToon10Prop.BaseColorTexture);
            set => _material.SetTextureScale(MToon10Prop.BaseColorTexture, value);
        }

        public Vector2 TextureOffset
        {
            get => _material.GetTextureOffset(MToon10Prop.BaseColorTexture);
            set => _material.SetTextureOffset(MToon10Prop.BaseColorTexture, value);
        }
    }
}