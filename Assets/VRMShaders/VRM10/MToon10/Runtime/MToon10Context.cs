using UnityEngine;

namespace VRMShaders.VRM10.MToon10.Runtime
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
        }

        public MToon10TransparentWithZWriteMode TransparentWithZWriteMode
        {
            get => (MToon10TransparentWithZWriteMode) _material.GetInt(MToon10Prop.TransparentWithZWrite);
        }

        public float AlphaCutoff
        {
            get => _material.GetFloat(MToon10Prop.AlphaCutoff);
        }

        public int RenderQueueOffsetNumber
        {
            get => _material.GetInt(MToon10Prop.RenderQueueOffsetNumber);
        }

        public MToon10DoubleSidedMode DoubleSidedMode
        {
            get => (MToon10DoubleSidedMode) _material.GetInt(MToon10Prop.DoubleSided);
        }

        // Lighting
        public Color BaseColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.BaseColorFactor);
        }

        public Texture BaseColorTexture
        {
            get => _material.GetTexture(MToon10Prop.BaseColorTexture);
        }

        public Color ShadeColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.ShadeColorFactor);
        }

        public Texture ShadeColorTexture
        {
            get => _material.GetTexture(MToon10Prop.ShadeColorTexture);
        }

        public Texture NormalTexture
        {
            get => _material.GetTexture(MToon10Prop.NormalTexture);
        }

        public float NormalTextureScale
        {
            get => _material.GetFloat(MToon10Prop.NormalTextureScale);
        }

        public float ShadingShiftFactor
        {
            get => _material.GetFloat(MToon10Prop.ShadingShiftFactor);
        }

        public Texture ShadingShiftTexture
        {
            get => _material.GetTexture(MToon10Prop.ShadingShiftTexture);
        }

        public float ShadingShiftTextureScale
        {
            get => _material.GetFloat(MToon10Prop.ShadingShiftTextureScale);
        }

        public float ShadingToonyFactor
        {
            get => _material.GetFloat(MToon10Prop.ShadingToonyFactor);
        }

        // GI
        public float GiEqualizationFactor
        {
            get => _material.GetFloat(MToon10Prop.GiEqualizationFactor);
        }

        // Emission
        public Color EmissiveFactorLinear
        {
            get => _material.GetColor(MToon10Prop.EmissiveFactor).linear;
        }

        public Texture EmissiveTexture
        {
            get => _material.GetTexture(MToon10Prop.EmissiveTexture);
        }
        // Rim Lighting
        public Texture MatcapTexture
        {
            get => _material.GetTexture(MToon10Prop.MatcapTexture);
        }

        public Color ParametricRimColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.ParametricRimColorFactor);
        }

        public float ParametricRimFresnelPowerFactor
        {
            get => _material.GetFloat(MToon10Prop.ParametricRimFresnelPowerFactor);
        }

        public float ParametricRimLiftFactor
        {
            get => _material.GetFloat(MToon10Prop.ParametricRimLiftFactor);
        }

        public Texture RimMultiplyTexture
        {
            get => _material.GetTexture(MToon10Prop.RimMultiplyTexture);
        }

        public float RimLightingMixFactor
        {
            get => _material.GetFloat(MToon10Prop.RimLightingMixFactor);
        }

        // Outline
        public MToon10OutlineMode OutlineWidthMode
        {
            get => (MToon10OutlineMode) _material.GetInt(MToon10Prop.OutlineWidthMode);
        }

        public float OutlineWidthFactor
        {
            get => _material.GetFloat(MToon10Prop.OutlineWidthFactor);
        }

        public Texture OutlineWidthMultiplyTexture
        {
            get => _material.GetTexture(MToon10Prop.OutlineWidthMultiplyTexture);
        }

        public Color OutlineColorFactorSrgb
        {
            get => _material.GetColor(MToon10Prop.OutlineColorFactor);
        }

        public float OutlineLightingMixFactor
        {
            get => _material.GetFloat(MToon10Prop.OutlineLightingMixFactor);
        }

        // UV Animation
        public Texture UvAnimationMaskTexture
        {
            get => _material.GetTexture(MToon10Prop.UvAnimationMaskTexture);
        }

        public float UvAnimationScrollXSpeedFactor
        {
            get => _material.GetFloat(MToon10Prop.UvAnimationScrollXSpeedFactor);
        }

        public float UvAnimationScrollYSpeedFactor
        {
            get => _material.GetFloat(MToon10Prop.UvAnimationScrollYSpeedFactor);
        }

        public float UvAnimationRotationSpeedFactor
        {
            get => _material.GetFloat(MToon10Prop.UvAnimationRotationSpeedFactor);
        }

        // etc
        public Vector2 TextureScale
        {
            get => _material.GetTextureScale(MToon10Prop.BaseColorTexture);
        }

        public Vector2 TextureOffset
        {
            get => _material.GetTextureOffset(MToon10Prop.BaseColorTexture);
        }
    }
}